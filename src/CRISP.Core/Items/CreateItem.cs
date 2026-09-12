namespace CRISP.Core.Items;

/// <summary>
/// Represents the data required to create a new item.
/// </summary>
[ExportTsInterface]
public sealed record CreateItem : CreateCommand
{
    /// <summary>
    /// Gets the item display name.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets the optional free-text description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the free-text owner name.
    /// </summary>
    public string? Owner { get; init; }
}

/// <summary>
/// Validates item creation requests before they are handled by the application.
/// </summary>
public sealed class CreateItemValidator : AbstractValidator<CreateItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateItemValidator"/> class.
    /// </summary>
    public CreateItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(100).WithMessage("Item name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Owner)
            .MaximumLength(100).WithMessage("Owner must not exceed 100 characters.");
    }
}
