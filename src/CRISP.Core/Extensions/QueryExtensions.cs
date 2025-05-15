namespace CRISP;

/// <summary>
/// Extension methods for query operations in the CRISP architecture.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Applies pagination to a queryable collection.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="query">The queryable collection.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A queryable collection with pagination applied.</returns>
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        if (pageNumber <= 0)
            pageNumber = 1;

        if (pageSize <= 0)
            pageSize = 10;

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Creates a paginated result from a queryable collection.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="query">The queryable collection.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paginated result.</returns>
    public static PagedResult<T> ToPaginatedResult<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber <= 0)
            pageNumber = 1;

        if (pageSize <= 0)
            pageSize = 10;

        int totalCount = query.Count();

        // We need to preserve the query as IQueryable to maintain the expected type
        IQueryable<T> items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        return new PagedResult<T>
        {
            Items = items, // Pass the IQueryable directly without materializing
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Creates a paginated result from a queryable collection using filter parameters.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="query">The queryable collection.</param>
    /// <param name="filter">The filter parameters.</param>
    /// <returns>A paginated result.</returns>
    public static PagedResult<T> ToPaginatedResult<T>(
        this IQueryable<T> query,
        FilterBase filter) => query.ToPaginatedResult(filter.Page, filter.PageSize);
}