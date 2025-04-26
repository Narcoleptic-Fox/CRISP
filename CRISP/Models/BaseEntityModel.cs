namespace CRISP.Models
{
    public abstract record BaseEntityModel<TId> : BaseModel
        where TId : IEqualityComparer<TId>
    {
        public TId Id { get; init; } = default!;
    }
}
