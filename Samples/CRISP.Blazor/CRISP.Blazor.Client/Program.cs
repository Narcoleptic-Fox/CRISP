using CRISP.Blazor.Client;
using CRISP.Frontend;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddCrispFromAssemblies(typeof(Routes).Assembly);

await builder.Build().RunAsync();
