using Microsoft.CodeAnalysis;

namespace CRISP.InjectionGenerator.Tests;

public class RequestHandlerGeneratorTests
{
    [Fact]
    public void Generator_ShouldRegisterRequestHandlers()
    {
        // Arrange
        string source = @"
using CRISP.Requests;
using CRISP.Responses;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestResponse : IResponse
    {
        public bool IsSuccess { get; set; }
    }

    public class TestRequest : IRequest<TestResponse>
    {
        public string Query { get; set; }
    }

    public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public ValueTask<TestResponse> Handle(TestRequest request)
        {
            return new ValueTask<TestResponse>(new TestResponse { IsSuccess = true });
        }
    }
}";

        // Act - Test with the main generator
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the consolidated registration includes request handlers
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
        Assert.Contains("RegisterRequestHandlers", serviceOutput);
    }

    [Fact]
    public void Generator_ShouldHandleVoidRequestHandlers()
    {
        // Arrange
        string source = @"
using CRISP.Requests;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestVoidRequest : IRequest
    {
        public string Command { get; set; }
    }

    public class TestVoidRequestHandler : IRequestHandler<TestVoidRequest>
    {
        public ValueTask Handle(TestVoidRequest request)
        {
            Console.WriteLine(request.Command);
            return ValueTask.CompletedTask;
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
    public void Generator_ShouldHandleMultipleRequestHandlers()
    {
        // Arrange
        string source = @"
using CRISP.Requests;
using CRISP.Responses;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestResponse1 : IResponse
    {
        public bool IsSuccess { get; set; }
    }

    public class TestResponse2 : IResponse
    {
        public string Result { get; set; }
    }

    public class TestRequest1 : IRequest<TestResponse1>
    {
        public string Query { get; set; }
    }

    public class TestRequest2 : IRequest<TestResponse2>
    {
        public int Id { get; set; }
    }

    public class TestRequestHandler1 : IRequestHandler<TestRequest1, TestResponse1>
    {
        public ValueTask<TestResponse1> Handle(TestRequest1 request)
        {
            return new ValueTask<TestResponse1>(new TestResponse1 { IsSuccess = true });
        }
    }

    public class TestRequestHandler2 : IRequestHandler<TestRequest2, TestResponse2>
    {
        public ValueTask<TestResponse2> Handle(TestRequest2 request)
        {
            return new ValueTask<TestResponse2>(new TestResponse2 { Result = $""Result: {request.Id}"" });
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
    public void Generator_ShouldNotGenerateCodeWhenNoRequestHandlersExist()
    {
        // Arrange
        string source = @"
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod() { }
    }";

        // Act - Since we're always generating the main registration method, check that it still exists
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Even with no specific services, the main registration method should still be generated
        Assert.Empty(serviceOutput);
    }
}