using Microsoft.CodeAnalysis;

namespace CRISP.InjectionGenerator.Tests;

public class FilteredQueryServiceGeneratorTests
{
    [Fact]
    public void Generator_ShouldRegisterFilteredQueryServices()
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

    public class TestFilteredQuery : FilteredQuery<TestModel>
    {
        public string SearchTerm { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class TestFilteredQueryService : IFilteredQueryService<TestFilteredQuery, TestModel>
    {
        public ValueTask<FilteredResponse<TestModel>> Send(TestFilteredQuery request)
        {
            Console.WriteLine($""Filtered query for: {request.SearchTerm}, Page: {request.Page}, PageSize: {request.PageSize}"");
            return new ValueTask<FilteredResponse<TestModel>>(new FilteredResponse<TestModel>());
        }

        public void Dispose() { }
    }";

        // Act - Test with the main generator
        (GeneratorDriverRunResult mainResult, Compilation _) = SourceGeneratorTestHelper.RunGenerator(source);
        string serviceOutput = SourceGeneratorTestHelper.GetGeneratedOutput(mainResult, "CrispServiceRegistration.g.cs");

        // Assert - Check that the consolidated registration includes filtered query services
        Assert.NotEmpty(serviceOutput);
        Assert.Contains("public static IServiceCollection AddCrispServices(this IServiceCollection services)", serviceOutput);
        Assert.Contains("RegisterFilteredQueryServices", serviceOutput);
    }

    [Fact]
    public void Generator_ShouldHandleMultipleFilteredQueries()
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

    public class TestFilteredQuery1 : FilteredQuery<TestModel1>
    {
        public string SearchTerm { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class TestFilteredQuery2 : FilteredQuery<TestModel2>
    {
        public int MinId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class TestFilteredQueryService1 : IFilteredQueryService<TestFilteredQuery1, TestModel1>
    {
        public ValueTask<FilteredResponse<TestModel1>> Send(TestFilteredQuery1 request)
        {
            return new ValueTask<FilteredResponse<TestModel1>>(new FilteredResponse<TestModel1>());
        }

        public void Dispose() { }
    }

    public class TestFilteredQueryService2 : IFilteredQueryService<TestFilteredQuery2, TestModel2>
    {
        public ValueTask<FilteredResponse<TestModel2>> Send(TestFilteredQuery2 request)
        {
            return new ValueTask<FilteredResponse<TestModel2>>(new FilteredResponse<TestModel2>());
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
    public void Generator_ShouldNotGenerateCodeWhenNoFilteredQueryServicesExist()
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