using CRISP.Models;
using CRISP.Responses;

namespace CRISP.Requests
{
    /// <summary>
    /// Base abstract record for all query operations that return a specific response type.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from this query.</typeparam>
    /// <remarks>
    /// This is the base class for all query objects in the CQRS pattern.
    /// Queries represent read operations that retrieve data from the system without modifying state.
    /// </remarks>
    public abstract record Query<TResponse> : IRequest<TResponse>;
    
    /// <summary>
    /// Represents a query that returns a single model entity.
    /// </summary>
    /// <typeparam name="TResponse">The type of model to return.</typeparam>
    /// <remarks>
    /// Use this for queries that retrieve a single entity by its identifier,
    /// such as "get customer by id" or "get order details".
    /// </remarks>
    public abstract record SingularQuery<TResponse> : Query<TResponse>
        where TResponse : BaseModel?
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingularQuery{TResponse}"/> class with the specified entity ID.
        /// </summary>
        /// <param name="Id">The unique identifier of the entity to retrieve.</param>
        public SingularQuery(Guid Id) => this.Id = Id;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SingularQuery{TResponse}"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for frameworks that require a parameterless constructor.
        /// When using this constructor, ensure the Id property is explicitly set before sending the query.
        /// </remarks>
        public SingularQuery() { }
        
        /// <summary>
        /// Gets or initializes the unique identifier of the entity to retrieve.
        /// </summary>
        public Guid Id { get; init; }
    }

    /// <summary>
    /// Represents a query that returns a filtered collection of model entities.
    /// </summary>
    /// <typeparam name="TResponse">The type of model entities in the collection.</typeparam>
    /// <remarks>
    /// Use this for queries that retrieve multiple entities with filtering and sorting options,
    /// such as "find customers by region" or "get orders by status".
    /// </remarks>
    public abstract record FilteredQuery<TResponse> : Query<FilteredResponse<TResponse>>
        where TResponse : BaseModel
    {
        /// <summary>
        /// Gets or initializes a value indicating whether archived entities should be included in the results.
        /// </summary>
        public bool IncludeArchived { get; init; }
        
        /// <summary>
        /// Gets or initializes the name of the property to sort results by.
        /// </summary>
        public string? SortBy { get; init; }
        
        /// <summary>
        /// Gets or initializes a value indicating whether the sort order should be descending.
        /// </summary>
        public bool Descending { get; init; }
        
        /// <summary>
        /// Gets or initializes a string filter to apply to the query.
        /// </summary>
        /// <remarks>
        /// The specific implementation of filtering depends on the query handler.
        /// </remarks>
        public string? Filter { get; init; }
    }

    /// <summary>
    /// Represents a paged query that returns a subset of a larger filtered collection.
    /// </summary>
    /// <typeparam name="TResponse">The type of model entities in the collection.</typeparam>
    /// <remarks>
    /// Use this for queries that retrieve large data sets that need to be paginated,
    /// such as "list all products page by page" or "search results with pagination".
    /// </remarks>
    public abstract record PagedQuery<TResponse> : FilteredQuery<TResponse>
        where TResponse : BaseModel
    {
        /// <summary>
        /// Gets or initializes the zero-based page index.
        /// </summary>
        /// <remarks>
        /// Defaults to 0 (first page).
        /// </remarks>
        public int PageIndex { get; init; } = 0;
        
        /// <summary>
        /// Gets or initializes the maximum number of items to return per page.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="int.MaxValue"/>, effectively disabling pagination unless explicitly set.
        /// </remarks>
        public int PageSize { get; init; } = int.MaxValue;
    }
}
