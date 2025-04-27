using CRISP.Core.Extensions;
using CRISP.Core.Responses;
using FluentAssertions;

namespace CRISP.Core.Tests.Extensions;

public class ResponseExtensionsTests
{
    [Fact]
    public void EnsureSuccess_WithSuccessfulGenericResponse_ReturnsData()
    {
        // Arrange
        var data = "Test Data";
        var response = new Response<string>
        {
            Data = data,
            IsSuccess = true,
            Message = "Operation successful"
        };

        // Act
        var result = response.EnsureSuccess();

        // Assert
        result.Should().Be(data);
    }

    [Fact]
    public void EnsureSuccess_WithFailedGenericResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new Response<string>
        {
            Data = null,
            IsSuccess = false,
            Message = "Operation failed",
            Errors = new[] { "Error 1", "Error 2" }
        };

        // Act
        Action act = () => response.EnsureSuccess();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed: Error 1, Error 2");
    }

    [Fact]
    public void EnsureSuccess_WithFailedGenericResponseAndCustomErrorMessage_ThrowsInvalidOperationExceptionWithCustomMessage()
    {
        // Arrange
        var response = new Response<string>
        {
            Data = null,
            IsSuccess = false,
            Message = "Operation failed",
            Errors = new[] { "Error 1", "Error 2" }
        };

        // Act
        Action act = () => response.EnsureSuccess("Custom error message");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Custom error message: Error 1, Error 2");
    }

    [Fact]
    public void EnsureSuccess_WithNonGenericSuccessfulResponse_DoesNotThrow()
    {
        // Arrange
        var response = new Response
        {
            IsSuccess = true,
            Message = "Operation successful"
        };

        // Act
        Action act = () => response.EnsureSuccess();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureSuccess_WithNonGenericFailedResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new Response
        {
            IsSuccess = false,
            Message = "Operation failed",
            Errors = new[] { "Error 1", "Error 2" }
        };

        // Act
        Action act = () => response.EnsureSuccess();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed: Error 1, Error 2");
    }

    [Fact]
    public void EnsureSuccess_WithNonGenericFailedResponseNoErrors_ThrowsInvalidOperationExceptionWithJustMessage()
    {
        // Arrange
        var response = new Response
        {
            IsSuccess = false,
            Message = "Operation failed",
            Errors = null
        };

        // Act
        Action act = () => response.EnsureSuccess();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed");
    }
}