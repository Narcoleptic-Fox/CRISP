namespace TodoApi.Features.Todo.Models;

public record TodoDto(
    int Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record CreateTodoDto(
    string Title,
    string? Description);

public record UpdateTodoDto(
    string? Title,
    string? Description,
    bool? IsCompleted);