using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Crisp.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Crisp.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CommandDispatcherBenchmarks
{
    private IServiceProvider _serviceProvider = null!;
    private ICommandDispatcher _commandDispatcher = null!;
    private SimpleCommand _command = null!;
    private CommandWithResult _commandWithResult = null!;

    [GlobalSetup]
    public void Setup()
    {
        ServiceCollection services = new();

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(CommandDispatcherBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = false; // Disable for pure performance
            crisp.Options.Pipeline.EnableErrorHandling = false;
        });

        _serviceProvider = services.BuildServiceProvider();
        _commandDispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _command = new SimpleCommand();
        _commandWithResult = new CommandWithResult { Value = 42 };
    }

    //[GlobalCleanup]
    //public void Cleanup()
    //{
    //    _serviceProvider?.Dispose();
    //}

    [Benchmark(Baseline = true)]
    public async Task DirectHandlerInvocation()
    {
        SimpleCommandHandler handler = new();
        await handler.Handle(_command);
    }

    [Benchmark]
    public async Task CrispCommandDispatcher_VoidCommand() => await _commandDispatcher.Send(_command);

    [Benchmark]
    public async Task CrispCommandDispatcher_CommandWithResult() => await _commandDispatcher.Send(_commandWithResult);

    [Benchmark]
    public async Task CrispCommandDispatcher_MultipleCommands()
    {
        Task[] tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = _commandDispatcher.Send(_command);
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task CrispCommandDispatcher_ConcurrentCommands()
    {
        Task[] tasks = new Task[100];
        for (int i = 0; i < 100; i++)
        {
            tasks[i] = Task.Run(() => _commandDispatcher.Send(_command));
        }
        await Task.WhenAll(tasks);
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CommandDispatcherWithPipelineBenchmarks
{
    private IServiceProvider _serviceProvider = null!;
    private ICommandDispatcher _commandDispatcher = null!;
    private SimpleCommand _command = null!;

    [GlobalSetup]
    public void Setup()
    {
        ServiceCollection services = new();

        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(CommandDispatcherBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = true;
            crisp.Options.Pipeline.EnableErrorHandling = true;
        });

        _serviceProvider = services.BuildServiceProvider();
        _commandDispatcher = _serviceProvider.GetRequiredService<ICommandDispatcher>();
        _command = new SimpleCommand();
    }

    //[GlobalCleanup]
    //public void Cleanup()
    //{
    //    _serviceProvider?.Dispose();
    //}

    [Benchmark(Baseline = true)]
    public async Task WithoutPipeline()
    {
        // Create a new service provider without pipeline behaviors for comparison
        ServiceCollection services = new();
        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(CommandDispatcherBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = false;
            crisp.Options.Pipeline.EnableErrorHandling = false;
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICommandDispatcher dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(_command);
    }

    [Benchmark]
    public async Task WithFullPipeline() => await _commandDispatcher.Send(_command);

    [Benchmark]
    public async Task WithLoggingOnly()
    {
        ServiceCollection services = new();
        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(CommandDispatcherBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = true;
            crisp.Options.Pipeline.EnableErrorHandling = false;
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICommandDispatcher dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(_command);
    }

    [Benchmark]
    public async Task WithValidationOnly()
    {
        ServiceCollection services = new();
        services.AddCrisp(crisp =>
        {
            crisp.RegisterHandlersFromAssemblies(typeof(CommandDispatcherBenchmarks).Assembly);
            crisp.Options.Pipeline.EnableLogging = false;
            crisp.Options.Pipeline.EnableErrorHandling = false;
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICommandDispatcher dispatcher = serviceProvider.GetRequiredService<ICommandDispatcher>();
        await dispatcher.Send(_command);
    }
}

// Test commands and handlers
public record SimpleCommand : ICommand;

public class SimpleCommandHandler : ICommandHandler<SimpleCommand>
{
    public Task Handle(SimpleCommand command, CancellationToken cancellationToken = default) =>
        // Simulate minimal work
        Task.CompletedTask;
}

public record CommandWithResult : ICommand<int>
{
    public int Value { get; set; }
}

public class CommandWithResultHandler : ICommandHandler<CommandWithResult, int>
{
    public Task<int> Handle(CommandWithResult command, CancellationToken cancellationToken = default) =>
        // Simulate minimal work with result
        Task.FromResult(command.Value * 2);
}