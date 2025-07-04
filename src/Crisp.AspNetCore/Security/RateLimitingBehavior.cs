using Crisp.Common;
using Crisp.Exceptions;
using Crisp.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Crisp.AspNetCore.Security;

/// <summary>
/// Pipeline behavior that implements rate limiting for CRISP requests.
/// Supports multiple algorithms and client identification strategies.
/// </summary>
public class RateLimitingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly RateLimitingOptions _options;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RateLimitingBehavior<TRequest, TResponse>> _logger;
    private readonly IRateLimitStore _store;

    public RateLimitingBehavior(
        RateLimitingOptions options,
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RateLimitingBehavior<TRequest, TResponse>> logger,
        IRateLimitStore store)
    {
        _options = options;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _store = store;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return await next();
        }

        Type requestType = typeof(TRequest);
        RateLimitAttribute? rateLimitAttribute = GetRateLimitAttribute(requestType);

        if (rateLimitAttribute == null && _options.DefaultPolicy == null)
        {
            return await next();
        }

        RateLimitPolicy policy = rateLimitAttribute?.ToPolicy() ?? _options.DefaultPolicy!;
        string clientId = GetClientIdentifier();
        string key = GenerateKey(clientId, requestType.Name, policy);

        bool allowed = await CheckRateLimitAsync(key, policy, cancellationToken);

        if (!allowed)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on {RequestType}",
                clientId, requestType.Name);

            throw new CrispException($"Rate limit exceeded for {requestType.Name}. Try again later.");
        }

        return await next();
    }

    private async Task<bool> CheckRateLimitAsync(string key, RateLimitPolicy policy, CancellationToken cancellationToken)
    {
        switch (policy.Algorithm)
        {
            case RateLimitAlgorithm.TokenBucket:
                return await CheckTokenBucketAsync(key, policy, cancellationToken);

            case RateLimitAlgorithm.SlidingWindow:
                return await CheckSlidingWindowAsync(key, policy, cancellationToken);

            case RateLimitAlgorithm.FixedWindow:
                return await CheckFixedWindowAsync(key, policy, cancellationToken);

            default:
                return true;
        }
    }

    private async Task<bool> CheckTokenBucketAsync(string key, RateLimitPolicy policy, CancellationToken cancellationToken)
    {
        TokenBucket? bucket = await _store.GetTokenBucketAsync(key, cancellationToken);

        if (bucket == null)
        {
            bucket = new TokenBucket
            {
                Tokens = policy.Limit,
                LastRefill = DateTime.UtcNow,
                Capacity = policy.Limit
            };
        }

        // Refill tokens based on time passed
        DateTime now = DateTime.UtcNow;
        TimeSpan timeSpan = now - bucket.LastRefill;
        int tokensToAdd = (int)(timeSpan.TotalSeconds * policy.Limit / policy.Window.TotalSeconds);

        bucket.Tokens = Math.Min(bucket.Capacity, bucket.Tokens + tokensToAdd);
        bucket.LastRefill = now;

        if (bucket.Tokens >= 1)
        {
            bucket.Tokens--;
            await _store.SetTokenBucketAsync(key, bucket, policy.Window, cancellationToken);
            return true;
        }

        await _store.SetTokenBucketAsync(key, bucket, policy.Window, cancellationToken);
        return false;
    }

    private async Task<bool> CheckSlidingWindowAsync(string key, RateLimitPolicy policy, CancellationToken cancellationToken)
    {
        SlidingWindow? window = await _store.GetSlidingWindowAsync(key, cancellationToken);
        DateTime now = DateTime.UtcNow;
        DateTime windowStart = now - policy.Window;

        if (window == null)
        {
            window = new SlidingWindow { Requests = [] };
        }

        // Remove expired requests
        window.Requests.RemoveAll(r => r < windowStart);

        if (window.Requests.Count >= policy.Limit)
        {
            await _store.SetSlidingWindowAsync(key, window, policy.Window, cancellationToken);
            return false;
        }

        window.Requests.Add(now);
        await _store.SetSlidingWindowAsync(key, window, policy.Window, cancellationToken);
        return true;
    }

    private async Task<bool> CheckFixedWindowAsync(string key, RateLimitPolicy policy, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        DateTime windowStart = new(
            now.Year, now.Month, now.Day, now.Hour,
            now.Minute / (int)policy.Window.TotalMinutes * (int)policy.Window.TotalMinutes, 0);

        string windowKey = $"{key}:{windowStart:yyyyMMddHHmm}";
        int counter = await _store.GetCounterAsync(windowKey, cancellationToken);

        if (counter >= policy.Limit)
        {
            return false;
        }

        await _store.IncrementCounterAsync(windowKey, policy.Window, cancellationToken);
        return true;
    }

    private string GetClientIdentifier()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return "unknown";
        }

        // Try different identification strategies
        switch (_options.ClientIdentificationStrategy)
        {
            case ClientIdentificationStrategy.IpAddress:
                return GetClientIpAddress(httpContext);

            case ClientIdentificationStrategy.UserId:
                return GetUserId(httpContext);

            case ClientIdentificationStrategy.ApiKey:
                return GetApiKey(httpContext);

            case ClientIdentificationStrategy.Composite:
                return GetCompositeIdentifier(httpContext);

            default:
                return GetClientIpAddress(httpContext);
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        string? forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        string? realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        return !string.IsNullOrEmpty(realIp) ? realIp : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GetUserId(HttpContext context) => context.User?.Identity?.Name ?? GetClientIpAddress(context);

    private static string GetApiKey(HttpContext context)
    {
        string? apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault() ??
                    context.Request.Headers["Authorization"].FirstOrDefault();

        return !string.IsNullOrEmpty(apiKey) ? ComputeHash(apiKey) : GetClientIpAddress(context);
    }

    private static string GetCompositeIdentifier(HttpContext context)
    {
        List<string> components =
        [
            GetClientIpAddress(context),
            context.User?.Identity?.Name ?? "anonymous",
            context.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown"
        ];

        return ComputeHash(string.Join("|", components));
    }

    private static string ComputeHash(string input)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16]; // Use first 16 characters
    }

    private static string GenerateKey(string clientId, string requestType, RateLimitPolicy policy) => $"rate_limit:{clientId}:{requestType}:{policy.Algorithm}";

    private static RateLimitAttribute? GetRateLimitAttribute(Type requestType) => requestType.GetCustomAttributes(typeof(RateLimitAttribute), inherit: true)
            .Cast<RateLimitAttribute>()
            .FirstOrDefault();
}

