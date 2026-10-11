using System.Text;

namespace CRISP.Core.Common
{
    /// <summary>
    /// Defines a contract for queries that return a specific response type.
    /// </summary>
    /// <typeparam name="TResponse">The type of response returned by the query. Must be a reference type.</typeparam>
    /// <remarks>
    /// Queries represent read-only operations that retrieve data from the system.
    /// They follow the CQRS (Command Query Responsibility Segregation) pattern,
    /// separating read operations from write operations.
    /// </remarks>
    public interface IQuery<TResponse> where TResponse : class;

    /// <summary>
    /// Represents a query that retrieves a single entity by its identifier.
    /// </summary>
    /// <typeparam name="TResponse">The type of entity to retrieve. Must inherit from <see cref="BaseModel"/>.</typeparam>
    /// <remarks>
    /// This record provides a standard way to query for individual entities using their unique identifier.
    /// It can be instantiated with or without an ID, allowing for flexible usage patterns.
    /// </remarks>
    public abstract record SingularQuery<TResponse> : IQuery<TResponse>
        where TResponse : BaseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingularQuery{TResponse}"/> record.
        /// </summary>
        public SingularQuery() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularQuery{TResponse}"/> record with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        public SingularQuery(Guid id) => Id = id;

        /// <summary>
        /// Gets or sets the unique identifier of the entity to retrieve.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> representing the unique identifier of the entity.
        /// </value>
        public Guid Id { get; init; }
    }

    /// <summary>
    /// Represents a query that retrieves a paginated collection of entities.
    /// </summary>
    /// <typeparam name="TResponse">The type of entities in the collection. Must inherit from <see cref="BaseModel"/>.</typeparam>
    /// <remarks>
    /// This abstract record provides a foundation for implementing paginated queries with sorting,
    /// filtering, and archival status options. It includes sensible defaults and helper methods
    /// to retrieve effective values when properties are not explicitly set.
    /// </remarks>
    public abstract record PagedQuery<TResponse> : IQuery<PagedResponse<TResponse>>
        where TResponse : BaseModel
    {
        /// <summary>
        /// Gets or sets the zero-based page number for pagination.
        /// </summary>
        /// <value>
        /// A nullable integer representing the page number. If null, the default page from <see cref="Query.Page"/> will be used.
        /// </value>
        public int? Page { get; set; } = Query.Page;

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        /// <value>
        /// A nullable integer representing the page size. If null, the default page size from <see cref="Query.PageSize"/> will be used.
        /// </value>
        public int? PageSize { get; set; } = Query.PageSize;

        /// <summary>
        /// Gets or sets the property name to sort by.
        /// </summary>
        /// <value>
        /// A nullable string representing the property name for sorting. If null, the default sort property from <see cref="Query.SortBy"/> will be used.
        /// </value>
        public string? SortBy { get; set; } = Query.SortBy;

        /// <summary>
        /// Gets or sets a value indicating whether to sort in descending order.
        /// </summary>
        /// <value>
        /// A nullable boolean indicating sort direction. If null, the default sort direction from <see cref="Query.SortDescending"/> will be used.
        /// </value>
        public bool? SortDescending { get; set; } = Query.SortDescending;

        /// <summary>
        /// Gets or sets a collection of specific entity identifiers to include in the results.
        /// </summary>
        /// <value>
        /// A nullable enumerable of entity identifiers to filter by. If <c>null</c>, no ID filtering is applied.
        /// </value>
        public IEnumerable<Guid>? Ids { get; set; }

        /// <summary>
        /// Gets the effective page number, falling back to the system default when no page was supplied.
        /// </summary>
        /// <returns>The requested page number, or <see cref="Query.Page"/> when <see cref="Page"/> is <c>null</c>.</returns>
        public int GetPageOrDefault() => Page ?? Query.Page;

        /// <summary>
        /// Gets the effective page size, falling back to the system default when no page size was supplied.
        /// </summary>
        /// <returns>The requested page size, or <see cref="Query.PageSize"/> when <see cref="PageSize"/> is <c>null</c>.</returns>
        public int GetPageSizeOrDefault() => PageSize ?? Query.PageSize;

        /// <summary>
        /// Converts the paging, sorting, and identifier filters into a URL query string.
        /// </summary>
        /// <returns>A query string beginning with <c>?</c> that represents the populated query parameters.</returns>
        public virtual string ToQueryString()
        {
            var builder = new StringBuilder();
            builder.Append($"?page={Page ?? Query.Page}&pageSize={PageSize ?? Query.PageSize}");
            if (!string.IsNullOrEmpty(SortBy))
            {
                builder.Append($"&sortBy={SortBy}");
            }
            if (SortDescending is not null)
            {
                builder.Append($"&sortDescending={SortDescending}");
            }
            if (Ids is not null && Ids.Any())
            {
                builder.Append($"&ids={string.Join(',', Ids)}");
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// Defines a contract for services that execute queries and return responses.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to execute. Must implement <see cref="IQuery{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the query. Must be a reference type.</typeparam>
    /// <remarks>
    /// Query services implement the query handling logic in the CQRS pattern.
    /// They are responsible for processing queries and returning the appropriate data
    /// without causing side effects or modifying system state.
    /// </remarks>
    public interface IQueryService<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
        where TResponse : class
    {
        /// <summary>
        /// Executes the specified query and returns the corresponding response.
        /// </summary>
        /// <param name="query">The query instance containing the parameters for the operation.</param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResponse}"/> that represents the asynchronous query operation.
        /// The task result contains the response data requested by the query.
        /// </returns>
        /// <remarks>
        /// This method should be implemented to handle the specific query logic without causing side effects.
        /// The operation should be idempotent and safe to retry in case of transient failures.
        /// </remarks>
        ValueTask<TResponse> Send(TQuery query, CancellationToken cancellationToken = default);
    }
}
