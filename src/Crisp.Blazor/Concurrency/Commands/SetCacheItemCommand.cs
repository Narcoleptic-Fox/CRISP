using Blazor.Concurrency.Cache;
using Crisp.Commands;

namespace Crisp.Blazor.Concurrency.Commands;

/// <summary>
/// Command to set a cached item using web workers.
/// </summary>
public sealed record SetCacheItemCommand : ICommand<CacheSetResult>
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
