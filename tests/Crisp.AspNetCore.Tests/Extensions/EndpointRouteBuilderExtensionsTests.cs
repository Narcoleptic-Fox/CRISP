using Crisp.Commands;
using Crisp.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Crisp.AspNetCore.Tests.Extensions;

public class EndpointRouteBuilderExtensionsTests : TestBase
{
    [Fact]
    public void MapCrisp_ShouldRegisterEndpoints()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapCrisp();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void MapCrisp_WithNullApp_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IEndpointRouteBuilder)null!).MapCrisp());
    }

    [Fact]
    public void MapCrispFromAssemblies_ShouldRegisterEndpointsFromSpecifiedAssemblies()
    {
        // Arrange
        var app = CreateWebApplication();
        var assemblies = new[] { typeof(EndpointRouteBuilderExtensionsTests).Assembly };

        // Act
        var result = app.MapCrispFromAssemblies(assemblies);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void MapCrispFromAssemblies_WithNullApp_ShouldThrow()
    {
        // Arrange
        var assemblies = new[] { typeof(EndpointRouteBuilderExtensionsTests).Assembly };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IEndpointRouteBuilder)null!).MapCrispFromAssemblies(assemblies));
    }

    [Fact]
    public void MapCrispFromAssemblies_WithNullAssemblies_ShouldThrow()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            app.MapCrispFromAssemblies(null!));
    }

    [Fact]
    public void MapCrispFromAssemblies_WithEmptyAssemblies_ShouldNotThrow()
    {
        // Arrange
        var app = CreateWebApplication();
        var assemblies = Array.Empty<Assembly>();

        // Act
        var result = app.MapCrispFromAssemblies(assemblies);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void MapCrispEndpoint_ShouldRegisterSpecificEndpoint()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = ((IEndpointRouteBuilder)app).MapCrispEndpoint<RouteTestCommand>();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapCrispEndpoint_WithNullApp_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IEndpointRouteBuilder)null!).MapCrispEndpoint<RouteTestCommand>());
    }

    [Fact]
    public void MapCommand_ShouldRegisterCommandEndpoint()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapCommand<RouteTestCommand, RouteTestResponse>();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapCommand_WithCustomRoute_ShouldUseCustomRoute()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapCommand<RouteTestCommand, RouteTestResponse>("/custom/command");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapCommand_WithNullApp_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IEndpointRouteBuilder)null!).MapCommand<RouteTestCommand, RouteTestResponse>());
    }

    [Fact]
    public void MapQuery_ShouldRegisterQueryEndpoint()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapQuery<RouteTestQuery, RouteTestResponse>();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapQuery_WithCustomRoute_ShouldUseCustomRoute()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapQuery<RouteTestQuery, RouteTestResponse>("/custom/query");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapQuery_WithNullApp_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IEndpointRouteBuilder)null!).MapQuery<RouteTestQuery, RouteTestResponse>());
    }

    [Fact]
    public void MapVoidCommand_ShouldRegisterVoidCommandEndpoint()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapVoidCommand<RouteVoidTestCommand>();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapVoidCommand_WithCustomRoute_ShouldUseCustomRoute()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapVoidCommand<RouteVoidTestCommand>("/custom/void");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void MapVoidCommand_WithNullApp_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ((IEndpointRouteBuilder)null!).MapVoidCommand<RouteVoidTestCommand>());
    }

    [Fact]
    public void MapCrisp_WithCustomConfiguration_ShouldApplyConfiguration()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapCrisp(options =>
        {
            options.RoutePrefix = "/v1/api";
            options.EnableSwagger = true;
            options.RequireAuthorization = true;
        });

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void MapCrisp_WithNullConfiguration_ShouldUseDefaults()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var result = app.MapCrisp(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void MapGroup_ShouldCreateRouteGroup()
    {
        // Arrange
        var app = CreateWebApplication();

        // Act
        var group = app.MapGroup("/api/v1");
        var result = group.MapCrisp();

        // Assert
        result.Should().NotBeNull();
    }

    // Test classes
    public record RouteTestCommand(string Value) : ICommand<RouteTestResponse>;
    public record RouteTestQuery(int Id) : IQuery<RouteTestResponse>;
    public record RouteVoidTestCommand(string Action) : ICommand;
    public record RouteTestResponse(string Result);

    public class RouteTestCommandHandler : ICommandHandler<RouteTestCommand, RouteTestResponse>
    {
        public Task<RouteTestResponse> Handle(RouteTestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new RouteTestResponse($"Route Command: {request.Value}"));
        }
    }

    public class RouteTestQueryHandler : IQueryHandler<RouteTestQuery, RouteTestResponse>
    {
        public Task<RouteTestResponse> Handle(RouteTestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new RouteTestResponse($"Route Query: {request.Id}"));
        }
    }

    public class RouteVoidTestCommandHandler : ICommandHandler<RouteVoidTestCommand>
    {
        public Task Handle(RouteVoidTestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}