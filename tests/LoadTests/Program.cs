using NBomber.CSharp;

namespace Crisp.LoadTests;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("CRISP Framework Load Tests");
        Console.WriteLine("==========================");
        Console.WriteLine();
        
        if (args.Length == 0)
        {
            Console.WriteLine("Available test scenarios:");
            Console.WriteLine("  basic     - Basic CRUD operations");
            Console.WriteLine("  stress    - High-load stress test");
            Console.WriteLine("  mixed     - Mixed read/write operations");
            Console.WriteLine("  health    - Health check endpoints");
            Console.WriteLine("  all       - Run all scenarios");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run <scenario>");
            return;
        }

        var scenario = args[0].ToLowerInvariant();
        
        switch (scenario)
        {
            case "basic":
                BasicLoadTest.Run();
                break;
            case "stress":
                StressLoadTest.Run();
                break;
            case "mixed":
                MixedLoadTest.Run();
                break;
            case "health":
                HealthCheckLoadTest.Run();
                break;
            case "all":
                RunAllScenarios();
                break;
            default:
                Console.WriteLine($"Unknown scenario: {scenario}");
                break;
        }
    }

    private static void RunAllScenarios()
    {
        Console.WriteLine("Running all load test scenarios...");
        
        BasicLoadTest.Run();
        Console.WriteLine("Basic load test completed. Press any key to continue...");
        Console.ReadKey();
        
        MixedLoadTest.Run();
        Console.WriteLine("Mixed load test completed. Press any key to continue...");
        Console.ReadKey();
        
        HealthCheckLoadTest.Run();
        Console.WriteLine("Health check load test completed. Press any key to continue...");
        Console.ReadKey();
        
        StressLoadTest.Run();
        Console.WriteLine("All load tests completed!");
    }
}