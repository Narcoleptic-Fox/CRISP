using CRISP.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace CRISP.Server.Features.Todos.Endpoints;

public sealed class DeleteTodoEndpoint : IEndpoint
{
    public static RouteGroupBuilder MapEndpoint(RouteGroupBuilder app)
    {
        app.MapDelete("{id:guid}", Handle)
            .WithName("DeleteTodo")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            ;

        return app;
    }

    public static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromKeyedServices("Todos")] IModifyService<DeleteCommand> service,
        [FromServices] IEventDispatcher eventDispatcher,
        [FromServices] IOutputCacheStore cache,
        CancellationToken cancellationToken)
    {
        await service.Send(new DeleteCommand(id), cancellationToken);
        await cache.EvictByTagAsync("Todos", cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed class DeleteTodoService(ApplicationDbContext context) : IModifyService<DeleteCommand>
{
    public async ValueTask Send(DeleteCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await context.Todos.FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"Todo with ID '{command.Id}' not found.");

        // Soft delete is handled by interceptor if configured, otherwise hard delete
        context.Todos.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
