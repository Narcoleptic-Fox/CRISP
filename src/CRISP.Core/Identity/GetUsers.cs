using System.Text;

namespace CRISP.Core.Identity;

/// <summary>
/// Represents a paged user search query with optional identity-specific filters.
/// </summary>
[ExportTsInterface]
public sealed record GetUsers : PagedQuery<User>
{
    /// <summary>
    /// Gets the username filter.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets the email address filter.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets the role filter.
    /// </summary>
    public Roles? Role { get; init; }

    /// <summary>
    /// Gets a filter indicating whether active users should be matched.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Converts the populated user query filters into URL query string parameters.
    /// </summary>
    /// <returns>A query string beginning with <c>?</c> that represents the populated filters.</returns>
    public override string ToQueryString()
    {
        var builder = new StringBuilder();
        builder.Append(base.ToQueryString());
        if (Username is not null)
            builder.Append($"&username={Uri.EscapeDataString(Username)}");
        if (Email is not null)
            builder.Append($"&email={Uri.EscapeDataString(Email)}");
        if (Role is not null)
            builder.Append($"&role={Role}");
        if (IsActive is not null)
            builder.Append($"&isActive={IsActive}");

        return builder.ToString();
    }
}
