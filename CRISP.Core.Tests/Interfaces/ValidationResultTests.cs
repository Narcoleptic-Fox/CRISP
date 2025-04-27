using CRISP.Core.Interfaces;
using FluentAssertions;

namespace CRISP.Core.Tests.Interfaces;

public class ValidationResultTests
{
    [Fact]
    public void Success_ReturnsValidResult()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithErrors_ReturnsInvalidResultWithErrors()
    {
        // Arrange
        var error1 = new ValidationError("Name", "Name is required");
        var error2 = new ValidationError("Email", "Email is invalid");

        // Act
        var result = ValidationResult.Failure(error1, error2);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(error1);
        result.Errors.Should().Contain(error2);
    }

    [Fact]
    public void Failure_WithMessageAndProperty_ReturnsInvalidResultWithSingleError()
    {
        // Arrange
        string errorMessage = "Value is invalid";
        string propertyName = "TestProperty";

        // Act
        var result = ValidationResult.Failure(errorMessage, propertyName);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        
        var error = result.Errors.First();
        error.PropertyName.Should().Be(propertyName);
        error.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithMessageOnly_ReturnsInvalidResultWithSingleError()
    {
        // Arrange
        string errorMessage = "Value is invalid";

        // Act
        var result = ValidationResult.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        
        var error = result.Errors.First();
        error.PropertyName.Should().BeEmpty();
        error.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void Constructor_DefaultValues_SetsCorrectDefaults()
    {
        // Act
        var result = new ValidationResult();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse(); // Default value of bool is false
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void IValidationResult_Errors_ReturnsReadOnlyList()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Name", "Name is required"),
            new ValidationError("Email", "Email is invalid")
        };
        var result = ValidationResult.Failure(errors);
        
        // Act
        IValidationResult interfaceResult = result;
        var readOnlyErrors = interfaceResult.Errors;

        // Assert
        readOnlyErrors.Should().NotBeNull();
        readOnlyErrors.Should().HaveCount(2);
        readOnlyErrors.Should().BeEquivalentTo(errors);
        readOnlyErrors.Should().BeAssignableTo<IReadOnlyList<ValidationError>>();
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
        var error = new ValidationError(propertyName, errorMessage);

        // Assert
        error.Should().NotBeNull();
        error.PropertyName.Should().Be(propertyName);
        error.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void PropertySetters_UpdateValues()
    {
        // Arrange
        var error = new ValidationError("Name", "Name is required");
        
        // Act
        error.PropertyName = "Email";
        error.ErrorMessage = "Email is invalid";

        // Assert
        error.PropertyName.Should().Be("Email");
        error.ErrorMessage.Should().Be("Email is invalid");
    }
}
