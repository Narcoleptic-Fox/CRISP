using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace CRISP.InjectionGenerator
{
    internal abstract class BaseGenerator : IIncrementalGenerator
    {
        // Default options to use if no ICRISPOptions implementation is found
        private static readonly (bool IncludeOpenGenerics, bool ScanReferencedAssemblies, List<string> AssembliesToScan, List<string> ExcludedAssemblies) DefaultOptions =
            (true, false, new List<string>(), new List<string>());

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
            // Try to find ICRISPOptions implementation
            var (includeOpenGenerics, scanReferencedAssemblies, assembliesToScan, excludedAssemblies) = 
                FindCRISPOptions(compilation, classes);

            // If we're looking for open generics but options say don't include them, return empty
            if (onlyOpenGenerics && !includeOpenGenerics)
                return [];

            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> results = [];

            // First, process class declarations from the current compilation
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

                ProcessTypeSymbolForInterface(classSymbol, interfaceToFind, isOpenGeneric, results);
            }

            // Only scan reference assemblies if enabled in options
            if (scanReferencedAssemblies)
            {
                foreach (var reference in compilation.References)
                {
                    if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                        continue;

                    string assemblyName = assemblySymbol.Name;
                    
                    // Skip system and Microsoft assemblies to improve performance, plus any excluded assemblies
                    if (IsSystemAssembly(assemblyName) || 
                        excludedAssemblies.Contains(assemblyName))
                        continue;
                    
                    // If we have a specific list of assemblies to scan and this one isn't in it, skip
                    if (assembliesToScan.Count > 0 && !assembliesToScan.Contains(assemblyName))
                        continue;

                    // Process all non-abstract public classes from the referenced assembly
                    foreach (INamedTypeSymbol typeSymbol in GetAllPublicClassesFromAssembly(assemblySymbol))
                    {
                        // Skip abstract classes
                        if (typeSymbol.IsAbstract)
                            continue;

                        // Filter based on whether we want open generics or not
                        bool isOpenGeneric = typeSymbol.TypeParameters.Length > 0;
                        if (onlyOpenGenerics && !isOpenGeneric || !onlyOpenGenerics && isOpenGeneric)
                            continue;

                        ProcessTypeSymbolForInterface(typeSymbol, interfaceToFind, isOpenGeneric, results);
                    }
                }
            }

            return results;
        }

        private static (bool IncludeOpenGenerics, bool ScanReferencedAssemblies, List<string> AssembliesToScan, List<string> ExcludedAssemblies) 
            FindCRISPOptions(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes)
        {
            // Look for any class that implements ICRISPOptions
            foreach (var classDeclaration in classes)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
                    continue;

                // Check if this class implements ICRISPOptions
                if (classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "CRISP.ICRISPOptions"))
                {
                    // Find the constructor or static field that would likely create this options
                    // This is a simplified approach - in reality, we can't fully determine runtime values during compilation
                    var includeOpenGenerics = TryGetBooleanPropertyValue(classSymbol, "IncludeOpenGenerics") ?? DefaultOptions.IncludeOpenGenerics;
                    var scanReferencedAssemblies = TryGetBooleanPropertyValue(classSymbol, "ScanReferencedAssemblies") ?? DefaultOptions.ScanReferencedAssemblies;
                    
                    // For list properties, we can't reliably get their values at compile time
                    // So we'll use the default empty lists
                    return (includeOpenGenerics, scanReferencedAssemblies, DefaultOptions.AssembliesToScan, DefaultOptions.ExcludedAssemblies);
                }
            }

            // No options found, use defaults
            return DefaultOptions;
        }

        private static bool? TryGetBooleanPropertyValue(INamedTypeSymbol classSymbol, string propertyName)
        {
            // Try to find the property
            var property = classSymbol.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
            if (property == null) 
                return null;

            // Check if it has an initializer with a constant value
            foreach (var syntaxRef in property.DeclaringSyntaxReferences)
            {
                if (syntaxRef.GetSyntax() is PropertyDeclarationSyntax propertySyntax && 
                    propertySyntax.Initializer?.Value != null)
                {
                    // This is a very simplified approach that only works for literal true/false values
                    // In a real implementation, you might need to analyze the syntax more carefully
                    string initializer = propertySyntax.Initializer.Value.ToString();
                    if (initializer == "true") 
                        return true;
                    if (initializer == "false") 
                        return false;
                }
            }

            return null;
        }

        private static bool IsSystemAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("System.") || 
                   assemblyName.StartsWith("Microsoft.") ||
                   assemblyName == "System" || 
                   assemblyName == "mscorlib" || 
                   assemblyName == "netstandard";
        }

        private static void ProcessTypeSymbolForInterface(
            INamedTypeSymbol classSymbol,
            string interfaceToFind, 
            bool isOpenGeneric,
            List<(string ClassName, string Namespace, string InterfaceType, int TypeParameterCount, List<string> TypeParameterConstraints)> results)
        {
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
                    {
                        // For open generics, we just use the original interface definition
                        interfaceType = interfaceName;
                    }
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

        private static IEnumerable<INamedTypeSymbol> GetAllPublicClassesFromAssembly(IAssemblySymbol assembly)
        {
            HashSet<INamedTypeSymbol> processedTypes = new HashSet<INamedTypeSymbol>();
            Queue<INamespaceSymbol> namespacesToProcess = new Queue<INamespaceSymbol>();
            
            // Start with the global namespace
            namespacesToProcess.Enqueue(assembly.GlobalNamespace);
            
            while (namespacesToProcess.Count > 0)
            {
                INamespaceSymbol current = namespacesToProcess.Dequeue();
                
                // Process all public classes in this namespace
                foreach (INamedTypeSymbol typeSymbol in current.GetTypeMembers())
                {
                    if (typeSymbol.TypeKind == TypeKind.Class && typeSymbol.DeclaredAccessibility == Accessibility.Public)
                    {
                        processedTypes.Add(typeSymbol);
                        
                        // Also get nested public classes
                        foreach (var nestedType in typeSymbol.GetTypeMembers())
                        {
                            if (nestedType.TypeKind == TypeKind.Class && nestedType.DeclaredAccessibility == Accessibility.Public)
                            {
                                processedTypes.Add(nestedType);
                            }
                        }
                    }
                }
                
                // Add child namespaces to the queue
                foreach (INamespaceSymbol childNamespace in current.GetNamespaceMembers())
                {
                    namespacesToProcess.Enqueue(childNamespace);
                }
            }
            
            return processedTypes;
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

            // If no attribute, try to find the default from options
            // As a fallback, default to Transient
            return "AddTransient";
        }

        // This method was replaced with a simpler version since we can't easily determine options at compile time
        private static string GetDefaultServiceLifetime(SyntaxTree syntaxTree)
        {
            return "AddTransient";
        }

        protected static bool IsFromCrispNamespace(INamedTypeSymbol symbol) => 
            symbol.ContainingNamespace?.ToDisplayString().StartsWith("CRISP") == true;

        public abstract bool Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> classes,
            SourceProductionContext context);
    }
}