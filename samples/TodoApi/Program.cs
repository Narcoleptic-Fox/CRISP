using Crisp.OpenApi;
using TodoApi.Features.Todo;

namespace TodoApi;
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add CRISP framework
        builder.Services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(Program).Assembly);
        });

        // Add additional services
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITodoRepository, InMemoryTodoRepository>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseCrispSwagger();
        }

        app.UseHttpsRedirection();

        // Map CRISP endpoints
        app.MapCrisp();

        app.Run();
    }
}
