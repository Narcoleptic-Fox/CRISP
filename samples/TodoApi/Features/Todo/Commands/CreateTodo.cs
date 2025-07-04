using Crisp.Commands;
using System.ComponentModel.DataAnnotations;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Commands;

public record CreateTodoCommand(
    [Required]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    string Title,

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    string? Description) : ICommand<TodoDto>;

public class CreateTodo : ICommandHandler<CreateTodoCommand, TodoDto>
{
    private readonly ITodoRepository _repository;

    public CreateTodo(ITodoRepository repository) => _repository = repository;

    public async Task<TodoDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        TodoEntity todo = new()
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false
        };

        TodoEntity createdTodo = await _repository.CreateAsync(todo);

        return new TodoDto(
            createdTodo.Id,
            createdTodo.Title,
            createdTodo.Description,
            createdTodo.IsCompleted,
            createdTodo.CreatedAt,
            createdTodo.CompletedAt);
    }
}
