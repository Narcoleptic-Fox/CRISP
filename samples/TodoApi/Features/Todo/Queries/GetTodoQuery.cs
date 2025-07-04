using Crisp.Queries;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Queries;

public record GetTodoQuery(int Id) : IQuery<TodoDto?>;

public class GetTodoHandler : IQueryHandler<GetTodoQuery, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public GetTodoHandler(ITodoRepository repository) => _repository = repository;

    public async Task<TodoDto?> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        TodoEntity? todo = await _repository.GetByIdAsync(request.Id);
        return todo == null
            ? null
            : new TodoDto(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.CompletedAt);
    }
}