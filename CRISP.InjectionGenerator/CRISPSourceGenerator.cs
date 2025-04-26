using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CRISP.InjectionGenerator
{
    [Generator]
    public class CRISPSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register our syntax receiver
            IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> syntaxProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
                )
                .Collect();

            // Combine with compilation
            context.RegisterSourceOutput(
                context.CompilationProvider.Combine(syntaxProvider),
                (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            try
            {
                // Execute each specialized generator
                BaseGenerator[] generators = new BaseGenerator[]
                {
                    new EventHandlerGenerator(),
                    new RequestHandlerGenerator(),
                    new EventServiceGenerator(),
                    new RequestServiceGenerator(),
                    new QueryServiceGenerator(),
                    new FilteredQueryServiceGenerator()
                };

                // Execute each generator and track which ones produced output
                List<Type> executedGenerators = [];
                foreach (BaseGenerator generator in generators)
                {
                    if (generator.Execute(compilation, classes, context))
                        executedGenerators.Add(generator.GetType());
                }

                // Generate the main extension method to register all services if any generators were executed
                if (executedGenerators.Any())
                    GenerateServiceRegistrationExtension(context, executedGenerators);
            }
            catch (Exception ex)
            {
                // Add a diagnostic for the exception
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CRISP001",
                            "CRISP Generator Error",
                            "Error in CRISP Source Generator: {0}",
                            "CRISP.SourceGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None,
                        ex.ToString()));
            }
        }

        private static void GenerateServiceRegistrationExtension(SourceProductionContext context, List<Type> executedGenerators)
        {
            StringBuilder sb = new();
            sb.AppendLine(@"using System;
using Microsoft.Extensions.DependencyInjection;

namespace CRISP
{
    public static class CrispServiceRegistrationExtensions
    {
        public static IServiceCollection AddCrispServices(this IServiceCollection services)
        {");

            // Only call registration methods for generators that were actually executed
            if (executedGenerators.Contains(typeof(EventHandlerGenerator)))
                sb.AppendLine("            services.RegisterEventHandlers();");

            if (executedGenerators.Contains(typeof(RequestHandlerGenerator)))
                sb.AppendLine("            services.RegisterRequestHandlers();");

            if (executedGenerators.Contains(typeof(EventServiceGenerator)))
                sb.AppendLine("            services.RegisterEventServices();");

            if (executedGenerators.Contains(typeof(RequestServiceGenerator)))
                sb.AppendLine("            services.RegisterRequestServices();");

            if (executedGenerators.Contains(typeof(QueryServiceGenerator)))
                sb.AppendLine("            services.RegisterQueryServices();");

            if (executedGenerators.Contains(typeof(FilteredQueryServiceGenerator)))
                sb.AppendLine("            services.RegisterFilteredQueryServices();");

            sb.AppendLine(@"
            return services;
        }
    }
}");
            context.AddSource("CrispServiceRegistration.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}