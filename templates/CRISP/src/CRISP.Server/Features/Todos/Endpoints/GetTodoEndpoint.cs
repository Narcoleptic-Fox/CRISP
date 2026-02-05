using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Alias to avoid namespace collision
using TodoContracts = CRISP.Core.Todos;

namespace CRISP.Server.Features.Todos.Endpoints;

public sealed class GetTodoEndpoint : ISingularQueryEndpoint<TodoContracts.Todo>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapGet("{id:guid}", Handle)
            .WithName("GetTodo")
            .Produces<TodoContracts.Todo>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .CacheOutput(builder => builder.Tag("Todos"))
            ;

        return app;
    }

    public static async Task<IResult> Handle(
        [AsParameters] SingularQuery<TodoContracts.Todo> query,
        [FromServices] IQueryService<SingularQuery<TodoContracts.Todo>, TodoContracts.Todo> service,
        CancellationToken cancellationToken)
    {
        TodoContracts.Todo? result = await service.Send(query, cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}

public sealed class GetTodoService(ApplicationDbContext context) : IQueryService<SingularQuery<TodoContracts.Todo>, TodoContracts.Todo>
{
    public async ValueTask<TodoContracts.Todo> Send(SingularQuery<TodoContracts.Todo> query, CancellationToken cancellationToken = default)
    {
        var entity = await context.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken);

        if (entity is null)
            return null!;

        return new TodoContracts.Todo
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            IsCompleted = entity.IsCompleted,
            DueDate = entity.DueDate,
            CreatedAt = entity.CreatedOn,
            CompletedAt = entity.CompletedAt,
        };
    }
}
