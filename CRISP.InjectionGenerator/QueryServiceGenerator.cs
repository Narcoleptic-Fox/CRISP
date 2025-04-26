using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CRISP.InjectionGenerator
{
    internal class QueryServiceGenerator : BaseGenerator
    {
        private const string InterfaceToFind = "CRISP.IQueryService";

        public override bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            // Find all query service implementations
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> queryServices = GetAllImplementations(compilation, classes, InterfaceToFind, false);
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> openGenericQueryServices = GetAllImplementations(compilation, classes, InterfaceToFind, true);

            if (!queryServices.Any() && !openGenericQueryServices.Any())
                return false;

            string source = GenerateQueryServiceRegistration(compilation, queryServices, openGenericQueryServices);
            context.AddSource("QueryServiceRegistration.g.cs", SourceText.From(source, Encoding.UTF8));

            return true;
        }

        private string GenerateQueryServiceRegistration(
            Compilation compilation,
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> concreteServices,
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> openGenericServices)
        {
            StringBuilder sb = new();
            sb.AppendLine(@"using System;
using Microsoft.Extensions.DependencyInjection;
using CRISP;

namespace CRISP
{
    public static class QueryServiceRegistrationExtensions
    {
        public static IServiceCollection RegisterQueryServices(this IServiceCollection services)
        {");

            // Register concrete services
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints) service in concreteServices)
            {
                // Find the symbol for the service class to check for ServiceLifetimeAttribute
                INamedTypeSymbol serviceSymbol = compilation.GetTypeByMetadataName($"{service.Namespace}.{service.ClassName}");
                string registrationMethod = "AddTransient"; // Default

                if (serviceSymbol != null)
                    registrationMethod = GetServiceRegistrationMethod(serviceSymbol);

                sb.AppendLine($"            services.{registrationMethod}(typeof({service.InterfaceType}), typeof({service.Namespace}.{service.ClassName}));");
            }

            // Register open generic services
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints) service in openGenericServices)
            {
                // Find the symbol for the service class to check for ServiceLifetimeAttribute
                INamedTypeSymbol serviceSymbol = compilation.GetTypeByMetadataName($"{service.Namespace}.{service.ClassName}`{service.TypeParameterCount}");
                string registrationMethod = "AddTransient"; // Default

                if (serviceSymbol != null)
                    registrationMethod = GetServiceRegistrationMethod(serviceSymbol);

                // For IQueryService, we always have 2 type parameters: TQuery, TResponse
                sb.AppendLine($"            services.{registrationMethod}(typeof(CRISP.IQueryService<,>), typeof({service.Namespace}.{service.ClassName}<,>));");
            }

            sb.AppendLine(@"
            return services;
        }
    }
}");
            return sb.ToString();
        }
    }
}