namespace CRISP.Core.Constants
{
    /// <summary>
    /// Provides default constants for query operations and pagination.
    /// </summary>
    /// <remarks>
    /// This class contains commonly used default values for query parameters such as
    /// pagination settings, sorting preferences, and filtering options. These constants
    /// ensure consistent behavior across the application when explicit values are not provided.
    /// 
    /// The values defined here serve as system-wide defaults that can be referenced
    /// throughout the application to maintain consistency in query behavior.
    /// </remarks>
    public static class Query
    {
        /// <summary>
        /// The default page number for paginated queries (zero-based).
        /// </summary>
        /// <remarks>
        /// This constant defines the starting page for pagination. Using a zero-based
        /// indexing system where the first page is represented by 0, which is common
        /// in many programming contexts and APIs.
        /// </remarks>
        public const int Page = 0;

        /// <summary>
        /// The default number of items to return per page in paginated queries.
        /// </summary>
        /// <remarks>
        /// This constant defines a reasonable default page size that balances
        /// performance with usability. A page size of 100 provides enough items
        /// to be useful while avoiding excessive memory usage or slow response times.
        /// </remarks>
        public const int PageSize = 100;

        /// <summary>
        /// The default property name to use for sorting query results.
        /// </summary>
        /// <remarks>
        /// This constant specifies the default sort field when no explicit sorting
        /// is requested. Sorting by "Id" provides a consistent and predictable
        /// ordering that works across all entities that inherit from <see cref="Common.BaseModel"/>.
        /// </remarks>
        public const string SortBy = "Id";

        /// <summary>
        /// The default sort direction for query results.
        /// </summary>
        /// <remarks>
        /// This constant defines the default sort order as ascending (<c>false</c>).
        /// Ascending order typically provides the most intuitive experience for users,
        /// showing items from lowest to highest values (e.g., oldest to newest by ID,
        /// A to Z alphabetically).
        /// </remarks>
        public const bool SortDescending = false;

        /// <summary>
        /// The default setting for including archived entities in query results.
        /// </summary>
        /// <remarks>
        /// This constant determines whether archived (soft-deleted) entities should
        /// be included in query results by default. Setting this to <c>false</c> ensures
        /// that normal business operations exclude archived entities, while still
        /// allowing explicit inclusion when needed for administrative or audit purposes.
        /// </remarks>
        public const bool IncludeArchived = false;
    }
}
