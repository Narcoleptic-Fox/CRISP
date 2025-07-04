using Crisp.AspNetCore.Security;
using Crisp.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Crisp.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring CRISP security features.
/// </summary>
public static class SecurityExtensions
{
    /// <summary>
    /// Adds comprehensive security features to the CRISP pipeline.
    /// </summary>
    public static IServiceCollection AddCrispSecurity(this IServiceCollection services, Action<CrispSecurityOptions>? configure = null)
    {
        CrispSecurityOptions options = new();
        configure?.Invoke(options);

        // Register security options
        services.AddSingleton(options);

        // Add core dependencies
        services.AddHttpContextAccessor();
        services.TryAddSingleton(HtmlEncoder.Default);
        services.TryAddSingleton<IRateLimitStore, MemoryRateLimitStore>();

        // Add security behaviors if enabled
        if (options.Authorization.Enabled)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            services.AddScoped<ClaimsPrincipal>(provider =>
            {
                IHttpContextAccessor? httpContextAccessor = provider.GetService<IHttpContextAccessor>();
                return httpContextAccessor?.HttpContext?.User ?? new ClaimsPrincipal();
            });
        }

        if (options.InputSanitization.Enabled)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InputSanitizationBehavior<,>));
            services.AddSingleton(options.InputSanitization);
        }

        if (options.RateLimiting.Enabled)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RateLimitingBehavior<,>));
            services.AddSingleton(options.RateLimiting);
        }

        // Add security audit service
        services.AddSingleton<SecurityAuditService>();
        services.AddSingleton(options.SecurityAudit);

        return services;
    }

    /// <summary>
    /// Adds authorization support to CRISP.
    /// </summary>
    public static IServiceCollection AddCrispAuthorization(this IServiceCollection services, Action<AuthorizationOptions>? configure = null)
    {
        services.AddAuthorization(configure);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped<ClaimsPrincipal>(provider =>
        {
            IHttpContextAccessor? httpContextAccessor = provider.GetService<IHttpContextAccessor>();
            return httpContextAccessor?.HttpContext?.User ?? new ClaimsPrincipal();
        });

        return services;
    }

    /// <summary>
    /// Adds input sanitization to CRISP.
    /// </summary>
    public static IServiceCollection AddCrispInputSanitization(this IServiceCollection services, Action<InputSanitizationOptions>? configure = null)
    {
        InputSanitizationOptions options = new();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.TryAddSingleton(HtmlEncoder.Default);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InputSanitizationBehavior<,>));

        return services;
    }

    /// <summary>
    /// Adds rate limiting to CRISP.
    /// </summary>
    public static IServiceCollection AddCrispRateLimiting(this IServiceCollection services, Action<RateLimitingOptions>? configure = null)
    {
        RateLimitingOptions options = new();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddHttpContextAccessor();
        services.TryAddSingleton<IRateLimitStore, MemoryRateLimitStore>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RateLimitingBehavior<,>));

        return services;
    }

    /// <summary>
    /// Adds distributed rate limiting using Redis or similar.
    /// </summary>
    public static IServiceCollection AddCrispDistributedRateLimiting<TStore>(this IServiceCollection services, Action<RateLimitingOptions>? configure = null)
        where TStore : class, IRateLimitStore
    {
        RateLimitingOptions options = new();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddHttpContextAccessor();
        services.AddSingleton<IRateLimitStore, TStore>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RateLimitingBehavior<,>));

        return services;
    }

    /// <summary>
    /// Adds security auditing to CRISP.
    /// </summary>
    public static IServiceCollection AddCrispSecurityAudit(this IServiceCollection services, Action<SecurityAuditOptions>? configure = null)
    {
        SecurityAuditOptions options = new();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<SecurityAuditService>();

        return services;
    }

    /// <summary>
    /// Configures common authorization policies for CRISP.
    /// </summary>
    public static AuthorizationOptions AddCrispPolicies(this AuthorizationOptions options)
    {
        // Admin policy
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin", "Administrator"));

        // User policy
        options.AddPolicy("AuthenticatedUser", policy =>
            policy.RequireAuthenticatedUser());

        // API access policy
        options.AddPolicy("ApiAccess", policy =>
            policy.RequireClaim("scope", "api"));

        // Read-only policy
        options.AddPolicy("ReadOnly", policy =>
            policy.RequireClaim("permission", "read"));

        // Write policy
        options.AddPolicy("Write", policy =>
            policy.RequireClaim("permission", "write", "admin"));

        // Manager policy
        options.AddPolicy("Manager", policy =>
            policy.RequireRole("Manager", "Admin")
                  .RequireClaim("department"));

        return options;
    }
}

/// <summary>
/// Comprehensive security configuration options for CRISP.
/// </summary>
public class CrispSecurityOptions
{
    /// <summary>
    /// Authorization configuration.
    /// </summary>
    public CrispAuthorizationOptions Authorization { get; set; } = new();

    /// <summary>
    /// Input sanitization configuration.
    /// </summary>
    public InputSanitizationOptions InputSanitization { get; set; } = new();

    /// <summary>
    /// Rate limiting configuration.
    /// </summary>
    public RateLimitingOptions RateLimiting { get; set; } = new();

    /// <summary>
    /// Security audit configuration.
    /// </summary>
    public SecurityAuditOptions SecurityAudit { get; set; } = new();
}

/// <summary>
/// Authorization-specific configuration options.
/// </summary>
public class CrispAuthorizationOptions
{
    /// <summary>
    /// Whether authorization is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Whether to require authentication by default.
    /// </summary>
    public bool RequireAuthenticationByDefault { get; set; } = false;

    /// <summary>
    /// Default authorization policy name.
    /// </summary>
    public string? DefaultPolicy { get; set; }
}