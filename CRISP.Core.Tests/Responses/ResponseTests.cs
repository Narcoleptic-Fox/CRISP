using CRISP.Core.Responses;
using FluentAssertions;

namespace CRISP.Core.Tests.Responses;

public class ResponseTests
{
    [Fact]
    public void Response_Generic_Success_SetsPropertiesCorrectly()
    {
        // Arrange
        var data = "Test Data";
        var message = "Custom success message";

        // Act
        var response = Response<string>.Success(data, message);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Response_Generic_Success_UsesDefaultMessage_WhenNotProvided()
    {
        // Arrange
        var data = 42;

        // Act
        var response = Response<int>.Success(data);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be("Operation completed successfully");
        response.Data.Should().Be(data);
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Response_Generic_Failure_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Operation failed";
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var response = Response<string>.Failure(message, errors);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Response_Generic_Failure_WithoutErrors_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Operation failed";

        // Act
        var response = Response<string>.Failure(message);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Response_Non_Generic_Success_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Custom success message";

        // Act
        var response = Response.Success(message);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Response_Non_Generic_Success_UsesDefaultMessage_WhenNotProvided()
    {
        // Act
        var response = Response.Success();

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().Be("Operation completed successfully");
        response.Data.Should().BeNull();
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Response_Non_Generic_Failure_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Operation failed";
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var response = Response.Failure(message, errors);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Response_Non_Generic_Failure_WithoutErrors_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Operation failed";

        // Act
        var response = Response.Failure(message);

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Response_Generic_Constructor_InitializesDefaultValues()
    {
        // Act
        var response = new Response<int>();

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse(); // Default value for bool is false
        response.Message.Should().Be(string.Empty);
        response.Data.Should().Be(default);
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Response_Non_Generic_Constructor_InitializesDefaultValues()
    {
        // Act
        var response = new Response();

        // Assert
        response.Should().NotBeNull();
        response.IsSuccess.Should().BeFalse(); // Default value for bool is false
        response.Message.Should().Be(string.Empty);
        response.Data.Should().BeNull();
        response.Errors.Should().BeNull();
    }
}
