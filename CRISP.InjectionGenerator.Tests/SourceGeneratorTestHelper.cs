using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace CRISP.InjectionGenerator.Tests;

public static class SourceGeneratorTestHelper
{
    public static (GeneratorDriverRunResult RunResult, Compilation OutputCompilation) RunGenerator(string source) => RunGenerator(source, new CRISPSourceGenerator());

    public static (GeneratorDriverRunResult RunResult, Compilation OutputCompilation) RunGenerator(string source, IIncrementalGenerator generator)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references for necessary assemblies
        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions).Assembly.Location)
        ];

        // Add reference to the CRISP library
        Assembly crispAssembly = typeof(Events.IEvent).Assembly;
        references.Add(MetadataReference.CreateFromFile(crispAssembly.Location));

        // Create a compilation
        CSharpCompilation compilation = CSharpCompilation.Create(
            "TestCompilation",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Use GeneratorDriver directly instead of CSharpGeneratorDriver
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new[] { generator.AsSourceGenerator() },
            additionalTexts: ImmutableArray<AdditionalText>.Empty,
            parseOptions: (CSharpParseOptions)syntaxTree.Options,
            driverOptions: default);

        // Run the generator
        driver = driver.RunGenerators(compilation);
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        return (runResult, compilation);
    }

    public static string GetGeneratedOutput(GeneratorDriverRunResult result, string fileName)
    {
        // Find the generated file by name
        SyntaxTree? generatedFile = result.GeneratedTrees
            .FirstOrDefault(t => t.FilePath.EndsWith(fileName));

        return generatedFile?.GetText().ToString() ?? string.Empty;
    }
}