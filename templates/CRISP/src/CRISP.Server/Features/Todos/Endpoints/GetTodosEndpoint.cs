using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Alias to avoid namespace collision
using TodoContracts = CRISP.Core.Todos;

namespace CRISP.Server.Features.Todos.Endpoints;

public sealed class GetTodosEndpoint : IPagedQueryEndpoint<TodoContracts.GetTodos, TodoContracts.Todos>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("", Handle)
            .WithName(nameof(TodoContracts.GetTodos))
            .Produces<PagedResponse<TodoContracts.Todos>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .CacheOutput(builder => builder
                .SetVaryByQuery(
                    nameof(TodoContracts.GetTodos.Page),
                    nameof(TodoContracts.GetTodos.PageSize),
                    nameof(TodoContracts.GetTodos.SortBy),
                    nameof(TodoContracts.GetTodos.SortDescending),
                    nameof(TodoContracts.GetTodos.Title),
                    nameof(TodoContracts.GetTodos.IsCompleted),
                    nameof(TodoContracts.GetTodos.DueBefore),
                    nameof(TodoContracts.GetTodos.DueAfter))
                .Tag("Todos"))
            ;

        return app;
    }

    public static async Task<IResult> Handle(
        [AsParameters] TodoContracts.GetTodos query,
        [FromServices] IQueryService<TodoContracts.GetTodos, PagedResponse<TodoContracts.Todos>> service,
        CancellationToken cancellationToken)
    {
        PagedResponse<TodoContracts.Todos> response = await service.Send(query, cancellationToken);
        return TypedResults.Ok(response);
    }
}

public sealed class GetTodosService(ApplicationDbContext context) : IQueryService<TodoContracts.GetTodos, PagedResponse<TodoContracts.Todos>>
{
    public async ValueTask<PagedResponse<TodoContracts.Todos>> Send(TodoContracts.GetTodos query, CancellationToken cancellationToken = default)
    {
        var todosQuery = context.Todos.AsNoTracking().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.Title))
            todosQuery = todosQuery.Where(t => t.Title.Contains(query.Title));

        if (query.IsCompleted.HasValue)
            todosQuery = todosQuery.Where(t => t.IsCompleted == query.IsCompleted.Value);

        if (query.DueBefore.HasValue)
            todosQuery = todosQuery.Where(t => t.DueDate <= query.DueBefore.Value);

        if (query.DueAfter.HasValue)
            todosQuery = todosQuery.Where(t => t.DueDate >= query.DueAfter.Value);

        int totalCount = await todosQuery.CountAsync(cancellationToken);

        var items = await todosQuery
            .OrderBy(query.GetSortByOrDefault(), query.GetSortDescendingOrDefault())
            .Skip(query.GetPageOrDefault() * query.GetPageSizeOrDefault())
            .Take(query.GetPageSizeOrDefault())
            .Select(t => new TodoContracts.Todos
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted,
                DueDate = t.DueDate,
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<TodoContracts.Todos>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }
}