/// <summary>
/// Configuration options for rate limiting behavior.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Whether rate limiting is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default rate limit policy if none specified on request.
    /// </summary>
    public RateLimitPolicy? DefaultPolicy { get; set; }

    /// <summary>
    /// Strategy for identifying clients.
    /// </summary>
    public ClientIdentificationStrategy ClientIdentificationStrategy { get; set; } = ClientIdentificationStrategy.IpAddress;
}

/// <summary>
/// Rate limit policy configuration.
/// </summary>
public class RateLimitPolicy
{
    /// <summary>
    /// Maximum number of requests allowed.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Time window for the limit.
    /// </summary>
    public TimeSpan Window { get; set; }

    /// <summary>
    /// Rate limiting algorithm to use.
    /// </summary>
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.SlidingWindow;
}

/// <summary>
/// Attribute to specify rate limits for CRISP requests.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RateLimitAttribute : Attribute
{
    public int Limit { get; }
    public int WindowSeconds { get; }
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.SlidingWindow;

    public RateLimitAttribute(int limit, int windowSeconds)
    {
        Limit = limit;
        WindowSeconds = windowSeconds;
    }

    public RateLimitPolicy ToPolicy() => new()
    {
        Limit = Limit,
        Window = TimeSpan.FromSeconds(WindowSeconds),
        Algorithm = Algorithm
    };
}

/// <summary>
/// Rate limiting algorithms.
/// </summary>
public enum RateLimitAlgorithm
{
    /// <summary>
    /// Fixed time window algorithm.
    /// </summary>
    FixedWindow,

    /// <summary>
    /// Sliding time window algorithm.
    /// </summary>
    SlidingWindow,

    /// <summary>
    /// Token bucket algorithm.
    /// </summary>
    TokenBucket
}

/// <summary>
/// Client identification strategies.
/// </summary>
public enum ClientIdentificationStrategy
{
    /// <summary>
    /// Use client IP address.
    /// </summary>
    IpAddress,

    /// <summary>
    /// Use authenticated user ID.
    /// </summary>
    UserId,

    /// <summary>
    /// Use API key from headers.
    /// </summary>
    ApiKey,

    /// <summary>
    /// Use composite of multiple factors.
    /// </summary>
    Composite
}

/// <summary>
/// Interface for rate limit data storage.
/// </summary>
public interface IRateLimitStore
{
    Task<TokenBucket?> GetTokenBucketAsync(string key, CancellationToken cancellationToken);
    Task SetTokenBucketAsync(string key, TokenBucket bucket, TimeSpan expiry, CancellationToken cancellationToken);

    Task<SlidingWindow?> GetSlidingWindowAsync(string key, CancellationToken cancellationToken);
    Task SetSlidingWindowAsync(string key, SlidingWindow window, TimeSpan expiry, CancellationToken cancellationToken);

    Task<int> GetCounterAsync(string key, CancellationToken cancellationToken);
    Task IncrementCounterAsync(string key, TimeSpan expiry, CancellationToken cancellationToken);
}

/// <summary>
/// In-memory implementation of rate limit store.
/// </summary>
public class MemoryRateLimitStore : IRateLimitStore
{
    private readonly IMemoryCache _cache;

    public MemoryRateLimitStore(IMemoryCache cache) => _cache = cache;

    public Task<TokenBucket?> GetTokenBucketAsync(string key, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(key, out TokenBucket? bucket);
        return Task.FromResult(bucket);
    }

    public Task SetTokenBucketAsync(string key, TokenBucket bucket, TimeSpan expiry, CancellationToken cancellationToken)
    {
        _cache.Set(key, bucket, expiry);
        return Task.CompletedTask;
    }

    public Task<SlidingWindow?> GetSlidingWindowAsync(string key, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(key, out SlidingWindow? window);
        return Task.FromResult(window);
    }

    public Task SetSlidingWindowAsync(string key, SlidingWindow window, TimeSpan expiry, CancellationToken cancellationToken)
    {
        _cache.Set(key, window, expiry);
        return Task.CompletedTask;
    }

    public Task<int> GetCounterAsync(string key, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(key, out int counter);
        return Task.FromResult(counter);
    }

    public Task IncrementCounterAsync(string key, TimeSpan expiry, CancellationToken cancellationToken)
    {
        int current = _cache.TryGetValue(key, out int counter) ? counter : 0;
        _cache.Set(key, current + 1, expiry);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Token bucket for rate limiting.
/// </summary>
public class TokenBucket
{
    public int Tokens { get; set; }
    public DateTime LastRefill { get; set; }
    public int Capacity { get; set; }
}

/// <summary>
/// Sliding window for rate limiting.
/// </summary>
public class SlidingWindow
{
    public List<DateTime> Requests { get; set; } = [];
}