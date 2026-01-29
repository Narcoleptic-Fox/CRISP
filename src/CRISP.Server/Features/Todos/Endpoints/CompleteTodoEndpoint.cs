using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

// Alias to avoid namespace collision
using TodoContracts = CRISP.Core.Todos;

namespace CRISP.Server.Features.Todos.Endpoints;

public sealed class CompleteTodoEndpoint : IModifyEndpoint<TodoContracts.CompleteTodo>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapPost("{id:guid}/complete", Handle)
            .WithName(nameof(TodoContracts.CompleteTodo))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            ;

        return app;
    }

    public static async Task<IResult> Handle(
        [FromBody] TodoContracts.CompleteTodo command,
        [FromServices] IModifyService<TodoContracts.CompleteTodo> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        await cache.EvictByTagAsync("Todos", cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class CompleteTodoService(ApplicationDbContext context) : IModifyService<TodoContracts.CompleteTodo>
{
    public async ValueTask Send(TodoContracts.CompleteTodo command, CancellationToken cancellationToken = default)
    {
        var entity = await context.Todos.FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"Todo with ID '{command.Id}' not found.");

        entity.IsCompleted = true;
        entity.CompletedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
