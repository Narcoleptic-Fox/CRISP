using Microsoft.CodeAnalysis;

namespace CRISP.InjectionGenerator.Tests;

public class QueryServiceGeneratorTests
{
    [Fact]
    public void Generator_ShouldRegisterQueryServices()
    {
        // Arrange
        string source = @"
using CRISP;
using CRISP.Models;
using CRISP.Requests;
using CRISP.Responses;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestModel : BaseModel
    {
        public string Name { get; set; }
    }

    public class TestQuery : Query<TestModel>
    {
        public string SearchTerm { get; set; }
    }

    public class TestQueryService : IQueryService<TestQuery, TestModel>
    {
        public ValueTask<TestModel> Send(TestQuery request)
        {
            Console.WriteLine($""Querying for: {request.SearchTerm}"");
            return new ValueTask<TestModel>(new TestModel { Name = request.SearchTerm });
        }

        public void Dispose() { }
    }";

        // Act - Test with the main generator
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the consolidated registration includes query services
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
        Assert.Contains("RegisterQueryServices", serviceOutput);
    }

    [Fact]
    public void Generator_ShouldHandleMultipleQueries()
    {
        // Arrange
        string source = @"
using CRISP;
using CRISP.Models;
using CRISP.Requests;
using CRISP.Responses;
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestModel1 : BaseModel
    {
        public string Name { get; set; }
    }

    public class TestModel2 : BaseModel
    {
        public int Id { get; set; }
    }

    public class TestQuery1 : Query<TestModel1>
    {
        public string SearchTerm { get; set; }
    }

    public class TestQuery2 : Query<TestModel2>
    {
        public int MinId { get; set; }
    }

    public class TestQueryService1 : IQueryService<TestQuery1, TestModel1>
    {
        public ValueTask<TestModel1> Send(TestQuery1 request)
        {
            return new ValueTask<TestModel1>(new TestModel1 { Name = request.SearchTerm });
        }

        public void Dispose() { }
    }

    public class TestQueryService2 : IQueryService<TestQuery2, TestModel2>
    {
        public ValueTask<TestModel2> Send(TestQuery2 request)
        {
            return new ValueTask<TestModel2>(new TestModel2 { Id = request.MinId });
        }

        public void Dispose() { }
    }";

        // Act
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the service registration extension is generated
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
    }

    [Fact]
    public void Generator_ShouldNotGenerateCodeWhenNoQueryServicesExist()
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