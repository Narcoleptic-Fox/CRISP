namespace CRISP.Core.Identity
{
    public record Users : BaseModel
    {
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; }
        public DateTimeOffset? LockoutEnd { get; init; }
        public bool LockOutEnabled { get; init; }
    }
}
