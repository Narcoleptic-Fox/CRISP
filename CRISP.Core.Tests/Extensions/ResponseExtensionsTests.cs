using CRISP.Core.Extensions;
using CRISP.Core.Responses;
using FluentAssertions;

namespace CRISP.Core.Tests.Extensions;

public class ResponseExtensionsTests
{
    #region EnsureSuccess<T>
    
    [Fact]
    public void EnsureSuccess_Generic_WithSuccessfulResponse_ReturnsData()
    {
        // Arrange
        var data = "Test Data";
        var response = Response<string>.Success(data);

        // Act
        var result = response.EnsureSuccess();

        // Assert
        result.Should().Be(data);
    }

    [Fact]
    public void EnsureSuccess_Generic_WithFailedResponse_ThrowsException()
    {
        // Arrange
        var response = Response<string>.Failure("Operation failed");

        // Act & Assert
        var action = () => response.EnsureSuccess();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed");
    }

    [Fact]
    public void EnsureSuccess_Generic_WithFailedResponseAndErrors_ThrowsException()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };
        var response = Response<string>.Failure("Operation failed", errors);

        // Act & Assert
        var action = () => response.EnsureSuccess();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed: Error 1, Error 2");
    }

    [Fact]
    public void EnsureSuccess_Generic_WithFailedResponseAndCustomErrorMessage_ThrowsException()
    {
        // Arrange
        var response = Response<string>.Failure("Operation failed");
        var customErrorMessage = "Custom error message";

        // Act & Assert
        var action = () => response.EnsureSuccess(customErrorMessage);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage(customErrorMessage);
    }
    
    #endregion
    
    #region EnsureSuccess (non-generic)
    
    [Fact]
    public void EnsureSuccess_NonGeneric_WithSuccessfulResponse_DoesNotThrow()
    {
        // Arrange
        var response = Response.Success();

        // Act & Assert
        var action = () => response.EnsureSuccess();
        action.Should().NotThrow();
    }

    [Fact]
    public void EnsureSuccess_NonGeneric_WithFailedResponse_ThrowsException()
    {
        // Arrange
        var response = Response.Failure("Operation failed");

        // Act & Assert
        var action = () => response.EnsureSuccess();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed");
    }

    [Fact]
    public void EnsureSuccess_NonGeneric_WithFailedResponseAndErrors_ThrowsException()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };
        var response = Response.Failure("Operation failed", errors);

        // Act & Assert
        var action = () => response.EnsureSuccess();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Operation failed: Error 1, Error 2");
    }

    [Fact]
    public void EnsureSuccess_NonGeneric_WithFailedResponseAndCustomErrorMessage_ThrowsException()
    {
        // Arrange
        var response = Response.Failure("Operation failed");
        var customErrorMessage = "Custom error message";

        // Act & Assert
        var action = () => response.EnsureSuccess(customErrorMessage);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage(customErrorMessage);
    }
    
    #endregion
    
    #region Map
    
    [Fact]
    public void Map_WithSuccessfulResponse_MapsData()
    {
        // Arrange
        var source = Response<int>.Success(42, "Success");
        Func<int, string> mapper = i => i.ToString();

        // Act
        var result = source.Map(mapper);

        // Assert
        result.Should().BeOfType<Response<string>>();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Success");
        result.Data.Should().Be("42");
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void Map_WithFailedResponse_DoesNotCallMapper()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };
        var source = Response<int>.Failure("Failed", errors);
        bool mapperCalled = false;
        Func<int, string> mapper = i => { mapperCalled = true; return i.ToString(); };

        // Act
        var result = source.Map(mapper);

        // Assert
        mapperCalled.Should().BeFalse();
        result.Should().BeOfType<Response<string>>();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed");
        result.Data.Should().Be(default);
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Map_WithSuccessfulResponseAndNullData_ReturnsDefaultDestination()
    {
        // Arrange
        var source = new Response<string>
        {
            IsSuccess = true,
            Message = "Success",
            Data = null
        };
        
        Func<string, int> mapper = s => int.Parse(s);

        // Act
        var result = source.Map(mapper);

        // Assert
        result.Should().BeOfType<Response<int>>();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Success");
        result.Data.Should().Be(0); // default(int)
        result.Errors.Should().BeNull();
    }
    
    #endregion
}
