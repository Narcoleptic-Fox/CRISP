namespace CRISP.Responses
{
    /// <summary>
    /// Represents a response containing a filtered collection of items with pagination information.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <remarks>
    /// This class is used as the response type for filtered queries that may return multiple items.
    /// It includes both the current page of results and metadata about the total number of items
    /// available across all pages.
    /// </remarks>
    public sealed record FilteredResponse<T>
        where T : class
    {
        /// <summary>
        /// Gets or sets the collection of items in the current page of results.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = [];
        
        /// <summary>
        /// Gets or sets the total number of items available across all pages.
        /// </summary>
        /// <remarks>
        /// This value represents the total count without pagination, useful for
        /// calculating the total number of pages or showing progress indicators.
        /// </remarks>
        public int TotalItems { get; set; }
    }
}
