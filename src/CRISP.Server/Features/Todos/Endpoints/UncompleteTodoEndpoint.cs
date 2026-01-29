using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

// Alias to avoid namespace collision
using TodoContracts = CRISP.Core.Todos;

namespace CRISP.Server.Features.Todos.Endpoints;

public sealed class UncompleteTodoEndpoint : IModifyEndpoint<TodoContracts.UncompleteTodo>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapPost("{id:guid}/uncomplete", Handle)
            .WithName(nameof(TodoContracts.UncompleteTodo))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            ;

        return app;
    }

    public static async Task<IResult> Handle(
        [FromBody] TodoContracts.UncompleteTodo command,
        [FromServices] IModifyService<TodoContracts.UncompleteTodo> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        await cache.EvictByTagAsync("Todos", cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class UncompleteTodoService(ApplicationDbContext context) : IModifyService<TodoContracts.UncompleteTodo>
{
    public async ValueTask Send(TodoContracts.UncompleteTodo command, CancellationToken cancellationToken = default)
    {
        var entity = await context.Todos.FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"Todo with ID '{command.Id}' not found.");

        entity.IsCompleted = false;
        entity.CompletedAt = null;

        await context.SaveChangesAsync(cancellationToken);
    }
}
