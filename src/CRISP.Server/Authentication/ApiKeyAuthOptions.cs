using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace CRISP.Server.Authentication;

/// <summary>
/// Defines the API key authentication scheme metadata and hashing helper.
/// </summary>
public sealed class ApiKeyAuthOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// The default authentication scheme name for API key authentication.
    /// </summary>
    public const string DefaultScheme = "ApiKey";

    /// <summary>
    /// The request header that carries API key credentials.
    /// </summary>
    public const string HeaderName = "X-API-Key";

    /// <summary>
    /// Computes the persisted hash for an API key value.
    /// </summary>
    /// <param name="apiKey">The raw API key value.</param>
    /// <returns>The Base64-encoded SHA-256 hash of the API key.</returns>
    public static string HashKey(string apiKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hash);
    }
}
