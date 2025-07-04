using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Crisp.Commands;
using Crisp.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Crisp.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MemoryAllocationBenchmarks
{
    private IServiceProvider _serviceProvider = null!;
    private ICommandDispatcher _commandDispatcher = null!;
    private IQueryDispatcher _queryDispatcher = null!;
    private SimpleCommand _command = null!;
    private SimpleQuery _query = null!;

    [GlobalSetup]
    public void Setup()
    {
        ServiceCollection services = new();

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(MemoryAllocationBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = false;
            crisp.Options.Pipeline.EnableErrorHandling = false;
        });

        _serviceProvider = services.BuildServiceProvider();
        _commandDispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _queryDispatcher = _serviceProvider.GetRequiredService<IQueryDispatcher>();
        _command = new SimpleCommand();
        _query = new SimpleQuery { Id = 1 };
    }

    //[GlobalCleanup]
    //public void Cleanup()
    //{
    //    _serviceProvider?.Dispose();
    //}

    [Benchmark]
    public async Task SingleCommandExecution() => await _commandDispatcher.Send(_command);

    [Benchmark]
    public async Task SingleQueryExecution() => await _queryDispatcher.Send(_query);

    [Benchmark]
    public async Task BatchCommandExecution()
    {
        Task[] tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = _commandDispatcher.Send(_command);
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task BatchQueryExecution()
    {
        Task<SimpleQueryResult>[] tasks = new Task<SimpleQueryResult>[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = _queryDispatcher.Send(_query);
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task MixedOperations()
    {
        Task[] commandTasks = new Task[5];
        Task<SimpleQueryResult>[] queryTasks = new Task<SimpleQueryResult>[5];

        for (int i = 0; i < 5; i++)
        {
            commandTasks[i] = _commandDispatcher.Send(_command);
            queryTasks[i] = _queryDispatcher.Send(_query);
        }

        await Task.WhenAll(commandTasks.Concat<Task>(queryTasks));
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ServiceResolutionBenchmarks
{
    private IServiceProvider _serviceProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        ServiceCollection services = new();

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(MemoryAllocationBenchmarks).Assembly);
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    //[GlobalCleanup]
    //public void Cleanup()
    //{
    //    _serviceProvider?.Dispose();
    //}

    [Benchmark(Baseline = true)]
    public void ResolveCommandDispatcher()
    {
        ICommandDispatcher dispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
    }

    [Benchmark]
    public void ResolveQueryDispatcher()
    {
        IQueryDispatcher dispatcher = _serviceProvider.GetRequiredService<IQueryDispatcher>();
    }

    [Benchmark]
    public void ResolveCommandHandler()
    {
        ICommandHandler<SimpleCommand> handler = _serviceProvider.GetRequiredService<ICommandHandler<SimpleCommand>>();
    }

    [Benchmark]
    public void ResolveQueryHandler()
    {
        IQueryHandler<SimpleQuery, SimpleQueryResult> handler = _serviceProvider.GetRequiredService<IQueryHandler<SimpleQuery, SimpleQueryResult>>();
    }

    [Benchmark]
    public void ResolveUsingScope()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ICommandDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
    }

    [Benchmark]
    public void ResolveMultipleServices()
    {
        ICommandDispatcher commandDispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        IQueryDispatcher queryDispatcher = _serviceProvider.GetRequiredService<IQueryDispatcher>();
        CrispOptions options = _serviceProvider.GetRequiredService<CrispOptions>();
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class StartupBenchmarks
{
    [Benchmark(Baseline = true)]
    public IServiceProvider SetupMinimalCrisp()
    {
        ServiceCollection services = new();

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(MemoryAllocationBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = false;
            crisp.Options.Pipeline.EnableErrorHandling = false;
        });

        return services.BuildServiceProvider();
    }

    [Benchmark]
    public IServiceProvider SetupFullCrisp()
    {
        ServiceCollection services = new();

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(MemoryAllocationBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = true;
            crisp.Options.Pipeline.EnableErrorHandling = true;
        });

        return services.BuildServiceProvider();
    }

    [Benchmark]
    public void CompilePipelines()
    {
        ServiceCollection services = new();

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(MemoryAllocationBenchmarks).Assembly);
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        // The compilation happens during service provider build
        // This benchmark measures the compilation time
    }

    [Benchmark]
    public void RegisterLargeNumberOfHandlers()
    {
        ServiceCollection services = new();

        // Register the same handlers multiple times to simulate a large application
        for (int i = 0; i < 100; i++)
        {
            services.AddScoped<ICommandHandler<SimpleCommand>, SimpleCommandHandler>();
            services.AddScoped<IQueryHandler<SimpleQuery, SimpleQueryResult>, SimpleQueryHandler>();
        }

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(MemoryAllocationBenchmarks).Assembly);
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
    }
}

// Test query and handlers
public class SimpleQuery : IQuery<SimpleQueryResult>
{
    public int Id { get; set; }
}

public class SimpleQueryResult
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
}

public class SimpleQueryHandler : IQueryHandler<SimpleQuery, SimpleQueryResult>
{
    public Task<SimpleQueryResult> Handle(SimpleQuery query, CancellationToken cancellationToken) => Task.FromResult(new SimpleQueryResult { Id = query.Id, Data = "Sample Data" });
}