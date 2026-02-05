using CRISP.Server.Data;
using CRISP.Server.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

// Alias to avoid namespace collision
using TodoContracts = CRISP.Core.Todos;

namespace CRISP.Server.Features.Todos.Endpoints;

public sealed class CreateTodoEndpoint : ICreateEndpoint<TodoContracts.CreateTodo>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapPost("", Handle)
            .WithName(nameof(TodoContracts.CreateTodo))
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            ;

        return app;
    }

    public static async Task<IResult> Handle(
        [FromBody] TodoContracts.CreateTodo command,
        [FromServices] ICreateService<TodoContracts.CreateTodo> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        Guid id = await service.Send(command, cancellationToken);
        await cache.EvictByTagAsync("Todos", cancellationToken);
        return TypedResults.Created($"/todos/{id}", id);
    }
}

public sealed class CreateTodoService(ApplicationDbContext context) : ICreateService<TodoContracts.CreateTodo>
{
    public async ValueTask<Guid> Send(TodoContracts.CreateTodo command, CancellationToken cancellationToken = default)
    {
        var entity = new TodoEntity
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            DueDate = command.DueDate,
            IsCompleted = false,
        };

        context.Todos.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
