using Crisp.Queries;
using Crisp.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Crisp.AspNetCore.Tests.Endpoints;

public class QueryEndpointTests : TestBase
{
    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        // Act
        var endpoint = new QueryEndpoint<QryTestQuery, QryTestQueryResponse>();

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
        var endpoint = new QueryEndpoint<QryTestQuery, QryTestQueryResponse>("/custom-query");

        // Assert
        endpoint.Pattern.Should().Be("/custom-query");
        endpoint.HttpMethod.Should().Be("GET");
    }

    [Fact]
    public void Constructor_WithNullPattern_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QueryEndpoint<QryTestQuery, QryTestQueryResponse>(null!));
    }

    [Fact]
    public void Map_ShouldRegisterGetEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddCrisp()
            .AddHandlersFromAssembly(typeof(QueryEndpointTests).Assembly);
        
        var app = WebApplication.CreateBuilder().Build();
        app.Services.GetRequiredService<IServiceProvider>();
        
        var endpoint = new QueryEndpoint<QryTestQuery, QryTestQueryResponse>();

        // Act
        var builder = endpoint.Map(app);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Map_WithNullApp_ShouldThrow()
    {
        // Arrange
        var endpoint = new QueryEndpoint<QryTestQuery, QryTestQueryResponse>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.Map(null!));
    }

    [Fact]
    public async Task Handle_ShouldBindFromRouteAndQuery()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<IQueryHandler<QryTestQuery, QryTestQueryResponse>, QryTestQueryHandler>();
            services.AddSingleton<IQueryDispatcher, TestQueryDispatcher>();
        });
        
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api/qrytest?id=123&name=test&category=sample");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkWithResponse()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<IQueryHandler<QryTestQuery, QryTestQueryResponse>, QryTestQueryHandler>();
            services.AddSingleton<IQueryDispatcher, TestQueryDispatcher>();
        });
        
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api/qrytest?id=456&name=example");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<QryTestQueryResponse>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        result.Should().NotBeNull();
        result!.Data.Should().Contain("456");
        result.Data.Should().Contain("example");
    }

    [Fact]
    public async Task Handle_WithMissingRequiredParameter_ShouldReturnBadRequest()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<IQueryHandler<QryTestQuery, QryTestQueryResponse>, QryTestQueryHandler>();
            services.AddSingleton<IQueryDispatcher, TestQueryDispatcher>();
        });
        
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api/qrytest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<IQueryHandler<QryTestQuery, QryTestQueryResponse>, QryTestQueryHandler>();
            services.AddSingleton<IQueryDispatcher, TestQueryDispatcher>();
        });
        
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api/qrytest?id=invalid-id&name=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handle_WithComplexQuery_ShouldBindCorrectly()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<IQueryHandler<QryComplexTestQuery, QryTestQueryResponse>, QryComplexTestQueryHandler>();
            services.AddSingleton<IQueryDispatcher, TestQueryDispatcher>();
        });
        
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api/qrycomplextest?filter=active&sortBy=name&pageSize=10&pageNumber=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<QryTestQueryResponse>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        result.Should().NotBeNull();
        result!.Data.Should().Contain("filter=active");
        result.Data.Should().Contain("sortBy=name");
        result.Data.Should().Contain("pageSize=10");
    }

    [Fact]
    public async Task Handle_WithArrayParameters_ShouldBindCorrectly()
    {
        // Arrange
        using var server = CreateTestServer(services =>
        {
            services.AddSingleton<IQueryHandler<QryArrayTestQuery, QryTestQueryResponse>, QryArrayTestQueryHandler>();
            services.AddSingleton<IQueryDispatcher, TestQueryDispatcher>();
        });
        
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api/qryarraytest?tags=tag1&tags=tag2&tags=tag3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<QryTestQueryResponse>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        result.Should().NotBeNull();
        result!.Data.Should().Contain("tag1,tag2,tag3");
    }

    // Test classes
    public record QryTestQuery(int Id, string Name, string? Category = null) : IQuery<QryTestQueryResponse>;
    public record QryComplexTestQuery(string Filter, string SortBy, int PageSize = 10, int PageNumber = 1) : IQuery<QryTestQueryResponse>;
    public record QryArrayTestQuery(string[] Tags) : IQuery<QryTestQueryResponse>;
    public record QryTestQueryResponse(string Data);

    public class QryTestQueryHandler : IQueryHandler<QryTestQuery, QryTestQueryResponse>
    {
        public Task<QryTestQueryResponse> Handle(QryTestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new QryTestQueryResponse($"Query: Id={request.Id}, Name={request.Name}, Category={request.Category}"));
        }
    }

    public class QryComplexTestQueryHandler : IQueryHandler<QryComplexTestQuery, QryTestQueryResponse>
    {
        public Task<QryTestQueryResponse> Handle(QryComplexTestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new QryTestQueryResponse($"Complex query: filter={request.Filter}, sortBy={request.SortBy}, pageSize={request.PageSize}, pageNumber={request.PageNumber}"));
        }
    }

    public class QryArrayTestQueryHandler : IQueryHandler<QryArrayTestQuery, QryTestQueryResponse>
    {
        public Task<QryTestQueryResponse> Handle(QryArrayTestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new QryTestQueryResponse($"Array query: tags={string.Join(",", request.Tags)}"));
        }
    }

    public class TestQueryDispatcher : IQueryDispatcher
    {
        public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        {
            return query switch
            {
                QryTestQuery tq => Task.FromResult((TResponse)(object)new QryTestQueryResponse($"Query: Id={tq.Id}, Name={tq.Name}, Category={tq.Category}")),
                QryComplexTestQuery cq => Task.FromResult((TResponse)(object)new QryTestQueryResponse($"Complex query: filter={cq.Filter}, sortBy={cq.SortBy}, pageSize={cq.PageSize}, pageNumber={cq.PageNumber}")),
                QryArrayTestQuery aq => Task.FromResult((TResponse)(object)new QryTestQueryResponse($"Array query: tags={string.Join(",", aq.Tags)}")),
                _ => throw new NotImplementedException($"Handler not implemented for {typeof(TResponse).Name}")
            };
        }
    }
}