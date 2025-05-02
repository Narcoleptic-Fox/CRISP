using CRISP.Validation;

namespace CRISP.Tests.Validation;

public class FluentValidatorTests
{
    [Fact]
    public void FluentValidator_BasicValidation_ValidatesCorrectly()
    {
        // Arrange
        UserValidator validator = new();
        User user = new()
        {
            Username = "", // Invalid: empty
            Email = "not-an-email", // Invalid: not an email format
            Age = -5, // Invalid: negative age
            Roles = [] // Invalid: empty roles list
        };

        // Act
        ValidationResult result = validator.Validate(user);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThan(0);

        // Check specific errors
        result.Errors.ShouldContain(e => e.PropertyName == "Username" && e.ErrorMessage == "Username is required");
        result.Errors.ShouldContain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email address is not valid");
        result.Errors.ShouldContain(e => e.PropertyName == "Age" && e.ErrorMessage == "Age must be greater than 0");
        result.Errors.ShouldContain(e => e.PropertyName == "Roles" && e.ErrorMessage == "User must have at least one role");
    }

    [Fact]
    public void FluentValidator_ValidModel_PassesValidation()
    {
        // Arrange
        UserValidator validator = new();
        User user = new()
        {
            Username = "john_doe",
            Email = "john@example.com",
            Age = 25,
            Roles = ["User"],
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                ZipCode = "12345"
            }
        };

        // Act
        ValidationResult result = validator.Validate(user);

        // Assert
        result.ShouldNotBeNull();
        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
    }

    [Fact]
    public void FluentValidator_ConditionalValidation_AppliesWhenConditionMet()
    {
        // Arrange
        UserValidator validator = new();

        // Adult with no address (should fail because address is required for adults)
        User adultWithoutAddress = new()
        {
            Username = "adult_user",
            Email = "adult@example.com",
            Age = 30,
            Roles = ["User"],
            Address = null
        };

        // Minor with no address (should pass because address is not required for minors)
        User minorWithoutAddress = new()
        {
            Username = "minor_user",
            Email = "minor@example.com",
            Age = 17,
            Roles = ["User"],
            Address = null
        };

        // Act
        ValidationResult adultResult = validator.Validate(adultWithoutAddress);
        ValidationResult minorResult = validator.Validate(minorWithoutAddress);

        // Assert
        adultResult.IsValid.ShouldBeFalse();
        adultResult.Errors.ShouldContain(e => e.PropertyName == "Address" && e.ErrorMessage == "Address is required");

        minorResult.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void FluentValidator_NestedValidation_ValidatesNestedProperties()
    {
        // Arrange
        UserValidator validator = new();
        User user = new()
        {
            Username = "john_doe",
            Email = "john@example.com",
            Age = 25,
            Roles = ["User"],
            Address = new Address
            {
                Street = "123 Main St",
                City = "", // Invalid: empty city
                ZipCode = "invalid-zip" // Invalid: incorrect format
            }
        };

        // Act
        ValidationResult result = validator.Validate(user);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Address.City" && e.ErrorMessage == "City is required");
        result.Errors.ShouldContain(e => e.PropertyName == "Address.ZipCode" &&
                                       e.ErrorMessage == "ZIP code must be in the format 12345 or 12345-6789");
    }

    [Fact]
    public void FluentValidator_CollectionValidation_ValidatesCollectionItems()
    {
        // Arrange
        UserValidator validator = new();
        User user = new()
        {
            Username = "john_doe",
            Email = "john@example.com",
            Age = 25,
            Roles = ["", "ARoleNameThatIsWayTooLongAndExceedsTheMaximumLength"],
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                ZipCode = "12345"
            }
        };

        // Act
        ValidationResult result = validator.Validate(user);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors.ShouldContain(e => e.PropertyName == "Roles" &&
                                     e.ErrorMessage == "Each role must not be empty and must be 20 characters or less");
    }

    [Fact]
    public void FluentValidator_RuleSet_AppliesRulesInSpecificSet()
    {
        // Arrange
        UserValidator validator = new();
        User user = new()
        {
            Username = "regular_user", // Valid for regular validation, invalid for admin
            Email = "user@example.com",
            Age = 25,
            Roles = ["User"], // Valid for regular validation, invalid for admin
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                ZipCode = "12345"
            }
        };

        // Act
        ValidationResult regularResult = validator.Validate(user); // Should pass regular validation
        ValidationResult adminResult = validator.Validate(user, "AdminValidation"); // Should fail admin validation

        // Assert
        regularResult.IsValid.ShouldBeTrue();

        adminResult.IsValid.ShouldBeFalse();
        adminResult.Errors.ShouldContain(e => e.PropertyName == "Username" &&
                                         e.ErrorMessage == "Admin usernames must start with 'admin_'");
        adminResult.Errors.ShouldContain(e => e.PropertyName == "Roles" &&
                                         e.ErrorMessage == "Admins must have either 'Admin' or 'SuperAdmin' role");
    }
}