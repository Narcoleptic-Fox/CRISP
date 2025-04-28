using CRISP.Core.Responses;

namespace CRISP.Core.Tests.Extensions;

public class ResponseExtensionsTests
{
    #region EnsureSuccess<T>

    [Fact]
    public void EnsureSuccess_Generic_WithSuccessfulResponse_ReturnsData()
    {
        // Arrange
        string data = "Test Data";
        Response<string> response = Response<string>.Success(data);

        // Act
        string result = response.EnsureSuccess();

        // Assert
        result.ShouldBe(data);
    }

    [Fact]
    public void EnsureSuccess_Generic_WithFailedResponse_ThrowsException()
    {
        // Arrange
        Response<string> response = Response<string>.Failure("Operation failed");

        // Act & Assert
        Func<string> action = () => response.EnsureSuccess();
        InvalidOperationException exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldBe("Operation failed");
    }

    [Fact]
    public void EnsureSuccess_Generic_WithFailedResponseAndErrors_ThrowsException()
    {
        // Arrange
        string[] errors = new[] { "Error 1", "Error 2" };
        Response<string> response = Response<string>.Failure("Operation failed", errors);

        // Act & Assert
        Func<string> action = () => response.EnsureSuccess();
        InvalidOperationException exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldBe("Operation failed: Error 1, Error 2");
    }

    [Fact]
    public void EnsureSuccess_Generic_WithFailedResponseAndCustomErrorMessage_ThrowsException()
    {
        // Arrange
        Response<string> response = Response<string>.Failure("Operation failed");
        string customErrorMessage = "Custom error message";

        // Act & Assert
        Func<string> action = () => response.EnsureSuccess(customErrorMessage);
        InvalidOperationException exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldBe(customErrorMessage);
    }

    #endregion

    #region EnsureSuccess (non-generic)

    [Fact]
    public void EnsureSuccess_NonGeneric_WithSuccessfulResponse_DoesNotThrow()
    {
        // Arrange
        Response response = Response.Success();

        // Act & Assert
        Action action = () => response.EnsureSuccess();
        action.ShouldNotThrow();
    }

    [Fact]
    public void EnsureSuccess_NonGeneric_WithFailedResponse_ThrowsException()
    {
        // Arrange
        Response response = Response.Failure("Operation failed");

        // Act & Assert
        Action action = () => response.EnsureSuccess();
        InvalidOperationException exception = action.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldBe("Operation failed");
    }

    [Fact]
    public void EnsureSuccess_NonGeneric_WithFailedResponseAndErrors_ThrowsException()
    {
        // Arrange
        string[] errors = new[] { "Error 1", "Error 2" };
        Response response = Response.Failure("Operation failed", errors);

        // Act & Assert
        Action action = () => response.EnsureSuccess();
        action.ShouldThrow<InvalidOperationException>().Message
            .ShouldBe("Operation failed: Error 1, Error 2");
    }

    [Fact]
    public void EnsureSuccess_NonGeneric_WithFailedResponseAndCustomErrorMessage_ThrowsException()
    {
        // Arrange
        Response response = Response.Failure("Operation failed");
        string customErrorMessage = "Custom error message";

        // Act & Assert
        Action action = () => response.EnsureSuccess(customErrorMessage);
        action.ShouldThrow<InvalidOperationException>().Message
            .ShouldBe(customErrorMessage);
    }

    #endregion

    #region Map

    [Fact]
    public void Map_WithSuccessfulResponse_MapsData()
    {
        // Arrange
        Response<int> source = Response<int>.Success(42, "Success");
        Func<int, string> mapper = i => i.ToString();

        // Act
        Response<string> result = source.Map(mapper);

        // Assert
        result.ShouldBeOfType<Response<string>>();
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBe("Success");
        result.Data.ShouldBe("42");
        result.Errors.ShouldBeNull();
    }

    [Fact]
    public void Map_WithFailedResponse_DoesNotCallMapper()
    {
        // Arrange
        string[] errors = new[] { "Error 1", "Error 2" };
        Response<int> source = Response<int>.Failure("Failed", errors);
        bool mapperCalled = false;
        Func<int, string> mapper = i => { mapperCalled = true; return i.ToString(); };

        // Act
        Response<string> result = source.Map(mapper);

        // Assert
        mapperCalled.ShouldBeFalse();
        result.ShouldBeOfType<Response<string>>();
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Failed");
        result.Data.ShouldBe(default);
        result.Errors.ShouldBeEquivalentTo(errors);
    }

    [Fact]
    public void Map_WithSuccessfulResponseAndNullData_ReturnsDefaultDestination()
    {
        // Arrange
        Response<string> source = new()
        {
            IsSuccess = true,
            Message = "Success",
            Data = null
        };

        Func<string, int> mapper = s => int.Parse(s);

        // Act
        Response<int> result = source.Map(mapper);

        // Assert
        result.ShouldBeOfType<Response<int>>();
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBe("Success");
        result.Data.ShouldBe(0); // default(int)
        result.Errors.ShouldBeNull();
    }

    #endregion
}
