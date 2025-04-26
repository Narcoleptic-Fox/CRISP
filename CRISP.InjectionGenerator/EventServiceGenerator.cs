using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CRISP.InjectionGenerator
{
    internal class EventServiceGenerator : BaseGenerator
    {
        private const string InterfaceToFind = "CRISP.IEventService";

        public override bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            // Find all event service implementations
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> eventServices = GetAllImplementations(compilation, classes, InterfaceToFind, false);
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> openGenericEventServices = GetAllImplementations(compilation, classes, InterfaceToFind, true);

            if (!eventServices.Any() && !openGenericEventServices.Any())
                return false;

            string source = GenerateEventServiceRegistration(compilation, eventServices, openGenericEventServices);
            context.AddSource("EventServiceRegistration.g.cs", SourceText.From(source, Encoding.UTF8));

            return true;
        }

        private string GenerateEventServiceRegistration(
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
    public static class EventServiceRegistrationExtensions
    {
        public static IServiceCollection RegisterEventServices(this IServiceCollection services)
        {");

            // Register concrete services
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints) service in concreteServices)
            {
                // Find the symbol for the service class to check for ServiceLifetimeAttribute
                INamedTypeSymbol serviceSymbol = compilation.GetTypeByMetadataName($"{service.Namespace}.{service.ClassName}");
                string registrationMethod = "AddTransient"; // Default

                if (serviceSymbol != null)
                    registrationMethod = GetServiceRegistrationMethod(serviceSymbol);

                sb.AppendLine($"            services.{registrationMethod}<CRISP.IEventService, {service.Namespace}.{service.ClassName}>();");
            }

            // Register open generic services
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints) service in openGenericServices)
            {
                // Find the symbol for the service class to check for ServiceLifetimeAttribute
                INamedTypeSymbol serviceSymbol = compilation.GetTypeByMetadataName($"{service.Namespace}.{service.ClassName}`{service.TypeParameterCount}");
                string registrationMethod = "AddTransient"; // Default

                if (serviceSymbol != null)
                    registrationMethod = GetServiceRegistrationMethod(serviceSymbol);

                if (service.TypeParameterCount > 0)
                {
                    string genericParams = string.Join(",", Enumerable.Repeat("T", service.TypeParameterCount));
                    sb.AppendLine($"            services.{registrationMethod}(typeof(CRISP.IEventService<{genericParams}>), typeof({service.Namespace}.{service.ClassName}<{genericParams}>));");
                }
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