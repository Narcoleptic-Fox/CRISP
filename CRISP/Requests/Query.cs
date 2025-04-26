using CRISP.Models;
using CRISP.Responses;

namespace CRISP.Requests
{
    public abstract record Query<TResponse> : IRequest<TResponse>;
    public abstract record SingularQuery<TResponse> : Query<TResponse>
        where TResponse : BaseModel?
    {
        public SingularQuery(Guid Id) => this.Id = Id;
        public SingularQuery() { }
        public Guid Id { get; init; }
    }

    public abstract record FilteredQuery<TResponse> : Query<FilteredResponse<TResponse>>
        where TResponse : BaseModel
    {
        public bool IncludeArchived { get; init; }
        public string? SortBy { get; init; }
        public bool Descending { get; init; }
        public string? Filter { get; init; }
    }

    public abstract record PagedQuery<TResponse> : FilteredQuery<TResponse>
        where TResponse : BaseModel
    {
        public int PageIndex { get; init; } = 0;
        public int PageSize { get; init; } = int.MaxValue;
    }
}
