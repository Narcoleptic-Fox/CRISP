using Crisp.Commands;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Commands;

public record UpdateTodoCommand(
    int Id,
    string? Title,
    string? Description,
    bool? IsCompleted) : ICommand<TodoDto?>;

public class UpdateTodo : ICommandHandler<UpdateTodoCommand, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public UpdateTodo(ITodoRepository repository) => _repository = repository;

    public async Task<TodoDto?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        TodoEntity? existingTodo = await _repository.GetByIdAsync(request.Id);
        if (existingTodo == null)
            return null;

        TodoEntity updatedTodo = new()
        {
            Id = existingTodo.Id,
            Title = request.Title ?? existingTodo.Title,
            Description = request.Description ?? existingTodo.Description,
            IsCompleted = request.IsCompleted ?? existingTodo.IsCompleted,
            CreatedAt = existingTodo.CreatedAt,
            CompletedAt = existingTodo.CompletedAt
        };

        TodoEntity? result = await _repository.UpdateAsync(request.Id, updatedTodo);
        return result == null
            ? null
            : new TodoDto(
            result.Id,
            result.Title,
            result.Description,
            result.IsCompleted,
            result.CreatedAt,
            result.CompletedAt);
    }
}