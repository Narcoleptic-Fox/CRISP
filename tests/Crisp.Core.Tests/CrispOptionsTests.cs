using Crisp;

namespace Crisp.Tests;

public class CrispOptionsTests
{
    [Fact]
    public void Default_ShouldHaveCorrectSettings()
    {
        // Act
        var options = new CrispOptions();

        // Assert
        options.Pipeline.Should().NotBeNull();
        options.Serialization.Should().NotBeNull();
        options.Endpoints.Should().NotBeNull();
    }

    [Fact]
    public void PipelineOptions_ShouldHaveDefaults()
    {
        // Act
        var options = new CrispOptions();

        // Assert
        options.Pipeline.EnableLogging.Should().BeTrue();
        options.Pipeline.EnableErrorHandling.Should().BeTrue();
    }

    [Fact]
    public void EndpointOptions_ShouldHaveDefaults()
    {
        // Act
        var options = new CrispOptions();

        // Assert
        options.Endpoints.AutoDiscoverEndpoints.Should().BeTrue();
        options.Endpoints.EnableOpenApi.Should().BeTrue();
        options.Endpoints.RoutePrefix.Should().Be("/api");
        options.Endpoints.UseKebabCase.Should().BeTrue();
        options.Endpoints.RequireAuthorization.Should().BeFalse();
    }

    [Fact]
    public void SerializationOptions_ShouldHaveDefaults()
    {
        // Act
        var options = new CrispOptions();

        // Assert
        options.Serialization.UseCamelCase.Should().BeTrue();
        options.Serialization.WriteIndented.Should().BeFalse();
        options.Serialization.IgnoreNullValues.Should().BeTrue();
    }
}
