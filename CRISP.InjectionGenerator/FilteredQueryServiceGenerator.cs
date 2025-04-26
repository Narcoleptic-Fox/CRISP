using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CRISP.InjectionGenerator
{
    internal class FilteredQueryServiceGenerator : BaseGenerator
    {
        private const string InterfaceToFind = "CRISP.IFilteredQueryService";

        public override bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            // Find all filtered query service implementations
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> filteredQueryServices = GetAllImplementations(compilation, classes, InterfaceToFind, false);
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> openGenericFilteredQueryServices = GetAllImplementations(compilation, classes, InterfaceToFind, true);

            if (!filteredQueryServices.Any() && !openGenericFilteredQueryServices.Any())
                return false;

            string source = GenerateFilteredQueryServiceRegistration(compilation, filteredQueryServices, openGenericFilteredQueryServices);
            context.AddSource("FilteredQueryServiceRegistration.g.cs", SourceText.From(source, Encoding.UTF8));

            return true;
        }

        private string GenerateFilteredQueryServiceRegistration(
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
    public static class FilteredQueryServiceRegistrationExtensions
    {
        public static IServiceCollection RegisterFilteredQueryServices(this IServiceCollection services)
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

                // For IFilteredQueryService, we always have 2 type parameters: TQuery, TResponse
                sb.AppendLine($"            services.{registrationMethod}(typeof(CRISP.IFilteredQueryService<,>), typeof({service.Namespace}.{service.ClassName}<,>));");
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