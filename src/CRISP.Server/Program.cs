using Asp.Versioning;
using FluentValidation;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using CRISP.Core.Common;
using CRISP.Server.Authentication;
using CRISP.Server.Authorization;
using CRISP.Server.Data;
using CRISP.Server.Features;
using CRISP.Server.Infrastructure;
using CRISP.Server.Infrastructure.Email;
using CRISP.Server.Infrastructure.Events;
using CRISP.Server.Infrastructure.Storage;
using CRISP.Server.Middleware;
using System.Reflection;

namespace CRISP.Server;

public class Program
{
    private const string CorsPolicyName = "Default";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. DATABASE
        builder.Services.AddDatabaseInfrastructure(builder.Configuration);

        // 2. AUTHENTICATION (dual-scheme: JWT Bearer + API Key)
        builder.Services.AddApiAuthentication(builder.Configuration);

        // 3. AUTHORIZATION (policy-based)
        builder.Services.AddApiAuthorization();

        // 4. RATE LIMITING (chained: per-IP + per-user)
        builder.Services.AddApiRateLimiting(builder.Configuration);

        // 5. CORS (restrictive)
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                var allowedOrigins = builder.Configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? ["https://localhost:7299", "http://localhost:5045", "http://localhost:5000"];

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // 6. EXCEPTION HANDLING (ProblemDetails + IExceptionHandler)
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // 7. SUPPORTING SERVICES
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddEmailInfrastructure(builder.Configuration);
        builder.Services.AddObjectStorageInfrastructure(builder.Configuration);
        builder.Services.AddEventInfrastructure(builder.Configuration);

        builder.Services.AddFeatures();

        // 9. API VERSIONING
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        });

        // 10. FLUENT VALIDATION
        Assembly[] assembliesToRegisterFrom =
            [
                typeof(CreateCommand).Assembly,    // Core Layer
                typeof(Program).Assembly,          // Application Layer
            ];
        builder.Services.AddValidatorsFromAssemblies(assembliesToRegisterFrom);

        // 11. OPENAPI (with security scheme definitions)
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<SecuritySchemeTransformer>();
        });

        // 12. KESTREL: request size limits + TLS on the load-balancer leg
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB

            // In deployed environments the load balancer routes to this container
            // over HTTPS (AWS Security Hub ELB target-group and health-check
            // encryption controls). The HTTPS endpoint itself is defined by the
            // "urls" key (https://0.0.0.0:3000) in appsettings.{Environment}.json;
            // here we supply the server certificate it needs. The ALB does not
            // validate this certificate, so an ephemeral self-signed cert
            // generated in memory is sufficient and keeps the root filesystem
            // read-only.
            if (!builder.Environment.IsDevelopment())
            {
                var targetCertificate = SelfSignedCertificate.CreateForTls();
                options.ConfigureHttpsDefaults(https => https.ServerCertificate = targetCertificate);
            }
        });

        // 13. Health Checks
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        var app = builder.Build();

        // DEFAULTS
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // No app.MapStaticAssets() here on purpose: this server owns no wwwroot assets of its own —
        // both SPAs are served from physical wwwroot/{inventory,calibration} by UseClientApps. Static
        // web assets are disabled in the csproj (two client apps collide at BasePath '/' on publish),
        // so the endpoints manifest MapStaticAssets needs isn't generated. Re-adding it throws at
        // startup: "static resources manifest file ... was not found".

        // MIDDLEWARE PIPELINE (order matters!)
        // Per Microsoft docs: https://learn.microsoft.com/aspnet/core/fundamentals/middleware#middleware-order
        // 1. Correlation ID (outermost - ensures trace ID is set before anything else)
        app.UseCorrelationId();

        // 2. Exception handling (catches everything downstream)
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        // 3. Response compression (before most middleware so responses are compressed)
        app.UseResponseCompression();

        if (app.Environment.IsDevelopment())
        {
            // 4. HTTPS redirection
            app.UseHttpsRedirection();
        }
        else
        {
            app.UseHsts();
        }

        // 5. Security headers (early - applies to all responses including errors)
        app.UseSecurityHeaders();

        // 6. CORS (must be before auth, after routing)
        app.UseCors(CorsPolicyName);

        // 7. Authentication (establishes identity)
        app.UseAuthentication();

        // 8. Request logging (after auth so user identity is available)
        app.UseRequestLogging();

        // 9. Rate limiting (after auth so we can partition by user identity)
        app.UseRateLimiter();

        // 10. Authorization (enforces policies)
        app.UseAuthorization();

        // 11. Output caching (after auth so we can cache)
        app.UseOutputCache();

        // 12. ENDPOINTS (versioned under /api/v{version})
        app.MapFeatures();

        // Unauthenticated health check
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
            .WithTags("Diagnostics")
            .AllowAnonymous()
            .WithName("HealthCheck");

        app.MapHealthChecks("/healthz").AllowAnonymous();

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
