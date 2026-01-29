using CRISP.Core.Common;

namespace CRISP.Core.Todos;

/// <summary>
/// Todo list item for paged responses.
/// </summary>
public record Todos : BaseModel
{
    public string Title { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// Single todo item response model with full details.
/// </summary>
public sealed record Todo : Todos
{
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
