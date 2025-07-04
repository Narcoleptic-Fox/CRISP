using Microsoft.AspNetCore.Mvc.Testing;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using System.Text;
using System.Text.Json;

namespace Crisp.LoadTests;

public static class BasicLoadTest
{
    public static void Run()
    {
        Console.WriteLine("Starting Basic Load Test...");

        using WebApplicationFactory<TodoApi.Program> factory = new();
        HttpClient httpClient = factory.CreateClient();

        NBomber.Contracts.ScenarioProps createTodoScenario = Scenario.Create("create_todo", async context =>
        {
            var command = new
            {
                Title = $"Load Test Todo {context.ScenarioInfo.ScenarioName}",
                Description = "Created during load testing"
            };

            string json = JsonSerializer.Serialize(command);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("/api/create-todo", content);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(2))
        );

        NBomber.Contracts.ScenarioProps getTodosScenario = Scenario.Create("get_all_todos", async context =>
        {
            HttpResponseMessage response = await httpClient.GetAsync("/api/get-all-todos");

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(2))
        );

        NBomberRunner
            .RegisterScenarios(createTodoScenario, getTodosScenario)
            .WithReportFolder("load-test-reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
            .Run();
    }
}

public static class MixedLoadTest
{
    public static void Run()
    {
        Console.WriteLine("Starting Mixed Operations Load Test...");

        using WebApplicationFactory<TodoApi.Program> factory = new();
        HttpClient httpClient = factory.CreateClient();
        List<Guid> createdTodoIds = [];

        // Warm up - create some todos first
        Console.WriteLine("Warming up with initial data...");
        for (int i = 0; i < 10; i++)
        {
            var warmupCommand = new
            {
                Title = $"Warmup Todo {i}",
                Description = "Initial data for load testing"
            };

            string json = JsonSerializer.Serialize(warmupCommand);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = httpClient.PostAsync("/api/create-todo", content).Result;

            if (response.IsSuccessStatusCode)
            {
                string responseContent = response.Content.ReadAsStringAsync().Result;
                JsonElement todo = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (todo.TryGetProperty("id", out JsonElement id))
                {
                    createdTodoIds.Add(Guid.Parse(id.GetString()!));
                }
            }
        }

        // Heavy read scenario (70% of traffic)
        NBomber.Contracts.ScenarioProps heavyReadScenario = Scenario.Create("heavy_read", async context =>
        {
            HttpResponseMessage response = await httpClient.GetAsync("/api/get-all-todos");
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithWeight(70)
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3))
        );

        // Moderate write scenario (20% of traffic)
        NBomber.Contracts.ScenarioProps moderateWriteScenario = Scenario.Create("moderate_write", async context =>
        {
            var command = new
            {
                Title = $"Mixed Test Todo {context.ScenarioInfo.ScenarioName}",
                Description = $"Created at {DateTime.UtcNow:HH:mm:ss}"
            };

            string json = JsonSerializer.Serialize(command);
            StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("/api/create-todo", content);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithWeight(20)
        .WithLoadSimulations(
            Simulation.Inject(rate: 15, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3))
        );

        // Specific item lookup scenario (10% of traffic)
        NBomber.Contracts.ScenarioProps specificLookupScenario = Scenario.Create("specific_lookup", async context =>
        {
            if (createdTodoIds.Count == 0)
            {
                return Response.Fail(message: "No todos available for lookup");
            }

            Guid randomId = createdTodoIds[Random.Shared.Next(createdTodoIds.Count)];
            HttpResponseMessage response = await httpClient.GetAsync($"/api/get-todo?id={randomId}");

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithWeight(10)
        .WithLoadSimulations(
            Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3))
        );

        NBomberRunner
            .RegisterScenarios(heavyReadScenario, moderateWriteScenario, specificLookupScenario)
            .WithReportFolder("mixed-load-test-reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
            .Run();
    }
}

public static class StressLoadTest
{
    public static void Run()
    {
        Console.WriteLine("Starting Stress Load Test...");
        Console.WriteLine("Warning: This test applies high load to verify framework limits.");

        using WebApplicationFactory<TodoApi.Program> factory = new();
        HttpClient httpClient = factory.CreateClient();

        NBomber.Contracts.ScenarioProps stressScenario = Scenario.Create("stress_test", async context =>
        {
            Func<Task<HttpResponseMessage>>[] operations = new[]
            {
                async () => await httpClient.GetAsync("/api/get-all-todos"),
                async () => {
                    var command = new
                    {
                        Title = $"Stress Todo {context.ScenarioInfo.ScenarioName}",
                        Description = "High load test"
                    };
                    string json = JsonSerializer.Serialize(command);
                    StringContent content = new(json, Encoding.UTF8, "application/json");
                    return await httpClient.PostAsync("/api/create-todo", content);
                }
            };

            Func<Task<HttpResponseMessage>> operation = operations[Random.Shared.Next(operations.Length)];
            HttpResponseMessage response = await operation();

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithLoadSimulations(
            // Ramp up gradually to find breaking point
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            Simulation.Inject(rate: 200, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)),
            Simulation.Inject(rate: 500, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(stressScenario)
            .WithReportFolder("stress-test-reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
            .Run();
    }
}

public static class HealthCheckLoadTest
{
    public static void Run()
    {
        Console.WriteLine("Starting Health Check Load Test...");

        using WebApplicationFactory<TodoApi.Program> factory = new();
        HttpClient httpClient = factory.CreateClient();

        NBomber.Contracts.ScenarioProps basicHealthScenario = Scenario.Create("basic_health", async context =>
        {
            HttpResponseMessage response = await httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
        );

        NBomber.Contracts.ScenarioProps detailedHealthScenario = Scenario.Create("detailed_health", async context =>
        {
            HttpResponseMessage response = await httpClient.GetAsync("/health/detailed");
            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail(message: $"Failed with status: {response.StatusCode}");
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(basicHealthScenario, detailedHealthScenario)
            .WithReportFolder("health-check-reports")
            .WithReportFormats(ReportFormat.Html, ReportFormat.Csv)
            .Run();
    }
}