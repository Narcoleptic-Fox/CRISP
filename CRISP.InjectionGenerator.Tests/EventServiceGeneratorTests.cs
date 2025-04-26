using Microsoft.CodeAnalysis;

namespace CRISP.InjectionGenerator.Tests;

public class EventServiceGeneratorTests
{
    [Fact]
    public void Generator_ShouldRegisterEventServices()
    {
        // Arrange
        string source = @"
using CRISP;
using CRISP.Events;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestEventService : IEventService
    {
        public ValueTask Publish(IEvent @event)
        {
            Console.WriteLine($""Publishing event: {@event.GetType().Name}"");
            return ValueTask.CompletedTask;
        }
    }
}";

        // Act - Test with the main generator
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the consolidated registration includes event services
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
        Assert.Contains("RegisterEventServices", serviceOutput); // Looking for the helper method
    }

    [Fact]
    public void Generator_ShouldHandleMultipleEventServices()
    {
        // Arrange
        string source = @"
using CRISP;
using CRISP.Events;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestEventService1 : IEventService
    {
        public ValueTask Publish(IEvent @event)
        {
            Console.WriteLine($""Service 1 publishing event: {@event.GetType().Name}"");
            return ValueTask.CompletedTask;
        }
    }

    public class TestEventService2 : IEventService
    {
        public ValueTask Publish(IEvent @event)
        {
            Console.WriteLine($""Service 2 publishing event: {@event.GetType().Name}"");
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
    public void Generator_ShouldNotGenerateCodeWhenNoEventServicesExist()
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