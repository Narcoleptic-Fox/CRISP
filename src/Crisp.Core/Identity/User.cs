namespace CRISP.Core.Identity
{
    public sealed record User : Users
    {
        public string PermissionString { get; init; } = string.Empty;
    }
}
