using Crisp.Builder;
using Crisp.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Crisp.AspNetCore.Tests.Builder;

public class NetCoreCrispBuilderTests
{
    [Fact]
    public void Build_ShouldDiscoverAndRegisterHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NetCoreCrispBuilder(services);
        builder.RegisterHandlersFromAssemblies(typeof(NetCoreCrispBuilderTests).Assembly);

        // Act
        builder.Build();

        // Assert
        builder.CommandHandlerCount.Should().BeGreaterThan(0);
        builder.CompiledPipelineCount.Should().BeGreaterThan(0);
        builder.CompilationTime.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Build_WithNoHandlers_ShouldCompleteSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NetCoreCrispBuilder(services);

        // Act
        builder.Build();

        // Assert
        builder.CommandHandlerCount.Should().Be(0);
        builder.QueryHandlerCount.Should().Be(0);
        builder.CompiledPipelineCount.Should().Be(0);
    }

    [Fact]
    public void CompilationTime_ShouldBeAvailableAfterBuild()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NetCoreCrispBuilder(services);

        // Act
        builder.Build();

        // Assert
        builder.CompilationTime.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Build_ShouldRegisterBuilderAsService()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NetCoreCrispBuilder(services);

        // Act
        builder.Build();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var registeredBuilder = serviceProvider.GetService<ICrispBuilder>();
        registeredBuilder.Should().NotBeNull();
        registeredBuilder.Should().BeSameAs(builder);
    }

    // Test command for discovery
    public record TestCommand(string Message) : ICommand<string>;

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Handled: {request.Message}");
        }
    }
}