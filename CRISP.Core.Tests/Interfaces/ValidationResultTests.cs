using CRISP.Core.Interfaces;

namespace CRISP.Core.Tests.Interfaces;

public class ValidationResultTests
{
    [Fact]
    public void Success_ReturnsValidResult()
    {
        // Act
        ValidationResult result = ValidationResult.Success();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Failure_WithErrors_ReturnsInvalidResultWithErrors()
    {
        // Arrange
        ValidationError error1 = new("Name", "Name is required");
        ValidationError error2 = new("Email", "Email is invalid");

        // Act
        ValidationResult result = ValidationResult.Failure(error1, error2);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBe(2);
        result.Errors.ShouldContain(error1);
        result.Errors.ShouldContain(error2);
    }

    [Fact]
    public void Failure_WithMessageAndProperty_ReturnsInvalidResultWithSingleError()
    {
        // Arrange
        string errorMessage = "Value is invalid";
        string propertyName = "TestProperty";

        // Act
        ValidationResult result = ValidationResult.Failure(errorMessage, propertyName);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBe(1);

        ValidationError error = result.Errors.First();
        error.PropertyName.ShouldBe(propertyName);
        error.ErrorMessage.ShouldBe(errorMessage);
    }

    [Fact]
    public void Failure_WithMessageOnly_ReturnsInvalidResultWithSingleError()
    {
        // Arrange
        string errorMessage = "Value is invalid";

        // Act
        ValidationResult result = ValidationResult.Failure(errorMessage);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeNull();
        result.Errors.Count().ShouldBe(1);

        ValidationError error = result.Errors.First();
        error.PropertyName.ShouldBeEmpty();
        error.ErrorMessage.ShouldBe(errorMessage);
    }

    [Fact]
    public void Constructor_DefaultValues_SetsCorrectDefaults()
    {
        // Act
        ValidationResult result = new();

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse(); // Default value of bool is false
        result.Errors.ShouldNotBeNull();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void IValidationResult_Errors_ReturnsReadOnlyList()
    {
        // Arrange
        ValidationError[] errors = new[]
        {
            new ValidationError("Name", "Name is required"),
            new ValidationError("Email", "Email is invalid")
        };
        ValidationResult result = ValidationResult.Failure(errors);

        // Act
        IValidationResult interfaceResult = result;
        IReadOnlyList<ValidationError> readOnlyErrors = interfaceResult.Errors;

        // Assert
        readOnlyErrors.ShouldNotBeNull();
        readOnlyErrors.Count().ShouldBe(2);
        readOnlyErrors.ShouldBeEquivalentTo(errors);
        readOnlyErrors.ShouldBeAssignableTo<IReadOnlyList<ValidationError>>();
    }
}

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        string propertyName = "Name";
        string errorMessage = "Name is required";

        // Act
        ValidationError error = new(propertyName, errorMessage);

        // Assert
        error.ShouldNotBeNull();
        error.PropertyName.ShouldBe(propertyName);
        error.ErrorMessage.ShouldBe(errorMessage);
    }

    [Fact]
    public void PropertySetters_UpdateValues()
    {
        // Arrange
        ValidationError error = new("Name", "Name is required")
        {
            // Act
            PropertyName = "Email",
            ErrorMessage = "Email is invalid"
        };

        // Assert
        error.PropertyName.ShouldBe("Email");
        error.ErrorMessage.ShouldBe("Email is invalid");
    }
}
