using Blazor.Concurrency.Cache;
using Crisp.Queries;

namespace Crisp.Concurrency.Queries
{
    /// <summary>
    /// Query to get a cached item using web workers.
    /// </summary>
    public sealed record GetCacheItemQuery : IQuery<CacheGetResult>
    {
        /// <summary>
        /// The cache key to set.
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// The value to cache.
        /// </summary>
        public required object Value { get; init; }

        /// <summary>
        /// Optional expiration time for the cached item.
        /// </summary>
        public TimeSpan? ExpiresIn { get; init; }
    }
}
