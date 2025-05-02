using CRISP.Validation;

namespace CRISP.Tests.Validation;

/// <summary>
/// Validator for the <see cref="Address"/> class.
/// </summary>
public class AddressValidator : FluentValidator<Address>
{
    /// <inheritdoc/>
    protected override void ConfigureValidationRules()
    {
        RuleFor(a => a.Street)
            .NotEmpty()
            .WithMessage("Street address is required")
            .MaxLength(100);

        RuleFor(a => a.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaxLength(50);

        RuleFor(a => a.ZipCode)
            .NotEmpty()
            .WithMessage("ZIP/Postal code is required")
            .Matches(@"^\d{5}(-\d{4})?$")
            .WithMessage("ZIP code must be in the format 12345 or 12345-6789");
    }
}

/// <summary>
/// Validator for the <see cref="User"/> class, demonstrating the fluent validation rules.
/// </summary>
public class UserValidator : FluentValidator<User>
{
    /// <inheritdoc/>
    protected override void ConfigureValidationRules()
    {
        // Basic validation rules
        RuleFor(u => u.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .Length(3, 20)
            .WithMessage("Username must be between 3 and 20 characters");

        RuleFor(u => u.Email)
            .NotEmpty()
            .WithMessage("Email address is required")
            .EmailAddress()
            .WithMessage("Email address is not valid");

        // Numeric validation with conditions
        RuleFor(u => u.Age)
            .GreaterThan(0)
            .WithMessage("Age must be greater than 0")
            .LessThan(120)
            .WithMessage("Age must be less than 120");

        // Collection validation
        RuleFor(u => u.Roles)
            .NotNull()
            .WithMessage("Roles collection cannot be null")
            .MinCount(1)
            .WithMessage("User must have at least one role");

        // ForEach validation on collection items
        RuleFor(u => u.Roles)
            .ForEach(role => !string.IsNullOrEmpty(role) && role.Length <= 20,
                    "Each role must not be empty and must be 20 characters or less");

        // Complex object validation
        RuleFor(u => u.Address)
            .NotNull()
            .WithMessage("Address is required")
            .When(u => u.Age >= 18); // Only require address for adults

        // Nested validator for complex object
        RuleFor(u => u.Address!)
            .ForEach(new AddressValidator())
            .When(u => u.Address != null);

        // Rule set for specific validation scenarios
        RuleSet("AdminValidation", () =>
        {
            RuleFor(u => u.Username)
                .Must(username => username.StartsWith("admin_"),
                     "Admin usernames must start with 'admin_'");

            RuleFor(u => u.Roles)
                .ForEach(role => role == "Admin" || role == "SuperAdmin",
                        "Admins must have either 'Admin' or 'SuperAdmin' role");
        });
    }
}