using CRISP.Server.Features.Todos.Endpoints;
using CRISP.ServiceDefaults.Features;

// Alias to avoid namespace collision
using TodoContracts = CRISP.Core.Todos;

namespace CRISP.Server.Features.Todos;

public class TodoFeature : IFeature
{
    public IServiceCollection AddFeature(IServiceCollection services)
    {
        // Commands
        services.AddScoped<ICreateService<TodoContracts.CreateTodo>, CreateTodoService>();
        services.AddScoped<IModifyService<TodoContracts.UpdateTodo>, UpdateTodoService>();
        services.AddScoped<IModifyService<TodoContracts.CompleteTodo>, CompleteTodoService>();
        services.AddScoped<IModifyService<TodoContracts.UncompleteTodo>, UncompleteTodoService>();
        services.AddKeyedScoped<IModifyService<DeleteCommand>, DeleteTodoService>("Todos");

        // Queries
        services.AddScoped<IQueryService<SingularQuery<TodoContracts.Todo>, TodoContracts.Todo>, GetTodoService>();
        services.AddScoped<IQueryService<TodoContracts.GetTodos, PagedResponse<TodoContracts.Todos>>, GetTodosService>();

        return services;
    }

    public IEndpointRouteBuilder MapFeature(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/todos")
            .WithTags("Todos")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        CreateTodoEndpoint.MapEndpoint(group);
        GetTodoEndpoint.MapEndpoint(group);
        GetTodosEndpoint.MapEndpoint(group);
        UpdateTodoEndpoint.MapEndpoint(group);
        CompleteTodoEndpoint.MapEndpoint(group);
        UncompleteTodoEndpoint.MapEndpoint(group);
        DeleteTodoEndpoint.MapEndpoint(group);

        return app;
    }
}
