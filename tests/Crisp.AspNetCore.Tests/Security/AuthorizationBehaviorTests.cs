using Crisp.AspNetCore.Security;
using Crisp.Commands;
using Crisp.Exceptions;
using Crisp.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Crisp.AspNetCore.Tests.Security;

public class AuthorizationBehaviorTests : TestBase
{
    [Fact]
    public async Task Handle_AuthorizedUser_CallsNext()
    {
        // Arrange
        var services = CreateServiceProvider(s =>
        {
            s.AddSingleton<IAuthorizationService, MockAuthorizationService>();
        });

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity("test", "name", "role"));
        var behavior = new AuthorizationBehavior<AuthTestCommand, AuthTestResponse>(
            services.GetRequiredService<IAuthorizationService>(),
            authenticatedUser,
            Substitute.For<ILogger<AuthorizationBehavior<AuthTestCommand, AuthTestResponse>>>());

        var request = new AuthTestCommand("test");
        var next = Substitute.For<RequestHandlerDelegate<AuthTestResponse>>();
        next().Returns(new AuthTestResponse("authorized"));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be("authorized");
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorized()
    {
        // Arrange
        var services = CreateServiceProvider(s =>
        {
            s.AddSingleton<IAuthorizationService, MockUnauthorizedService>();
        });

        var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity()); // Not authenticated
        var behavior = new AuthorizationBehavior<AuthTestCommand, AuthTestResponse>(
            services.GetRequiredService<IAuthorizationService>(),
            unauthenticatedUser,
            Substitute.For<ILogger<AuthorizationBehavior<AuthTestCommand, AuthTestResponse>>>());

        var request = new AuthTestCommand("test");
        var next = Substitute.For<RequestHandlerDelegate<AuthTestResponse>>();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            behavior.Handle(request, next, CancellationToken.None));

        await next.Received(0).Invoke();
    }

    [Fact]
    public async Task Handle_NoAuthRequirement_CallsNext()
    {
        // Arrange
        var services = CreateServiceProvider(s =>
        {
            s.AddSingleton<IAuthorizationService, MockAuthorizationService>();
        });

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity("test", "name", "role"));
        var behavior = new AuthorizationBehavior<NoAuthCommand, AuthTestResponse>(
            services.GetRequiredService<IAuthorizationService>(),
            authenticatedUser,
            Substitute.For<ILogger<AuthorizationBehavior<NoAuthCommand, AuthTestResponse>>>());

        var request = new NoAuthCommand("test");
        var next = Substitute.For<RequestHandlerDelegate<AuthTestResponse>>();
        next().Returns(new AuthTestResponse("no auth required"));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be("no auth required");
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_PolicyBased_ValidatesPolicy()
    {
        // Arrange
        var services = CreateServiceProvider(s =>
        {
            s.AddSingleton<IAuthorizationService, MockPolicyAuthorizationService>();
        });

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity("test", "name", "role"));
        var behavior = new AuthorizationBehavior<PolicyTestCommand, AuthTestResponse>(
            services.GetRequiredService<IAuthorizationService>(),
            authenticatedUser,
            Substitute.For<ILogger<AuthorizationBehavior<PolicyTestCommand, AuthTestResponse>>>());

        var request = new PolicyTestCommand("admin-action");
        var next = Substitute.For<RequestHandlerDelegate<AuthTestResponse>>();
        next().Returns(new AuthTestResponse("policy authorized"));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be("policy authorized");
        await next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesToNext()
    {
        // Arrange
        var services = CreateServiceProvider(s =>
        {
            s.AddSingleton<IAuthorizationService, MockAuthorizationService>();
        });

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity("test", "name", "role"));
        var behavior = new AuthorizationBehavior<AuthTestCommand, AuthTestResponse>(
            services.GetRequiredService<IAuthorizationService>(),
            authenticatedUser,
            Substitute.For<ILogger<AuthorizationBehavior<AuthTestCommand, AuthTestResponse>>>());

        var request = new AuthTestCommand("test");
        var next = Substitute.For<RequestHandlerDelegate<AuthTestResponse>>();
        next().Returns(new AuthTestResponse("with token"));

        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        var result = await behavior.Handle(request, next, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        await next.Received(1).Invoke();
    }

    // Test classes
    [Authorize]
    public record AuthTestCommand(string Action) : ICommand<AuthTestResponse>;

    public record NoAuthCommand(string Action) : ICommand<AuthTestResponse>;

    [Authorize(Policy = "AdminPolicy")]
    public record PolicyTestCommand(string Action) : ICommand<AuthTestResponse>;

    public record AuthTestResponse(string Result);

    // Mock services
    public class MockAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }
    }

    public class MockUnauthorizedService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            return Task.FromResult(AuthorizationResult.Failed());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            return Task.FromResult(AuthorizationResult.Failed());
        }
    }

    public class MockPolicyAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            if (policyName == "AdminPolicy")
                return Task.FromResult(AuthorizationResult.Success());
            
            return Task.FromResult(AuthorizationResult.Failed());
        }
    }
}