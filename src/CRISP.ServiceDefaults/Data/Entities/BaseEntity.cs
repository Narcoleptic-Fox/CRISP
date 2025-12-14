namespace CRISP.ServiceDefaults.Data.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
    }

    public abstract class BaseAuditableEntity : BaseEntity, IAchievable, ISoftDelete
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; }
        public string? ArchivingReason { get; set; }
        public bool IsDeleted { get; set; }
    }
}
