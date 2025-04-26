using Microsoft.CodeAnalysis;

namespace CRISP.InjectionGenerator.Tests;

public class RequestServiceGeneratorTests
{
    [Fact]
    public void Generator_ShouldRegisterRequestServices()
    {
        // Arrange
        string source = @"
using CRISP;
using CRISP.Requests;
using CRISP.Responses;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestRequestService : IRequestService<TestRequest, TestResponse>
    {
        public ValueTask<TestResponse> Send(TestRequest request)
        {
            Console.WriteLine($""Sending request: {typeof(TestRequest).Name}"");
            return new ValueTask<TestResponse>(new TestResponse());
        }

        public void Dispose() { }
    }

    public class TestRequest : IRequest<TestResponse>
    {
    }

    public class TestResponse : IResponse
    {
    }
}";

        // Act - Test with the main generator
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the consolidated registration includes request services
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
        Assert.Contains("RegisterRequestServices", serviceOutput);
    }

    [Fact]
    public void Generator_ShouldHandleMultipleRequestServices()
    {
        // Arrange
        string source = @"
using CRISP;
using CRISP.Requests;
using CRISP.Responses;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestRequestService1 : IRequestService<TestRequest1, TestResponse>
    {
        public ValueTask<TestResponse> Send(TestRequest1 request)
        {
            Console.WriteLine($""Service 1 sending request: {typeof(TestRequest1).Name}"");
            return new ValueTask<TestResponse>(new TestResponse());
        }

        public void Dispose() { }
    }

    public class TestRequestService2 : IRequestService<TestRequest2, TestResponse>
    {
        public ValueTask<TestResponse> Send(TestRequest2 request)
        {
            Console.WriteLine($""Service 2 sending request: {typeof(TestRequest2).Name}"");
            return new ValueTask<TestResponse>(new TestResponse());
        }

        public void Dispose() { }
    }

    public class TestRequest1 : IRequest<TestResponse>
    {
    }

    public class TestRequest2 : IRequest<TestResponse>
    {
    }

    public class TestResponse : IResponse
    {
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
    public void Generator_ShouldNotGenerateCodeWhenNoRequestServicesExist()
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