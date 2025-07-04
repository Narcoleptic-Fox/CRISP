using Crisp.Common;
using Crisp.Pipeline;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Crisp.AspNetCore.Security;

/// <summary>
/// Pipeline behavior that sanitizes input to prevent injection attacks.
/// Automatically cleans string properties in request objects.
/// </summary>
public class InputSanitizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly InputSanitizationOptions _options;
    private readonly ILogger<InputSanitizationBehavior<TRequest, TResponse>> _logger;
    private readonly HtmlEncoder _htmlEncoder;

    public InputSanitizationBehavior(
        InputSanitizationOptions options,
        ILogger<InputSanitizationBehavior<TRequest, TResponse>> logger,
        HtmlEncoder htmlEncoder)
    {
        _options = options;
        _logger = logger;
        _htmlEncoder = htmlEncoder;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_options.Enabled)
        {
            SanitizeObject(request);
        }

        return await next();
    }

    private void SanitizeObject(object? obj, int depth = 0)
    {
        if (obj == null || depth > _options.MaxDepth)
            return;

        Type type = obj.GetType();

        // Skip primitive types and known safe types
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
            type == typeof(DateTime) || type == typeof(Guid))
            return;

        IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite);

        foreach (PropertyInfo? property in properties)
        {
            try
            {
                object? value = property.GetValue(obj);

                if (value == null)
                    continue;

                if (property.PropertyType == typeof(string))
                {
                    string stringValue = (string)value;
                    string sanitized = SanitizeString(stringValue);

                    if (sanitized != stringValue)
                    {
                        property.SetValue(obj, sanitized);
                        _logger.LogDebug("Sanitized property {PropertyName} in {TypeName}",
                            property.Name, type.Name);
                    }
                }
                else if (property.PropertyType.IsClass)
                {
                    // Recursively sanitize nested objects
                    SanitizeObject(value, depth + 1);
                }
                else if (IsEnumerableType(property.PropertyType))
                {
                    // Handle collections
                    SanitizeEnumerable(value, depth + 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sanitize property {PropertyName} in {TypeName}",
                    property.Name, type.Name);
            }
        }
    }

    private void SanitizeEnumerable(object enumerable, int depth)
    {
        if (enumerable is System.Collections.IEnumerable items)
        {
            foreach (object? item in items)
            {
                if (item != null)
                {
                    SanitizeObject(item, depth);
                }
            }
        }
    }

    private string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string sanitized = input;

        // Remove SQL injection patterns
        if (_options.PreventSqlInjection)
        {
            sanitized = RemoveSqlInjectionPatterns(sanitized);
        }

        // Remove XSS patterns
        if (_options.PreventXss)
        {
            sanitized = RemoveXssPatterns(sanitized);
        }

        // HTML encode if requested
        if (_options.HtmlEncode)
        {
            sanitized = _htmlEncoder.Encode(sanitized);
        }

        // Remove dangerous characters
        if (_options.RemoveDangerousChars)
        {
            sanitized = RemoveDangerousCharacters(sanitized);
        }

        // Normalize whitespace
        if (_options.NormalizeWhitespace)
        {
            sanitized = NormalizeWhitespace(sanitized);
        }

        // Trim if needed
        if (_options.TrimStrings)
        {
            sanitized = sanitized.Trim();
        }

        // Enforce max length
        if (_options.MaxStringLength > 0 && sanitized.Length > _options.MaxStringLength)
        {
            sanitized = sanitized[.._options.MaxStringLength];
        }

        return sanitized;
    }

    private static string RemoveSqlInjectionPatterns(string input)
    {
        string[] patterns = new[]
        {
            @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|UPDATE|UNION( +ALL)?)\b)",
            @"(\b(AND|OR)\b.{1,6}?(=|>|<|\!|\||&))",
            @"(\b(CHAR|NCHAR|VARCHAR|NVARCHAR)\s*\(\s*\d+\s*\))",
            @"('|('')|;|--|\*|\+|\||\\)",
            @"(\%27)|(\%3B)|(\%2D)|(\%2A)"
        };

        foreach (string? pattern in patterns)
        {
            input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        return input;
    }

    private static string RemoveXssPatterns(string input)
    {
        string[] patterns = new[]
        {
            @"<script[^>]*>.*?</script>",
            @"<iframe[^>]*>.*?</iframe>",
            @"<object[^>]*>.*?</object>",
            @"<embed[^>]*>.*?</embed>",
            @"<link[^>]*>",
            @"<meta[^>]*>",
            @"javascript:",
            @"vbscript:",
            @"onload\s*=",
            @"onerror\s*=",
            @"onclick\s*=",
            @"onmouseover\s*="
        };

        foreach (string? pattern in patterns)
        {
            input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        return input;
    }

    private static string RemoveDangerousCharacters(string input)
    {
        // Remove null bytes and other dangerous characters
        char[] dangerous = new char[] { '\0', '\x1A', '\x1B', '\x7F' };

        foreach (char ch in dangerous)
        {
            input = input.Replace(ch.ToString(), "");
        }

        return input;
    }

    private static string NormalizeWhitespace(string input) =>
        // Replace multiple whitespace with single space
        Regex.Replace(input, @"\s+", " ");

    private static bool IsEnumerableType(Type type) => type != typeof(string) &&
               typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
}

/// <summary>
/// Configuration options for input sanitization behavior.
/// </summary>
public class InputSanitizationOptions
{
    /// <summary>
    /// Whether input sanitization is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum depth to traverse when sanitizing nested objects.
    /// </summary>
    public int MaxDepth { get; set; } = 10;

    /// <summary>
    /// Whether to prevent SQL injection patterns.
    /// </summary>
    public bool PreventSqlInjection { get; set; } = true;

    /// <summary>
    /// Whether to prevent XSS patterns.
    /// </summary>
    public bool PreventXss { get; set; } = true;

    /// <summary>
    /// Whether to HTML encode string values.
    /// </summary>
    public bool HtmlEncode { get; set; } = false;

    /// <summary>
    /// Whether to remove dangerous characters.
    /// </summary>
    public bool RemoveDangerousChars { get; set; } = true;

    /// <summary>
    /// Whether to normalize whitespace.
    /// </summary>
    public bool NormalizeWhitespace { get; set; } = true;

    /// <summary>
    /// Whether to trim string values.
    /// </summary>
    public bool TrimStrings { get; set; } = true;

    /// <summary>
    /// Maximum allowed length for string values (0 = no limit).
    /// </summary>
    public int MaxStringLength { get; set; } = 0;
}

/// <summary>
/// Attribute to skip sanitization for specific properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SkipSanitizationAttribute : Attribute
{
}

/// <summary>
/// Attribute to customize sanitization for specific properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SanitizationOptionsAttribute : Attribute
{
    public bool PreventSqlInjection { get; set; } = true;
    public bool PreventXss { get; set; } = true;
    public bool HtmlEncode { get; set; } = false;
    public bool RemoveDangerousChars { get; set; } = true;
    public int MaxLength { get; set; } = 0;
}