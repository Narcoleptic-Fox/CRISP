using CRISP.Client.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

namespace CRISP.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthenticationStateDeserialization();

            builder.Services.AddTransient<CookieHandler>();
            builder.Services.AddHttpClient("CRISP", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                            .AddHttpMessageHandler<CookieHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("CRISP"));

            // Add this in your service configuration
            builder.Services.AddScoped<IErrorBoundaryLogger, ErrorBoundaryLogger>();

            builder.AddModules<Program>();

            builder.Services.AddSingleton(typeof(StateContainer<>));

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 3000;
            });

            await builder.Build().RunAsync();
        }
    }

    public class ErrorBoundaryLogger : IErrorBoundaryLogger
    {
        private readonly ILogger<ErrorBoundaryLogger> _logger;

        public ErrorBoundaryLogger(ILogger<ErrorBoundaryLogger> logger) => _logger = logger;

        public ValueTask LogErrorAsync(Exception exception)
        {
            _logger.LogError(exception, "Unhandled error in Blazor component");
            return ValueTask.CompletedTask;
        }
    }
}
