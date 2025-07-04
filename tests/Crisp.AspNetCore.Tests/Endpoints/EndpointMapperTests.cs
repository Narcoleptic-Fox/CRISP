using Crisp.Endpoints;
using Crisp.Commands;
using Crisp.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Crisp.AspNetCore.Tests.Endpoints;

public class EndpointMapperTests : TestBase
{

    [Fact]
    public void DiscoverEndpoints_WithNullAssembly_ShouldThrow()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.DiscoverEndpoints(null!));
    }

    [Fact]
    public void DiscoverEndpoints_WithMultipleAssemblies_ShouldCombineResults()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var assemblies = new[] { typeof(EndpointMapperTests).Assembly, typeof(EndpointMapper).Assembly };

        // Act
        var endpoints = mapper.DiscoverEndpoints(assemblies);

        // Assert
        endpoints.Should().NotBeEmpty();
    }

    [Fact]
    public void MapEndpoints_ShouldRegisterAllDiscoveredEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddCrisp()
            .AddHandlersFromAssembly(typeof(EndpointMapperTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var app = WebApplication.CreateBuilder().Build();

        var mapper = serviceProvider.GetRequiredService<EndpointMapper>();
        var endpoints = mapper.DiscoverEndpoints(typeof(EndpointMapperTests).Assembly);

        // Act
        var routeHandlers = mapper.MapEndpoints(app, endpoints);

        // Assert
        routeHandlers.Should().NotBeEmpty();
        routeHandlers.Count().Should().Be(endpoints.Count());
    }

    [Fact]
    public void MapEndpoints_WithNullApp_ShouldThrow()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var endpoints = new List<IEndpoint>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.MapEndpoints(null!, endpoints));
    }

    [Fact]
    public void MapEndpoints_WithNullEndpoints_ShouldThrow()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var app = WebApplication.CreateBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.MapEndpoints(app, null!));
    }

    [Fact]
    public void MapEndpoints_WithEmptyEndpoints_ShouldReturnEmptyList()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var app = WebApplication.CreateBuilder().Build();
        var endpoints = new List<IEndpoint>();

        // Act
        var routeHandlers = mapper.MapEndpoints(app, endpoints);

        // Assert
        routeHandlers.Should().BeEmpty();
    }

    [Fact]
    public void CreateEndpoint_ForCommand_ShouldCreateCommandEndpoint()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var commandType = typeof(MapperTestCommand);
        var responseType = typeof(MapperTestResponse);

        // Act
        var endpoint = mapper.CreateEndpoint(commandType, responseType);

        // Assert
        endpoint.Should().NotBeNull();
        endpoint.RequestType.Should().Be(commandType);
        endpoint.ResponseType.Should().Be(responseType);
        endpoint.Should().BeAssignableTo<CommandEndpoint<MapperTestCommand, MapperTestResponse>>();
    }

    [Fact]
    public void CreateEndpoint_ForQuery_ShouldCreateQueryEndpoint()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var queryType = typeof(MapperTestQuery);
        var responseType = typeof(MapperTestResponse);

        // Act
        var endpoint = mapper.CreateEndpoint(queryType, responseType);

        // Assert
        endpoint.Should().NotBeNull();
        endpoint.RequestType.Should().Be(queryType);
        endpoint.ResponseType.Should().Be(responseType);
        endpoint.Should().BeAssignableTo<QueryEndpoint<MapperTestQuery, MapperTestResponse>>();
    }

    [Fact]
    public void CreateEndpoint_ForVoidCommand_ShouldCreateVoidCommandEndpoint()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var commandType = typeof(MapperVoidTestCommand);

        // Act
        var endpoint = mapper.CreateEndpoint(commandType, null);

        // Assert
        endpoint.Should().NotBeNull();
        endpoint.RequestType.Should().Be(commandType);
        endpoint.ResponseType.Should().BeNull();
        endpoint.Should().BeAssignableTo<VoidCommandEndpoint<MapperVoidTestCommand>>();
    }

    [Fact]
    public void CreateEndpoint_WithNullCommandType_ShouldThrow()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.CreateEndpoint(null!, typeof(MapperTestResponse)));
    }

    [Fact]
    public void CreateEndpoint_WithInvalidType_ShouldThrow()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var invalidType = typeof(string);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapper.CreateEndpoint(invalidType, typeof(MapperTestResponse)));
    }

    [Fact]
    public void MapCommandEndpoint_ShouldCreateAndRegisterCommandEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddCrisp();

        var app = WebApplication.CreateBuilder().Build();
        var mapper = new EndpointMapper(new CrispOptions());

        // Act
        var routeHandler = mapper.MapCommandEndpoint<MapperTestCommand, MapperTestResponse>(app);

        // Assert
        routeHandler.Should().NotBeNull();
    }

    [Fact]
    public void MapQueryEndpoint_ShouldCreateAndRegisterQueryEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddCrisp();

        var app = WebApplication.CreateBuilder().Build();
        var mapper = new EndpointMapper(new CrispOptions());

        // Act
        var routeHandler = mapper.MapQueryEndpoint<MapperTestQuery, MapperTestResponse>(app);

        // Assert
        routeHandler.Should().NotBeNull();
    }

    [Fact]
    public void GetEndpointMetadata_ShouldReturnCorrectInformation()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());
        var endpoint = mapper.CreateEndpoint(typeof(MapperTestCommand), typeof(MapperTestResponse));

        // Act
        var metadata = mapper.GetEndpointMetadata(endpoint);

        // Assert
        metadata.Should().NotBeNull();
        metadata.RequestType.Should().Be(typeof(MapperTestCommand));
        metadata.ResponseType.Should().Be(typeof(MapperTestResponse));
        metadata.HttpMethod.Should().NotBeNullOrEmpty();
        metadata.RoutePattern.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetEndpointMetadata_WithNullEndpoint_ShouldThrow()
    {
        // Arrange
        var mapper = new EndpointMapper(new CrispOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.GetEndpointMetadata(null!));
    }

    // Test classes
    public record MapperTestCommand(string Value) : ICommand<MapperTestResponse>;
    public record MapperTestQuery(int Id) : IQuery<MapperTestResponse>;
    public record MapperVoidTestCommand(string Action) : ICommand;
    public record MapperTestResponse(string Result);

    public class MapperTestCommandHandler : ICommandHandler<MapperTestCommand, MapperTestResponse>
    {
        public Task<MapperTestResponse> Handle(MapperTestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MapperTestResponse($"Mapped: {request.Value}"));
        }
    }

    public class MapperTestQueryHandler : IQueryHandler<MapperTestQuery, MapperTestResponse>
    {
        public Task<MapperTestResponse> Handle(MapperTestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MapperTestResponse($"Mapped Query: {request.Id}"));
        }
    }

    public class MapperVoidTestCommandHandler : ICommandHandler<MapperVoidTestCommand>
    {
        public Task Handle(MapperVoidTestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}