using Crisp.Builder;
using Crisp.Commands;
using Crisp.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Crisp.Runtime.Tests.Builder;

public class CrispBuilderBaseTests
{
    [Fact]
    public void ConfigureOptions_ShouldApplyConfiguration()
    {
        // Arrange
        ServiceCollection services = new();
        TestCrispBuilder builder = new(services);

        // Act
        builder.ConfigureOptions(options =>
        {
            options.Pipeline.EnableLogging = false;
        });

        // Assert
        builder.Options.Pipeline.EnableLogging.Should().BeFalse();
    }

    [Fact]
    public void AddAssembly_ShouldAddToAssemblies()
    {
        // Arrange
        ServiceCollection services = new();
        TestCrispBuilder builder = new(services);
        System.Reflection.Assembly assembly = typeof(CrispBuilderBaseTests).Assembly;

        // Act
        builder.AddAssembly(assembly);

        // Assert
        builder.GetHandlerMappings().Should().BeEmpty(); // Not built yet
    }

    [Fact]
    public void RegisterHandlersFromAssemblies_ShouldAddAssemblies()
    {
        // Arrange
        ServiceCollection services = new();
        TestCrispBuilder builder = new(services);
        System.Reflection.Assembly assembly = typeof(CrispBuilderBaseTests).Assembly;

        // Act
        builder.RegisterHandlersFromAssemblies(assembly);

        // Assert
        builder.GetHandlerMappings().Should().BeEmpty(); // Not built yet
    }

    [Fact]
    public void Build_WithHandlers_ShouldDiscoverAndCompile()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddScoped<TestCommandHandler>();
        services.AddScoped<TestQueryHandler>();

        TestCrispBuilder builder = new(services);
        builder.RegisterHandlersFromAssemblies(typeof(CrispBuilderBaseTests).Assembly);

        // Act
        builder.Build();

        // Assert
        builder.CommandHandlerCount.Should().BeGreaterThan(0);
        builder.QueryHandlerCount.Should().BeGreaterThan(0);
        builder.CompiledPipelineCount.Should().BeGreaterThan(0);
        builder.GetHandlerMappings().Should().NotBeEmpty();
    }

    private class TestCrispBuilder : CrispBuilderBase
    {
        public TestCrispBuilder(IServiceCollection services) : base(services) { }

        public override void Build()
        {
            DiscoverHandlers();
            RegisterHandlers();
            CompilePipelines();
            RegisterDispatchers();
        }
    }

    public record TestCommand(string Message) : ICommand<string>;
    public record TestQuery(string Filter) : IQuery<string>;

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> Handle(TestCommand request, CancellationToken cancellationToken) => Task.FromResult($"Handled: {request.Message}");
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken) => Task.FromResult($"Filtered: {request.Filter}");
    }
}