using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CRISP.InjectionGenerator
{
    internal abstract class BaseGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register for syntax notifications
            IncrementalValueProvider<ImmutableArray<ClassDeclarationSyntax>> classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is ClassDeclarationSyntax,
                    transform: (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Collect();

            // Register for compilation and syntax
            context.RegisterSourceOutput(
                context.CompilationProvider.Combine(classDeclarations),
                (spc, source) =>
                {
                    Compilation compilation = source.Left;
                    ImmutableArray<ClassDeclarationSyntax> classes = source.Right;
                    Execute(compilation, classes, spc);
                });
        }

        protected static List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)>
            GetAllImplementations(
                Compilation compilation,
                ImmutableArray<ClassDeclarationSyntax> classes,
                string interfaceToFind,
                bool onlyOpenGenerics)
        {
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> results = [];

            foreach (ClassDeclarationSyntax classDeclaration in classes)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
                    continue;

                // Skip abstract classes
                if (classSymbol.IsAbstract)
                    continue;

                // Filter based on whether we want open generics or not
                bool isOpenGeneric = classSymbol.TypeParameters.Length > 0;
                if (onlyOpenGenerics && !isOpenGeneric || !onlyOpenGenerics && isOpenGeneric)
                    continue;

                foreach (INamedTypeSymbol iface in classSymbol.AllInterfaces)
                {
                    string interfaceName = iface.OriginalDefinition.ToDisplayString();
                    if (interfaceName.StartsWith(interfaceToFind))
                    {
                        string genericArgs = iface.TypeArguments.Length > 0
                            ? string.Join(", ", iface.TypeArguments.Select(t => t.ToDisplayString()))
                            : "";

                        // For open generics, we'll use the type parameters from the class itself
                        string interfaceType = string.Empty;
                        if (isOpenGeneric)
                            // For open generics, we just use the original interface definition
                            interfaceType = interfaceName;
                        else
                        {
                            interfaceType = string.IsNullOrEmpty(genericArgs)
                                ? interfaceName
                                : $"{interfaceToFind}<{genericArgs}>";
                        }

                        // Extract type parameter constraints for open generics
                        List<string> constraints = [];
                        if (isOpenGeneric)
                        {
                            foreach (ITypeParameterSymbol typeParam in classSymbol.TypeParameters)
                            {
                                string constraint = GetTypeParameterConstraintString(typeParam);
                                if (!string.IsNullOrEmpty(constraint))
                                    constraints.Add(constraint);
                            }
                        }

                        results.Add((
                            classSymbol.Name,
                            classSymbol.ContainingNamespace.ToDisplayString(),
                            interfaceType,
                            classSymbol.TypeParameters.Length,
                            constraints
                        ));
                        break;
                    }
                }
            }

            return results;
        }

        protected static string GetTypeParameterConstraintString(ITypeParameterSymbol typeParameter)
        {
            List<string> constraints = [];

            // Check if the type parameter has "class" constraint
            if (typeParameter.HasReferenceTypeConstraint)
                constraints.Add("class");

            // Check if the type parameter has "struct" constraint
            if (typeParameter.HasValueTypeConstraint)
                constraints.Add("struct");

            // Check if the type parameter has "new()" constraint (parameterless constructor)
            if (typeParameter.HasConstructorConstraint)
                constraints.Add("new()");

            // Add any interface or base class constraints
            foreach (ITypeSymbol constraintType in typeParameter.ConstraintTypes)
            {
                constraints.Add(constraintType.ToDisplayString());
            }

            return constraints.Count == 0 ? string.Empty : $"where {typeParameter.Name} : {string.Join(", ", constraints)}";
        }

        protected static string GetServiceRegistrationMethod(INamedTypeSymbol classSymbol)
        {
            // Look for ServiceLifetimeAttribute on the class
            AttributeData attributeData = classSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "CRISP.ServiceLifetimeAttribute");

            if (attributeData != null && attributeData.ConstructorArguments.Length > 0)
            {
                // Get the ServiceLifetime enum value
                object lifetimeValue = attributeData.ConstructorArguments[0].Value;
                if (lifetimeValue != null)
                {
                    switch ((int)lifetimeValue)
                    {
                        case 0: // ServiceLifetime.Singleton
                            return "AddSingleton";
                        case 1: // ServiceLifetime.Scoped
                            return "AddScoped";
                        case 2: // ServiceLifetime.Transient
                            return "AddTransient";
                    }
                }
            }

            // Default to Transient if no attribute is found or if the value is invalid
            return "AddTransient";
        }

        protected static bool IsFromCrispNamespace(INamedTypeSymbol symbol) => symbol.ContainingNamespace?.ToDisplayString().StartsWith("CRISP") == true;

        public abstract bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context);
    }
}