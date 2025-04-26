using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CRISP.InjectionGenerator
{
    internal class RequestServiceGenerator : BaseGenerator
    {
        private const string InterfaceToFind = "CRISP.IRequestService";

        public override bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            // Find all request service implementations
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> requestServices = GetAllImplementations(compilation, classes, InterfaceToFind, false);
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> openGenericRequestServices = GetAllImplementations(compilation, classes, InterfaceToFind, true);

            if (!requestServices.Any() && !openGenericRequestServices.Any())
                return false;

            string source = GenerateRequestServiceRegistration(compilation, requestServices, openGenericRequestServices);
            context.AddSource("RequestServiceRegistration.g.cs", SourceText.From(source, Encoding.UTF8));

            return true;
        }

        private string GenerateRequestServiceRegistration(
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
    public static class RequestServiceRegistrationExtensions
    {
        public static IServiceCollection RegisterRequestServices(this IServiceCollection services)
        {");

            // Register concrete services
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints) service in concreteServices)
            {
                // Find the symbol for the service class to check for ServiceLifetimeAttribute
                INamedTypeSymbol serviceSymbol = compilation.GetTypeByMetadataName($"{service.Namespace}.{service.ClassName}");
                string registrationMethod = "AddTransient"; // Default

                if (serviceSymbol != null)
                    registrationMethod = GetServiceRegistrationMethod(serviceSymbol);

                sb.AppendLine($"            services.{registrationMethod}<CRISP.IRequestService, {service.Namespace}.{service.ClassName}>();");
            }

            // Register open generic services
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints) service in openGenericServices)
            {
                // Find the symbol for the service class to check for ServiceLifetimeAttribute
                INamedTypeSymbol serviceSymbol = compilation.GetTypeByMetadataName($"{service.Namespace}.{service.ClassName}`{service.TypeParameterCount}");
                string registrationMethod = "AddTransient"; // Default

                if (serviceSymbol != null)
                    registrationMethod = GetServiceRegistrationMethod(serviceSymbol);

                // If we have 2 type parameters, assume it's IRequestService<TRequest, TResponse>
                if (service.TypeParameterCount == 2)
                    sb.AppendLine($"            services.{registrationMethod}(typeof(CRISP.IRequestService<,>), typeof({service.Namespace}.{service.ClassName}<,>));");
                else if (service.TypeParameterCount == 1)
                {
                    // If we have 1 type parameter, assume it's IRequestService<TRequest>
                    sb.AppendLine($"            services.{registrationMethod}(typeof(CRISP.IRequestService<>), typeof({service.Namespace}.{service.ClassName}<>));");
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