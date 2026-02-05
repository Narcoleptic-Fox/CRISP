namespace CRISP.Core.Identity;

public sealed record CreateRole : CreateCommand
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<Permissions> Permissions { get; set; } = [];
}

public sealed class CreateRoleValidator : BaseValidator<CreateRole>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);
        RuleFor(x => x.Permissions)
            .NotNull()
            .ForEach(x => x.IsInEnum());
    }
}

public sealed record UpdateRole : ModifyCommand
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<Permissions> Permissions { get; set; } = [];
}

public sealed class UpdateRoleValidator : BaseValidator<UpdateRole>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);
        RuleFor(x => x.Permissions)
            .NotNull()
            .ForEach(x => x.IsInEnum());
    }
}