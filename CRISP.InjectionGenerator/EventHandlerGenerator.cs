using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CRISP.InjectionGenerator
{
    internal class EventHandlerGenerator : BaseGenerator
    {
        private const string InterfaceToFind = "CRISP.Events.IEventHandler";

        public override bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context)
        {
            // Find all event handlers (both concrete and generic)
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> eventHandlers = GetAllImplementations(compilation, classes, InterfaceToFind, false);
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> openGenericEventHandlers = GetAllImplementations(compilation, classes, InterfaceToFind, true);

            if (!eventHandlers.Any() && !openGenericEventHandlers.Any())
                return false;

            string source = GenerateEventHandlerRegistration(eventHandlers, openGenericEventHandlers);
            context.AddSource("EventHandlerRegistration.g.cs", SourceText.From(source, Encoding.UTF8));

            return true;
        }

        private string GenerateEventHandlerRegistration(
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> concreteHandlers,
            System.Collections.Generic.List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints)> openGenericHandlers)
        {
            StringBuilder sb = new();
            sb.AppendLine(@"using System;
using Microsoft.Extensions.DependencyInjection;
using CRISP.Events;

namespace CRISP
{
    public static class EventHandlerRegistrationExtensions
    {
        public static IServiceCollection RegisterEventHandlers(this IServiceCollection services)
        {");

            // Register concrete handlers
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints) handler in concreteHandlers)
            {
                sb.AppendLine($"            services.AddTransient(typeof({handler.InterfaceType}), typeof({handler.Namespace}.{handler.ClassName}));");
            }

            // Register open generic handlers
            foreach ((string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, System.Collections.Generic.List<string> TypeParameterConstraints) handler in openGenericHandlers)
            {
                if (handler.TypeParameterCount > 0)
                    sb.AppendLine($"            services.AddTransient(typeof(CRISP.Events.IEventHandler<>), typeof({handler.Namespace}.{handler.ClassName}<>));");
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