namespace CRISP.Requests
{
    public abstract record Command<TResponse> : IRequest<TResponse>;
    public abstract record Command : IRequest;
    public abstract record CreateCommand<TId> : Command<TId> where TId : IEqualityComparer<TId>;
    public abstract record ModifyCommand<TId> : Command where TId : IEqualityComparer<TId>
    {
        public TId Id { get; init; } = default!;
    }
    public abstract record DeleteCommand<TId> : ModifyCommand<TId> where TId : IEqualityComparer<TId>;
    public abstract record ArchiveCommand<TId> : ModifyCommand<TId> where TId : IEqualityComparer<TId>;
}
