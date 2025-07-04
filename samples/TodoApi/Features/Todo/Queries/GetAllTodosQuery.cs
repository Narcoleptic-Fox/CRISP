using Crisp.Queries;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo.Queries;

public record GetAllTodosQuery : IQuery<IEnumerable<TodoDto>>
{
    public string? CacheKey => "all-todos";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(2);
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);
}

public class GetAllTodosHandler : IQueryHandler<GetAllTodosQuery, IEnumerable<TodoDto>>
{
    private readonly ITodoRepository _repository;

    public GetAllTodosHandler(ITodoRepository repository) => _repository = repository;

    public async Task<IEnumerable<TodoDto>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<TodoEntity> todos = await _repository.GetAllAsync();

        return todos.Select(todo => new TodoDto(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.CompletedAt));
    }
}
