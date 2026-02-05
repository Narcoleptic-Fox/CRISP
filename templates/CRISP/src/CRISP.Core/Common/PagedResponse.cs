namespace CRISP.Core.Common
{
    /// <summary>
    /// Represents a paginated response containing a collection of items and metadata about the total count.
    /// </summary>
    /// <typeparam name="TModel">The type of items in the paginated response. Must inherit from <see cref="BaseModel"/>.</typeparam>
    /// <remarks>
    /// This record provides a standardized way to return paginated data from queries and API endpoints.
    /// It includes both the requested items for the current page and the total count of items available,
    /// which is essential for implementing pagination controls in user interfaces.
    /// 
    /// The record follows immutability principles and provides value-based equality semantics,
    /// making it suitable for use in caching scenarios and ensuring consistent behavior
    /// when comparing paginated responses.
    /// </remarks>
    public sealed record PagedResponse<TModel>
        where TModel : BaseModel
    {
        /// <summary>
        /// Gets the collection of items for the current page.
        /// </summary>
        /// <value>
        /// An <see cref="IEnumerable{TModel}"/> containing the items for the requested page.
        /// If no items are found, this will be an empty collection rather than <c>null</c>.
        /// The collection is initialized to an empty array by default to ensure it's never <c>null</c>.
        /// </value>
        /// <remarks>
        /// The items in this collection represent the subset of data requested based on
        /// pagination parameters such as page number and page size. The actual number
        /// of items may be less than the requested page size if there are insufficient
        /// items remaining or if this is the last page of results.
        /// </remarks>
        public IEnumerable<TModel> Items { get; init; } = [];

        /// <summary>
        /// Gets the total number of items available across all pages.
        /// </summary>
        /// <value>
        /// An integer representing the total count of items that match the query criteria,
        /// regardless of pagination. This value is used to calculate the total number of pages
        /// and to display pagination information in user interfaces.
        /// </value>
        /// <remarks>
        /// This count represents the total number of items that would be returned if no
        /// pagination were applied. It includes items on all pages, not just the current page.
        /// This information is essential for implementing pagination controls that show
        /// page numbers, "next/previous" buttons, and total page counts.
        /// </remarks>
        public int TotalCount { get; init; }
    }
}
