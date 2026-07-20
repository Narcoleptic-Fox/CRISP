namespace CRISP.Core.Identity;

/// <summary>
/// Represents the data required to update a user's preferences.
/// </summary>
[ExportTsInterface]
public sealed record UpdateUserSettings : ModifyCommand
{
    /// <summary>
    /// Gets a value indicating whether the user allows notifications.
    /// </summary>
    public bool AllowNotifications { get; init; } = true;
}

/// <summary>
/// Validates user settings update requests before they are handled by the application.
/// </summary>
public sealed class UpdateUserSettingsValidator : AbstractValidator<UpdateUserSettings>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsValidator"/> class.
    /// </summary>
    public UpdateUserSettingsValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.AllowNotifications)
            .NotNull().WithMessage("Allow Notifications are required.");
    }
}
