using Crisp.Endpoints;
using Crisp.Metadata;
using Crisp.Commands;
using Crisp.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace Crisp.AspNetCore.Tests.Endpoints;

public class EndpointConventionsTests : TestBase
{
    [Theory]
    [InlineData(typeof(CreateConventionCommand), "POST")]
    [InlineData(typeof(UpdateConventionCommand), "PUT")]
    [InlineData(typeof(DeleteConventionCommand), "DELETE")]
    [InlineData(typeof(GetConventionQuery), "GET")]
    [InlineData(typeof(ListConventionQuery), "GET")]
    public void DetermineHttpMethod_ShouldReturnCorrectMethod(Type commandType, string expectedMethod)
    {
        // Act
        var httpMethod = EndpointConventions.DetermineHttpMethod(commandType);

        // Assert
        httpMethod.Should().Be(expectedMethod);
    }

    [Fact]
    public void DetermineHttpMethod_WithNullType_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EndpointConventions.DetermineHttpMethod(null!));
    }

    [Theory]
    [InlineData(typeof(CreateConventionCommand), "/api/convention/create")]
    [InlineData(typeof(UpdateConventionCommand), "/api/convention/update/{id}")]
    [InlineData(typeof(DeleteConventionCommand), "/api/convention/delete/{id}")]
    [InlineData(typeof(GetConventionQuery), "/api/convention/get/{id}")]
    [InlineData(typeof(ListConventionQuery), "/api/convention/list")]
    public void DetermineRoutePattern_ShouldReturnCorrectPattern(Type commandType, string expectedPattern)
    {
        // Act
        var pattern = EndpointConventions.DetermineRoutePattern(commandType);

        // Assert
        pattern.Should().Be(expectedPattern);
    }

    [Fact]
    public void DetermineRoutePattern_WithNullType_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EndpointConventions.DetermineRoutePattern(null!));
    }

    [Fact]
    public void DetermineRoutePattern_WithCustomRouteAttribute_ShouldUseAttribute()
    {
        // Act
        var pattern = EndpointConventions.DetermineRoutePattern(typeof(CustomRouteCommand));

        // Assert
        pattern.Should().Be("/custom/route/{id}");
    }

    [Fact]
    public void DetermineHttpMethod_WithCustomHttpMethodAttribute_ShouldUseAttribute()
    {
        // Act
        var method = EndpointConventions.DetermineHttpMethod(typeof(CustomHttpMethodCommand));

        // Assert
        method.Should().Be("PATCH");
    }

    [Fact]
    public async Task BindFromRoute_ShouldBindRouteParameters()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.RouteValues.Add("id", "123");
        context.Request.RouteValues.Add("name", "test");

        // Act
        var result = await EndpointConventions.BindFromRoute<RouteTestCommand>(context);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(123);
        result.Name.Should().Be("test");
    }

    [Fact]
    public async Task BindFromRoute_WithMissingParameter_ShouldUseDefault()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.RouteValues.Add("id", "456");

        // Act
        var result = await EndpointConventions.BindFromRoute<RouteTestCommand>(context);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(456);
        result.Name.Should().BeNull();
    }

    [Fact]
    public async Task BindFromRoute_WithNullContext_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            EndpointConventions.BindFromRoute<RouteTestCommand>(null!));
    }

    [Fact]
    public void BindFromRouteAndQuery_ShouldBindBothRouteAndQueryParameters()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.RouteValues.Add("id", "789");
        context.Request.QueryString = new QueryString("?name=query-test&category=sample&isActive=true");

        // Act
        var result = EndpointConventions.BindFromRouteAndQuery<RouteQueryTestCommand>(context);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(789);
        result.Name.Should().Be("query-test");
        result.Category.Should().Be("sample");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void BindFromRouteAndQuery_WithArrayParameter_ShouldBindCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?tags=tag1&tags=tag2&tags=tag3");

        // Act
        var result = EndpointConventions.BindFromRouteAndQuery<ArrayParameterCommand>(context);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().BeEquivalentTo(new[] { "tag1", "tag2", "tag3" });
    }

    [Fact]
    public void BindFromRouteAndQuery_WithNullContext_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            EndpointConventions.BindFromRouteAndQuery<RouteQueryTestCommand>(null!));
    }

    [Theory]
    [InlineData(typeof(CreateConventionCommand), "Create Convention")]
    [InlineData(typeof(UpdateConventionCommand), "Update Convention")]
    [InlineData(typeof(DeleteConventionCommand), "Delete Convention")]
    [InlineData(typeof(GetConventionQuery), "Get Convention")]
    [InlineData(typeof(ListConventionQuery), "List Convention")]
    public void GetSummary_ShouldReturnReadableName(Type type, string expectedSummary)
    {
        // Act
        var summary = EndpointConventions.GetSummary(type);

        // Assert
        summary.Should().Be(expectedSummary);
    }

    [Fact]
    public void GetSummary_WithNullType_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EndpointConventions.GetSummary(null!));
    }

    [Theory]
    [InlineData(typeof(CreateConventionCommand), "Convention")]
    [InlineData(typeof(GetUserQuery), "User")]
    [InlineData(typeof(UpdateProductCommand), "Product")]
    public void ExtractTag_ShouldReturnEntityName(Type type, string expectedTag)
    {
        // Act
        var tag = EndpointConventions.ExtractTag(type);

        // Assert
        tag.Should().Be(expectedTag);
    }

    [Fact]
    public void ExtractTag_WithNullType_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => EndpointConventions.ExtractTag(null!));
    }

    [Fact]
    public void ExtractTag_WithGenericType_ShouldReturnGenericName()
    {
        // Act
        var tag = EndpointConventions.ExtractTag(typeof(GenericCommand<string>));

        // Assert
        tag.Should().Be("Generic");
    }

    // Test classes
    public record CreateConventionCommand(string Name) : ICommand<ConventionResponse>;
    public record UpdateConventionCommand(int Id, string Name) : ICommand<ConventionResponse>;
    public record DeleteConventionCommand(int Id) : ICommand;
    public record GetConventionQuery(int Id) : IQuery<ConventionResponse>;
    public record ListConventionQuery(string? Filter = null) : IQuery<ConventionResponse[]>;
    public record GetUserQuery(int UserId) : IQuery<UserResponse>;
    public record UpdateProductCommand(int ProductId, string Name) : ICommand<ProductResponse>;
    public record GenericCommand<T>(T Value) : ICommand<T>;

    [Route("/custom/route/{id}")]
    public record CustomRouteCommand(int Id) : ICommand<ConventionResponse>;

    [HttpMethod("PATCH")]
    public record CustomHttpMethodCommand(int Id) : ICommand<ConventionResponse>;

    public record RouteTestCommand(int Id, string? Name = null) : ICommand<ConventionResponse>;
    public record RouteQueryTestCommand(int Id, string Name, string? Category = null, bool IsActive = false) : IQuery<ConventionResponse>;
    public record ArrayParameterCommand(string[] Tags) : ICommand<ConventionResponse>;

    public record ConventionResponse(string Result);
    public record UserResponse(string Name);
    public record ProductResponse(string Name);
}