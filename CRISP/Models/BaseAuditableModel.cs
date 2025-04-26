namespace CRISP.Models
{
    public abstract record BaseAuditableModel<TId> : BaseEntityModel<TId>
        where TId : IEqualityComparer<TId>
    {
        public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;
        public string CreatedBy { get; init; } = default!;
        public DateTimeOffset LastModified { get; init; } = DateTimeOffset.UtcNow;
        public string LastModifiedBy { get; init; } = default!;
        public bool IsArchived { get; init; }
        public bool IsDeleted { get; init; }
    }
}
