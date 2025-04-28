using CRISP.Core.Responses;

namespace CRISP.Core.Tests.Responses;

public class ResponseTests
{
    [Fact]
    public void Response_Generic_Success_SetsPropertiesCorrectly()
    {
        // Arrange
        string data = "Test Data";
        string message = "Custom success message";

        // Act
        Response<string> response = Response<string>.Success(data, message);

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeTrue();
        response.Message.ShouldBe(message);
        response.Data.ShouldBe(data);
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Response_Generic_Success_UsesDefaultMessage_WhenNotProvided()
    {
        // Arrange
        int data = 42;

        // Act
        Response<int> response = Response<int>.Success(data);

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeTrue();
        response.Message.ShouldBe("Operation completed successfully");
        response.Data.ShouldBe(data);
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Response_Generic_Failure_SetsPropertiesCorrectly()
    {
        // Arrange
        string message = "Operation failed";
        string[] errors = new[] { "Error 1", "Error 2" };

        // Act
        Response<string> response = Response<string>.Failure(message, errors);

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeFalse();
        response.Message.ShouldBe(message);
        response.Data.ShouldBeNull();
        response.Errors.ShouldBe(errors);
    }

    [Fact]
    public void Response_Generic_Failure_WithoutErrors_SetsPropertiesCorrectly()
    {
        // Arrange
        string message = "Operation failed";

        // Act
        Response<string> response = Response<string>.Failure(message);

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeFalse();
        response.Message.ShouldBe(message);
        response.Data.ShouldBeNull();
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Response_Non_Generic_Success_SetsPropertiesCorrectly()
    {
        // Arrange
        string message = "Custom success message";

        // Act
        Response response = Response.Success(message);

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeTrue();
        response.Message.ShouldBe(message);
        response.Data.ShouldBeNull();
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Response_Non_Generic_Success_UsesDefaultMessage_WhenNotProvided()
    {
        // Act
        Response response = Response.Success();

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeTrue();
        response.Message.ShouldBe("Operation completed successfully");
        response.Data.ShouldBeNull();
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Response_Non_Generic_Failure_SetsPropertiesCorrectly()
    {
        // Arrange
        string message = "Operation failed";
        string[] errors = new[] { "Error 1", "Error 2" };

        // Act
        Response response = Response.Failure(message, errors);

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeFalse();
        response.Message.ShouldBe(message);
        response.Data.ShouldBeNull();
        response.Errors.ShouldBe(errors);
    }

    [Fact]
    public void Response_Non_Generic_Failure_WithoutErrors_SetsPropertiesCorrectly()
    {
        // Arrange
        string message = "Operation failed";

        // Act
        Response response = Response.Failure(message);

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeFalse();
        response.Message.ShouldBe(message);
        response.Data.ShouldBeNull();
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Response_Generic_Constructor_InitializesDefaultValues()
    {
        // Act
        Response<int> response = new();

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeFalse(); // Default value for bool is false
        response.Message.ShouldBe(string.Empty);
        response.Data.ShouldBe(default);
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Response_Non_Generic_Constructor_InitializesDefaultValues()
    {
        // Act
        Response response = new();

        // Assert
        response.ShouldNotBeNull();
        response.IsSuccess.ShouldBeFalse(); // Default value for bool is false
        response.Message.ShouldBe(string.Empty);
        response.Data.ShouldBeNull();
        response.Errors.ShouldBeNull();
    }

    [Fact]
    public void Success_WithoutData_CreatesSuccessResponseWithoutData()
    {
        // Act
        Response response = Response.Success();

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.IsSuccess.ShouldBeTrue(),
            () => response.Errors.ShouldBeEmpty()
        );
    }

    [Fact]
    public void Success_WithData_CreatesSuccessResponseWithData()
    {
        // Arrange
        string data = "test data";

        // Act
        Response<string> response = Response<string>.Success(data);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.IsSuccess.ShouldBeTrue(),
            () => response.Data.ShouldBe(data),
            () => response.Errors.ShouldBeEmpty()
        );
    }

    [Fact]
    public void Failure_WithMessageOnly_CreatesFailureWithSingleError()
    {
        // Arrange
        string errorMessage = "Error message";

        // Act
        Response response = Response.Failure(errorMessage);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.IsSuccess.ShouldBeFalse(),
            () => response.Errors!.Count().ShouldBe(1),
            () => response.Errors!.ElementAt(0).ShouldBe(errorMessage)
        );
    }

    [Fact]
    public void Failure_WithMessagesEnumerable_CreatesFailureWithMultipleErrors()
    {
        // Arrange
        List<string> errorMessages = ["Error 1", "Error 2", "Error 3"];

        // Act
        Response response = Response.Failure("Failed", errorMessages);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.IsSuccess.ShouldBeFalse(),
            () => response.Errors!.Count().ShouldBe(errorMessages.Count)
        );

        // Check all error messages are included
        for (int i = 0; i < errorMessages.Count; i++)
        {
            response.Errors!.ElementAt(i).ShouldBe(errorMessages[i]);
        }
    }

    [Fact]
    public void Failure_WithDataAndMessageOnly_CreatesFailureWithDataAndSingleError()
    {
        // Arrange
        string data = "test data";
        string errorMessage = "Error message";

        // Act
        Response<string> response = Response<string>.Failure(data, [errorMessage]);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.IsSuccess.ShouldBeFalse(),
            () => response.Data.ShouldBe(data),
            () => response.Errors!.Count().ShouldBe(1),
            () => response.Errors!.ElementAt(0).ShouldBe(errorMessage)
        );
    }

    [Fact]
    public void Failure_WithDataAndMessagesEnumerable_CreatesFailureWithDataAndMultipleErrors()
    {
        // Arrange
        string data = "test data";
        List<string> errorMessages = ["Error 1", "Error 2", "Error 3"];

        // Act
        Response<string> response = Response<string>.Failure(data, errorMessages);

        // Assert
        response.ShouldSatisfyAllConditions(
            () => response.IsSuccess.ShouldBeFalse(),
            () => response.Data.ShouldBe(data),
            () => response.Errors!.Count().ShouldBe(errorMessages.Count)
        );

        // Check all error messages are included
        for (int i = 0; i < errorMessages.Count; i++)
        {
            response.Errors!.ElementAt(i).ShouldBe(errorMessages[i]);
        }
    }

    [Fact]
    public void WithoutData_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        List<string> errorMessages = ["Error 1", "Error 2"];
        Response response = Response.Failure("Failed", errorMessages);

        // Act
        string? result = response.ToString();

        // Assert
        result.ShouldBe("Response { IsSuccess = False, Errors = [Error 1, Error 2] }");
    }

    [Fact]
    public void WithData_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        string data = "test data";
        List<string> errorMessages = ["Error 1", "Error 2"];
        Response<string> response = Response<string>.Failure(data, errorMessages);

        // Act
        string? result = response.ToString();

        // Assert
        result.ShouldBe("Response<String> { IsSuccess = False, Data = test data, Errors = [Error 1, Error 2] }");
    }
}
