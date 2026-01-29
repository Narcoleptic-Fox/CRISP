using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

// Alias to avoid namespace collision
using TodoContracts = CRISP.Core.Todos;

namespace CRISP.Server.Features.Todos.Endpoints;

public sealed class UpdateTodoEndpoint : IModifyEndpoint<TodoContracts.UpdateTodo>
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapPut("{id:guid}", Handle)
            .WithName(nameof(TodoContracts.UpdateTodo))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            ;

        return app;
    }

    public static async Task<IResult> Handle(
        [FromBody] TodoContracts.UpdateTodo command,
        [FromServices] IModifyService<TodoContracts.UpdateTodo> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(command, cancellationToken);
        await cache.EvictByTagAsync("Todos", cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class UpdateTodoService(ApplicationDbContext context) : IModifyService<TodoContracts.UpdateTodo>
{
    public async ValueTask Send(TodoContracts.UpdateTodo command, CancellationToken cancellationToken = default)
    {
        var entity = await context.Todos.FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"Todo with ID '{command.Id}' not found.");

        entity.Title = command.Title;
        entity.Description = command.Description;
        entity.DueDate = command.DueDate;

        await context.SaveChangesAsync(cancellationToken);
    }
}
