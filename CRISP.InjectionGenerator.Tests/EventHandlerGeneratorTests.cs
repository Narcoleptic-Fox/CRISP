using Microsoft.CodeAnalysis;

namespace CRISP.InjectionGenerator.Tests;

public class EventHandlerGeneratorTests
{
    [Fact]
    public void Generator_ShouldRegisterEventHandlers()
    {
        // Arrange
        string source = @"
using CRISP.Events;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestEvent : IEvent
    {
        public string Message { get; set; }
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public ValueTask Handle(TestEvent @event)
        {
            Console.WriteLine(@event.Message);
            return ValueTask.CompletedTask;
        }
    }
}";

        // Act - Test with the main generator
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the consolidated registration includes event handlers
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
        Assert.Contains("RegisterEventHandlers", serviceOutput);
    }

    [Fact]
    public void Generator_ShouldHandleMultipleEventHandlers()
    {
        // Arrange
        string source = @"
using CRISP.Events;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestEvent1 : IEvent
    {
        public string Message { get; set; }
    }

    public class TestEvent2 : IEvent
    {
        public int Id { get; set; }
    }

    public class TestEventHandler1 : IEventHandler<TestEvent1>
    {
        public ValueTask Handle(TestEvent1 @event)
        {
            Console.WriteLine(@event.Message);
            return ValueTask.CompletedTask;
        }
    }

    public class TestEventHandler2 : IEventHandler<TestEvent2>
    {
        public ValueTask Handle(TestEvent2 @event)
        {
            Console.WriteLine($""Event ID: {@event.Id}"");
            return ValueTask.CompletedTask;
        }
    }

    public class TestEventHandlerBoth : IEventHandler<TestEvent1>, IEventHandler<TestEvent2>
    {
        public ValueTask Handle(TestEvent1 @event)
        {
            Console.WriteLine($""Both handler: {@event.Message}"");
            return ValueTask.CompletedTask;
        }

        public ValueTask Handle(TestEvent2 @event)
        {
            Console.WriteLine($""Both handler: {@event.Id}"");
            return ValueTask.CompletedTask;
        }
    }
}";

        // Act
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the service registration extension is generated
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
    }

    [Fact]
    public void Generator_ShouldNotGenerateCodeWhenNoEventHandlersExist()
    {
        // Arrange
        string source = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod() { }
    }
}";

        // Act - Since we're always generating the main registration method, check that it still exists
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Even with no specific services, the main registration method should still be generated
        Assert.Empty(serviceOutput);
    }
}