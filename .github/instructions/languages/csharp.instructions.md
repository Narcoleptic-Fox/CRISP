---
applyTo: "**/*.cs"
---
# C# Programming Guidelines

## Naming Conventions

- **Classes, Methods, Properties**: Use PascalCase (e.g., `ClassName`, `MethodName`, `PropertyName`).
- **Interfaces**: Prefix with 'I' and use PascalCase (e.g., `IDisposable`).
- **Private/Protected Fields**: Use camelCase with underscore prefix (e.g., `_privateField`).
- **Parameters and Local Variables**: Use camelCase (e.g., `parameterName`, `localVariable`).
- **Constants**: Use PascalCase or ALL_CAPS depending on team preference.

## Code Style

- Use expression-bodied members for concise one-liners.
- Use `var` when the type is obvious from the right-hand side.
- Prefer string interpolation (`$"Hello {name}"`) over string concatenation.
- Use `nameof()` operator when referencing names of program elements.
- Use pattern matching where it improves readability.

## Language Features

- Use nullable reference types for better null safety (`string?`).
- Take advantage of init-only properties for immutability.
- Use record types for data-centric objects with value semantics.
- Leverage LINQ for collection operations but avoid excessive chaining.
- Use async/await for asynchronous operations.

## Common Practices

- Implement `IDisposable` for classes that manage resources.
- Use attributes for declarative metadata (`[Authorize]`, `[Required]`, etc.).
- Consider using extension methods for adding functionality to existing types.
- Utilize interfaces for dependency injection.
- Use events for loose coupling between components.

## Performance Tips

- Use `StringBuilder` for concatenating many strings.
- Consider using `Span<T>` and `Memory<T>` for high-performance scenarios.
- Prefer `struct` for small, immutable value types.
- Use `List<T>` over arrays when size needs to change.
- Consider using value tuples for simple data grouping.

## Framework-Specific Guidelines

### ASP.NET Core
- Use Dependency Injection for services.
- Prefer minimal APIs for simple endpoints.
- Use MVC pattern for complex web applications.
- Leverage middleware for cross-cutting concerns.

### Entity Framework Core
- Use migrations for database schema changes.
- Consider query optimization for large datasets.
- Use eager loading with `Include()` where appropriate.

## Testing

- Use xUnit, NUnit, or MSTest for unit testing.
- Mock dependencies using Moq or NSubstitute.
- Use Shouldly for more readable assertions.

## Documentation

- Use XML comments (`///`) for public APIs.
- Consider generating API documentation with DocFX or similar tools.

## Resources

- [C# Coding Conventions (Microsoft)](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Core Coding Guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- [C# Reference Documentation](https://docs.microsoft.com/en-us/dotnet/csharp/)
