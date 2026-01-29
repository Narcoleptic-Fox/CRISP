using CRISP.Core.Common;

namespace CRISP.Core.Todos;

/// <summary>
/// Create a new todo item.
/// </summary>
public sealed record CreateTodo : CreateCommand
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// Update an existing todo item.
/// </summary>
public sealed record UpdateTodo : ModifyCommand
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// Mark a todo as completed.
/// </summary>
public sealed record CompleteTodo : ModifyCommand;

/// <summary>
/// Mark a completed todo as not completed.
/// </summary>
public sealed record UncompleteTodo : ModifyCommand;
