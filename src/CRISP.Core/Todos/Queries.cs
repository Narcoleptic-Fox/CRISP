using System.Text;
using CRISP.Core.Common;

namespace CRISP.Core.Todos;

/// <summary>
/// Get a paged list of todos with optional filters.
/// </summary>
public sealed record GetTodos : PagedQuery<Todos>
{
    /// <summary>Filter by title (contains).</summary>
    public string? Title { get; init; }

    /// <summary>Filter by completion status.</summary>
    public bool? IsCompleted { get; init; }

    /// <summary>Filter by due date before.</summary>
    public DateTime? DueBefore { get; init; }

    /// <summary>Filter by due date after.</summary>
    public DateTime? DueAfter { get; init; }

    public override string ToQueryString()
    {
        var builder = new StringBuilder(base.ToQueryString());
        if (!string.IsNullOrEmpty(Title))
            builder.Append($"&title={Title}");
        if (IsCompleted.HasValue)
            builder.Append($"&isCompleted={IsCompleted.Value}");
        if (DueBefore.HasValue)
            builder.Append($"&dueBefore={DueBefore.Value:O}");
        if (DueAfter.HasValue)
            builder.Append($"&dueAfter={DueAfter.Value:O}");
        return builder.ToString();
    }
}
