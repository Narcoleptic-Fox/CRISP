namespace CRISP.Core.Identity;

public sealed record UpdateUser : ModifyCommand
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public sealed class UpdateUserValidator : BaseValidator<UpdateUser>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.");
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("User name is required.")
            .MaximumLength(256)
            .WithMessage("User name cannot exceed 256 characters.");
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(15)
            .WithMessage("Phone number cannot exceed 15 characters.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}