namespace CRISP.ServiceDefaults.Data.Entities
{
    public class SystemMessageEntity : BaseEntity
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    }
}