namespace CRISP.Core.Identity;

/// <summary>
/// Represents the data required to create a new user.
/// </summary>
[ExportTsInterface]
public sealed record CreateUser : CreateCommand
{
    /// <summary>
    /// Gets the unique username used to identify the user in the application.
    /// </summary>
    public string Username { get; init; } = default!;

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = default!;

    /// <summary>
    /// Gets the application role assigned to the user.
    /// </summary>
    public Roles Role { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user account can currently sign in or be assigned work.
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Validates user creation requests before they are handled by the application.
/// </summary>
public sealed class CreateUserValidator : AbstractValidator<CreateUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserValidator"/> class.
    /// </summary>
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified.");
        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive status is required.");
    }
}
