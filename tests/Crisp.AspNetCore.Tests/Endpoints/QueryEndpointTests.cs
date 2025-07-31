using Crisp.Endpoints;
using Crisp.Queries;

namespace Crisp.AspNetCore.Tests.Endpoints;

public class QueryEndpointTests : TestBase
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Act
        QueryEndpoint<QryTestQuery, QryTestQueryResponse> endpoint = new();

        // Assert
        endpoint.Pattern.Should().NotBeNullOrEmpty();
        endpoint.HttpMethod.Should().Be("GET");
        endpoint.RequestType.Should().Be(typeof(QryTestQuery));
        endpoint.ResponseType.Should().Be(typeof(QryTestQueryResponse));
    }

    [Fact]
    public void Constructor_WithCustomPattern_ShouldUseProvided()
    {
        // Act
        QueryEndpoint<QryTestQuery, QryTestQueryResponse> endpoint = new("/custom-query");

        // Assert
        endpoint.Pattern.Should().Be("/custom-query");
        endpoint.HttpMethod.Should().Be("GET");
    }

    [Fact]
    public void Constructor_WithNullPattern_ShouldThrow() =>
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QueryEndpoint<QryTestQuery, QryTestQueryResponse>(null!));

    [Fact]
    public void Map_ShouldRegisterGetEndpoint()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddRouting();
        services.AddCrisp()
            .AddHandlersFromAssembly(typeof(QueryEndpointTests).Assembly);

        WebApplication app = WebApplication.CreateBuilder().Build();
        app.Services.GetRequiredService<IServiceProvider>();

        QueryEndpoint<QryTestQuery, QryTestQueryResponse> endpoint = new();

        // Act
        RouteHandlerBuilder builder = endpoint.Map(app);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Map_WithNullApp_ShouldThrow()
    {
        // Arrange
        QueryEndpoint<QryTestQuery, QryTestQueryResponse> endpoint = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.Map(null!));
    }

    // Test classes
    public record QryTestQuery(int Id, string Name, string? Category = null) : IQuery<QryTestQueryResponse>;
    public record QryComplexTestQuery(string Filter, string SortBy, int PageSize = 10, int PageNumber = 1) : IQuery<QryTestQueryResponse>;
    public record QryArrayTestQuery(string[] Tags) : IQuery<QryTestQueryResponse>;
    public record QryTestQueryResponse(string Data);

    public class QryTestQueryHandler : IQueryHandler<QryTestQuery, QryTestQueryResponse>
    {
        public Task<QryTestQueryResponse> Handle(QryTestQuery request, CancellationToken cancellationToken) => Task.FromResult(new QryTestQueryResponse($"Query: Id={request.Id}, Name={request.Name}, Category={request.Category}"));
    }

    public class QryComplexTestQueryHandler : IQueryHandler<QryComplexTestQuery, QryTestQueryResponse>
    {
        public Task<QryTestQueryResponse> Handle(QryComplexTestQuery request, CancellationToken cancellationToken) => Task.FromResult(new QryTestQueryResponse($"Complex query: filter={request.Filter}, sortBy={request.SortBy}, pageSize={request.PageSize}, pageNumber={request.PageNumber}"));
    }

    public class QryArrayTestQueryHandler : IQueryHandler<QryArrayTestQuery, QryTestQueryResponse>
    {
        public Task<QryTestQueryResponse> Handle(QryArrayTestQuery request, CancellationToken cancellationToken) => Task.FromResult(new QryTestQueryResponse($"Array query: tags={string.Join(",", request.Tags)}"));
    }

    public class TestQueryDispatcher : IQueryDispatcher
    {
        public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default) => query switch
        {
            QryTestQuery tq => Task.FromResult((TResponse)(object)new QryTestQueryResponse($"Query: Id={tq.Id}, Name={tq.Name}, Category={tq.Category}")),
            QryComplexTestQuery cq => Task.FromResult((TResponse)(object)new QryTestQueryResponse($"Complex query: filter={cq.Filter}, sortBy={cq.SortBy}, pageSize={cq.PageSize}, pageNumber={cq.PageNumber}")),
            QryArrayTestQuery aq => Task.FromResult((TResponse)(object)new QryTestQueryResponse($"Array query: tags={string.Join(",", aq.Tags)}")),
            _ => throw new NotImplementedException($"Handler not implemented for {typeof(TResponse).Name}")
        };
    }
}