using Crisp.Common;
using Crisp.Exceptions;
using Crisp.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Crisp.AspNetCore.Security;

/// <summary>
/// Pipeline behavior that enforces authorization policies for CRISP requests.
/// Integrates with ASP.NET Core authorization system.
/// </summary>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ClaimsPrincipal? _user;
    private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;

    public AuthorizationBehavior(
        IAuthorizationService authorizationService,
        ClaimsPrincipal? user,
        ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    {
        _authorizationService = authorizationService;
        _user = user;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        Type requestType = typeof(TRequest);
        AuthorizeAttribute? authorizeAttribute = GetAuthorizeAttribute(requestType);

        if (authorizeAttribute == null)
        {
            // No authorization required
            return await next();
        }

        if (_user == null || !_user.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Unauthorized access attempt to {RequestType}", requestType.Name);
            throw new UnauthorizedException($"Authentication required for {requestType.Name}");
        }

        // Check authorization
        AuthorizationResult authResult = string.IsNullOrEmpty(authorizeAttribute.Policy)
            ? await _authorizationService.AuthorizeAsync(_user, null, Array.Empty<IAuthorizationRequirement>())
            : await _authorizationService.AuthorizeAsync(_user, authorizeAttribute.Policy);

        if (!authResult.Succeeded)
        {
            _logger.LogWarning("Authorization failed for user {UserId} accessing {RequestType}. Policies: {Policies}",
                _user.Identity.Name, requestType.Name, authorizeAttribute.Policy);

            throw new UnauthorizedException($"Access denied for {requestType.Name}");
        }

        _logger.LogDebug("Authorization successful for user {UserId} accessing {RequestType}",
            _user.Identity.Name, requestType.Name);

        return await next();
    }

    private static AuthorizeAttribute? GetAuthorizeAttribute(Type requestType) => requestType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();
}

/// <summary>
/// Authorization attribute for CRISP requests.
/// Can be applied to request classes to enforce authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CrispAuthorizeAttribute : AuthorizeAttribute
{
    public CrispAuthorizeAttribute() { }

    public CrispAuthorizeAttribute(string policy) : base(policy) { }
}

/// <summary>
/// Role-based authorization attribute for CRISP requests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CrispRequireRoleAttribute : CrispAuthorizeAttribute
{
    public CrispRequireRoleAttribute(params string[] roles) => Roles = string.Join(",", roles);
}

/// <summary>
/// Claims-based authorization attribute for CRISP requests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CrispRequireClaimAttribute : CrispAuthorizeAttribute
{
    public CrispRequireClaimAttribute(string claimType, string claimValue) => Policy = $"RequireClaim_{claimType}_{claimValue}";
}