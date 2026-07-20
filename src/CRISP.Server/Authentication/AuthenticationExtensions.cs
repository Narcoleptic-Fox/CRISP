using CRISP.Server.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace CRISP.Server.Authentication;

/// <summary>
/// Provides service registration helpers for API authentication schemes.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// The policy scheme that forwards requests to either JWT bearer or API key authentication.
    /// </summary>
    public const string CompositeScheme = "DualAuth";

    /// <summary>
    /// Registers JWT bearer authentication and API key authentication for API requests.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing identity settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // AddMicrosoftIdentityWebApi returns a different builder type,
        // so the Identity chain must be separate.
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CompositeScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddPolicyScheme(CompositeScheme, "JWT or API Key", options =>
        {
            options.ForwardDefaultSelector = SelectAuthenticationScheme;
        })
        .AddApiKeyAuth();

        services.AddAuthentication()
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddInMemoryTokenCaches();

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;
            options.Events ??= new JwtBearerEvents();

            var existingOnTokenValidated = options.Events.OnTokenValidated;
            options.Events.OnTokenValidated = async context =>
            {
                await existingOnTokenValidated(context);

                if (context.Principal is null || context.Result?.Failure is not null)
                    return;

                var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                await BearerTokenRoleClaimsEnricher.AddApplicationRoleClaimAsync(
                    context.Principal,
                    db,
                    context.HttpContext.RequestAborted);
            };
        });

        return services;
    }

    /// <summary>
    /// Registers the API key authentication handler with the authentication builder.
    /// </summary>
    /// <param name="builder">The authentication builder to extend.</param>
    /// <param name="configureOptions">An optional callback for configuring API key authentication options.</param>
    /// <returns>The authentication builder.</returns>
    public static AuthenticationBuilder AddApiKeyAuth(
        this AuthenticationBuilder builder,
        Action<ApiKeyAuthOptions>? configureOptions = null)
    {
        return builder.AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(
            ApiKeyAuthOptions.DefaultScheme,
            configureOptions ?? (_ => { }));
    }

    private static string SelectAuthenticationScheme(HttpContext context)
    {
        return context.Request.Headers.ContainsKey(ApiKeyAuthOptions.HeaderName)
            ? ApiKeyAuthOptions.DefaultScheme
            : JwtBearerDefaults.AuthenticationScheme;
    }
}
