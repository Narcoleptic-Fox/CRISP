using Crisp.Exceptions;

namespace Crisp.Core.Tests.Exceptions;

public class ValidationExceptionTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesExceptionWithDefaultMessage()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("One or more validation failures have occurred.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessage_CreatesExceptionWithSpecifiedMessage()
    {
        // Arrange
        const string expectedMessage = "Custom validation error message";

        // Act
        var exception = new ValidationException(expectedMessage);

        // Assert
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyMessage_CreatesExceptionWithEmptyMessage()
    {
        // Act
        var exception = new ValidationException(string.Empty);

        // Assert
        exception.Message.Should().Be(string.Empty);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullMessage_CreatesExceptionWithNullMessage()
    {
        // Act
        var exception = new ValidationException((string)null!);

        // Assert
        exception.Message.Should().Be("Exception of type 'Crisp.Exceptions.ValidationException' was thrown.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithErrorsCollection_CreatesExceptionWithJoinedErrors()
    {
        // Arrange
        var errors = new List<string>
        {
            "Name is required",
            "Email must be valid",
            "Age must be positive"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Name is required; Email must be valid; Age must be positive");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithSingleError_CreatesExceptionWithSingleError()
    {
        // Arrange
        var errors = new List<string> { "Username is required" };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Username is required");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyErrorsCollection_CreatesExceptionWithEmptyMessage()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be(string.Empty);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullErrorsCollection_ThrowsArgumentNullException()
    {
        // Act & Assert
        FluentActions.Invoking(() => new ValidationException((IEnumerable<string>)null!))
            .Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Error Collection Tests

    [Fact]
    public void Constructor_WithMultipleErrors_JoinsWithSemicolon()
    {
        // Arrange
        var errors = new List<string>
        {
            "Field1 error",
            "Field2 error",
            "Field3 error",
            "Field4 error"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Field1 error; Field2 error; Field3 error; Field4 error");
    }

    [Fact]
    public void Constructor_WithErrorsContainingSemicolons_HandlesCorrectly()
    {
        // Arrange
        var errors = new List<string>
        {
            "Error with; semicolon",
            "Another; error; with; multiple; semicolons"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Error with; semicolon; Another; error; with; multiple; semicolons");
    }

    [Fact]
    public void Constructor_WithErrorsContainingWhitespace_PreservesWhitespace()
    {
        // Arrange
        var errors = new List<string>
        {
            "  Error with leading spaces",
            "Error with trailing spaces  ",
            "  Error with both  "
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("  Error with leading spaces; Error with trailing spaces  ;   Error with both  ");
    }

    [Fact]
    public void Constructor_WithErrorsContainingNewlines_PreservesNewlines()
    {
        // Arrange
        var errors = new List<string>
        {
            "Error line 1\nError line 2",
            "Another\nmultiline\nerror"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Error line 1\nError line 2; Another\nmultiline\nerror");
    }

    [Fact]
    public void Constructor_WithEmptyStringInErrors_IncludesEmptyString()
    {
        // Arrange
        var errors = new List<string>
        {
            "Valid error",
            "",
            "Another valid error"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Valid error; ; Another valid error");
    }

    [Fact]
    public void Constructor_WithNullStringInErrors_HandlesGracefully()
    {
        // Arrange
        var errors = new List<string?>
        {
            "Valid error",
            null,
            "Another valid error"
        };

        // Act
        var exception = new ValidationException(errors!);

        // Assert
        exception.Message.Should().Be("Valid error; ; Another valid error");
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void ValidationException_InheritsFromCrispException()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Should().BeAssignableTo<CrispException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ValidationException_CanBeCaughtAsCrispException()
    {
        // Arrange
        CrispException? caughtException = null;

        // Act
        try
        {
            throw new ValidationException("Validation failed");
        }
        catch (CrispException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<ValidationException>();
    }

    [Fact]
    public void ValidationException_CanBeCaughtAsBaseException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            var errors = new List<string> { "Name is required", "Email is invalid" };
            throw new ValidationException(errors);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<ValidationException>();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithVeryLongErrorMessages_HandlesCorrectly()
    {
        // Arrange
        var longError1 = new string('A', 1000);
        var longError2 = new string('B', 1000);
        var errors = new List<string> { longError1, longError2 };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Contain(longError1);
        exception.Message.Should().Contain(longError2);
        exception.Message.Should().Contain("; ");
    }

    [Fact]
    public void Constructor_WithManyErrors_HandlesCorrectly()
    {
        // Arrange
        var errors = new List<string>();
        for (int i = 0; i < 1000; i++)
        {
            errors.Add($"Error {i}");
        }

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Contain("Error 0");
        exception.Message.Should().Contain("Error 999");
        exception.Message.Should().Contain("; ");
    }

    [Fact]
    public void Constructor_WithUnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        var errors = new List<string>
        {
            "名前が必要です",
            "电子邮件无效",
            "Неверный пароль"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("名前が必要です; 电子邮件无效; Неверный пароль");
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var errors = new List<string>
        {
            "Error with \"quotes\"",
            "Error with 'apostrophe'",
            "Error with <brackets>",
            "Error with &ampersand&"
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Contain("Error with \"quotes\"");
        exception.Message.Should().Contain("Error with 'apostrophe'");
        exception.Message.Should().Contain("Error with <brackets>");
        exception.Message.Should().Contain("Error with &ampersand&");
    }

    #endregion

    #region Data Property Tests

    [Fact]
    public void Exception_CanStoreValidationContext()
    {
        // Arrange
        var errors = new List<string> { "Name is required", "Email is invalid" };
        var exception = new ValidationException(errors);

        // Act
        exception.Data["ValidatedObject"] = "User";
        exception.Data["ValidationRules"] = "Required,EmailFormat";
        exception.Data["FieldsWithErrors"] = new[] { "Name", "Email" };
        exception.Data["Timestamp"] = DateTime.UtcNow;

        // Assert
        exception.Data["ValidatedObject"].Should().Be("User");
        exception.Data["ValidationRules"].Should().Be("Required,EmailFormat");
        exception.Data["FieldsWithErrors"].Should().BeEquivalentTo(new[] { "Name", "Email" });
        exception.Data["Timestamp"].Should().BeOfType<DateTime>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Exception_UsedInValidationScenario_WorksCorrectly()
    {
        // Arrange
        var user = new { Name = "", Email = "invalid-email", Age = -5 };
        ValidationException? caughtException = null;

        // Act - Simulate validation that fails
        try
        {
            SimulateUserValidation(user);
        }
        catch (ValidationException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.Message.Should().Contain("Name is required");
        caughtException.Message.Should().Contain("Email must be valid");
        caughtException.Message.Should().Contain("Age must be positive");
    }

    private static void SimulateUserValidation(object user)
    {
        var errors = new List<string>
        {
            "Name is required",
            "Email must be valid",
            "Age must be positive"
        };
        
        throw new ValidationException(errors);
    }

    [Fact]
    public void Exception_WithNestedValidation_PreservesContext()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            SimulateNestedValidation();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<ValidationException>();
        caughtException!.InnerException.Should().BeNull(); // ValidationException doesn't typically have inner exceptions
        caughtException.Message.Should().Contain("Outer validation failed");
    }

    private static void SimulateNestedValidation()
    {
        var outerErrors = new List<string> { "Outer validation failed" };
        throw new ValidationException(outerErrors);
    }

    [Fact]
    public void Exception_WithValidationResultIntegration_WorksCorrectly()
    {
        // Arrange
        var validationResults = new Dictionary<string, List<string>>
        {
            { "Name", new List<string> { "Required", "MinLength" } },
            { "Email", new List<string> { "InvalidFormat" } },
            { "Age", new List<string> { "Range" } }
        };

        // Act
        var flattenedErrors = validationResults
            .SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}"))
            .ToList();
        
        var exception = new ValidationException(flattenedErrors);

        // Assert
        exception.Message.Should().Contain("Name: Required");
        exception.Message.Should().Contain("Name: MinLength");
        exception.Message.Should().Contain("Email: InvalidFormat");
        exception.Message.Should().Contain("Age: Range");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Constructor_WithLargeErrorCollection_PerformsReasonably()
    {
        // Arrange
        const int errorCount = 10000;
        var errors = new List<string>();
        for (int i = 0; i < errorCount; i++)
        {
            errors.Add($"Validation error {i}: Field {i} is invalid");
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var exception = new ValidationException(errors);
        stopwatch.Stop();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithBasicException_ContainsExpectedInformation()
    {
        // Arrange
        var errors = new List<string> { "Name is required", "Email is invalid" };
        var exception = new ValidationException(errors);

        // Act
        string result = exception.ToString();

        // Assert
        result.Should().Contain("ValidationException");
        result.Should().Contain("Name is required; Email is invalid");
    }

    [Fact]
    public void ToString_WithDefaultMessage_ContainsDefaultMessage()
    {
        // Arrange
        var exception = new ValidationException();

        // Act
        string result = exception.ToString();

        // Assert
        result.Should().Contain("ValidationException");
        result.Should().Contain("One or more validation failures have occurred");
    }

    #endregion

    #region Stack Trace Tests

    [Fact]
    public void StackTrace_WhenThrown_ContainsStackInformation()
    {
        // Arrange & Act
        ValidationException? caughtException = null;
        
        try
        {
            ThrowValidationException();
        }
        catch (ValidationException ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException!.StackTrace.Should().NotBeNullOrEmpty();
        caughtException.StackTrace.Should().Contain(nameof(ThrowValidationException));
    }

    private static void ThrowValidationException()
    {
        var errors = new List<string> { "Test validation error" };
        throw new ValidationException(errors);
    }

    #endregion

    #region Array and IEnumerable Tests

    [Fact]
    public void Constructor_WithErrorsArray_WorksCorrectly()
    {
        // Arrange
        string[] errors = { "Error 1", "Error 2", "Error 3" };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Be("Error 1; Error 2; Error 3");
    }

    [Fact]
    public void Constructor_WithErrorsFromLinq_WorksCorrectly()
    {
        // Arrange
        var sourceErrors = new[] { "error1", "error2", "error3" };
        var processedErrors = sourceErrors.Select(e => e.ToUpper());

        // Act
        var exception = new ValidationException(processedErrors);

        // Assert
        exception.Message.Should().Be("ERROR1; ERROR2; ERROR3");
    }

    #endregion
}