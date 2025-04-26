using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CRISP.InjectionGenerator
{
    internal class RequestHandlerGenerator : BaseGenerator
    {
        private const string InterfaceToFind = "CRISP.Requests.IRequestHandler";

        public override bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            // Find all request handlers (both concrete and generic)
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> requestHandlers = GetAllImplementations(compilation, classes, InterfaceToFind, false);
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> openGenericRequestHandlers = GetAllImplementations(compilation, classes, InterfaceToFind, true);

            if (!requestHandlers.Any() && !openGenericRequestHandlers.Any())
                return false;

            string source = GenerateRequestHandlerRegistration(requestHandlers, openGenericRequestHandlers);
            context.AddSource("RequestHandlerRegistration.g.cs", SourceText.From(source, Encoding.UTF8));

            return true;
        }

        private string GenerateRequestHandlerRegistration(
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> concreteHandlers,
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> openGenericHandlers)
        {
            StringBuilder sb = new();
            sb.AppendLine(@"using System;
using Microsoft.Extensions.DependencyInjection;
using CRISP.Requests;

namespace CRISP
{
    public static class RequestHandlerRegistrationExtensions
    {
        public static IServiceCollection RegisterRequestHandlers(this IServiceCollection services)
        {");

            // Register concrete handlers
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints) handler in concreteHandlers)
            {
                sb.AppendLine($"            services.AddTransient(typeof({handler.InterfaceType}), typeof({handler.Namespace}.{handler.ClassName}));");
            }

            // Register open generic handlers
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints) handler in openGenericHandlers)
            {
                if (handler.TypeParameterCount == 2)
                    sb.AppendLine($"            services.AddTransient(typeof(CRISP.Requests.IRequestHandler<,>), typeof({handler.Namespace}.{handler.ClassName}<,>));");
                else if (handler.TypeParameterCount == 1)
                {
                    sb.AppendLine($"            services.AddTransient(typeof(CRISP.Requests.IRequestHandler<>), typeof({handler.Namespace}.{handler.ClassName}<>));");
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