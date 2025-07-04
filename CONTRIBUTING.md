# Contributing to CRISP Framework

Thank you for your interest in contributing to the CRISP (Command Response Interface Service Pattern) framework! We welcome contributions from the community and are grateful for your support.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Documentation](#documentation)
- [Issue Reporting](#issue-reporting)
- [Security Vulnerabilities](#security-vulnerabilities)

## Code of Conduct

This project adheres to a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension
- Git

### Development Setup

1. **Fork the Repository**
   ```bash
   # Fork the repo on GitHub, then clone your fork
   git clone https://github.com/yourusername/crisp.git
   cd crisp
   ```

2. **Build the Solution**
   ```bash
   dotnet build
   ```

3. **Run Tests**
   ```bash
   dotnet test
   ```

4. **Run Sample Application**
   ```bash
   cd samples/TodoApi
   dotnet run
   ```

## Contributing Guidelines

### Types of Contributions

We welcome several types of contributions:

- **Bug fixes** - Fix issues in existing functionality
- **Features** - Add new functionality to the framework
- **Documentation** - Improve or add documentation
- **Examples** - Create sample applications or code examples
- **Performance** - Optimize existing code for better performance
- **Tests** - Add or improve test coverage

### Before You Start

1. **Check existing issues** - Look for existing issues or discussions about your proposed change
2. **Create an issue** - For significant changes, create an issue first to discuss the approach
3. **Start small** - Begin with small contributions to familiarize yourself with the codebase

## Pull Request Process

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/issue-number-description
```

### 2. Make Your Changes

- Follow the [coding standards](#coding-standards)
- Add tests for new functionality
- Update documentation as needed
- Ensure all tests pass

### 3. Commit Your Changes

Use clear, descriptive commit messages:

```bash
git commit -m "Add rate limiting behavior for CRISP pipeline

- Implement TokenBucket, SlidingWindow, and FixedWindow algorithms
- Add configurable client identification strategies
- Include comprehensive unit tests
- Update documentation with usage examples"
```

### 4. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub with:

- **Clear title** describing the change
- **Detailed description** explaining what and why
- **Link to related issues** if applicable
- **Screenshots** for UI changes
- **Breaking changes** noted clearly

### 5. Code Review Process

- Maintainers will review your PR
- Address feedback promptly
- Keep your branch up to date with main
- Once approved, maintainers will merge

## Coding Standards

### C# Conventions

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Write XML documentation for public APIs
- Use nullable reference types appropriately

### Code Style

```csharp
// Good: Clear, descriptive naming
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IValidator<TRequest> _validator;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> HandleAsync(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Implementation...
    }
}
```

### File Organization

- One class per file
- Organize using statements
- Use appropriate namespaces
- Place files in logical folder structures

### Documentation

- XML documentation for all public members
- Clear README files for each package
- Code comments for complex logic
- Examples in documentation

## Testing Requirements

### Unit Tests

- **Coverage**: Aim for 90%+ test coverage
- **Naming**: Use descriptive test method names
- **Structure**: Arrange-Act-Assert pattern
- **Isolation**: Tests should not depend on each other

```csharp
[Fact]
public async Task HandleAsync_WithValidRequest_ShouldReturnSuccess()
{
    // Arrange
    var request = new TestCommand("valid-input");
    var expectedResult = Result<string>.Success("test-result");
    
    // Act
    var result = await _handler.HandleAsync(request, CancellationToken.None);
    
    // Assert
    result.Should().BeEquivalentTo(expectedResult);
}
```

### Integration Tests

- Test complete workflows
- Use TestServer for ASP.NET Core testing
- Test error scenarios
- Verify security behaviors

### Performance Tests

- Benchmark critical paths
- Test under load
- Measure memory usage
- Profile startup time

## Documentation

### Code Documentation

- XML documentation for all public APIs
- Include usage examples
- Document breaking changes
- Explain complex algorithms

### User Documentation

- Update relevant docs/ files
- Add examples to samples/
- Update README files
- Create migration guides for breaking changes

### API Documentation

- Document new endpoints
- Include request/response examples
- Note authentication requirements
- Specify rate limits

## Issue Reporting

### Bug Reports

Use the bug report template and include:

- **Description** of the issue
- **Steps to reproduce**
- **Expected behavior**
- **Actual behavior**
- **Environment details** (.NET version, OS, etc.)
- **Code samples** if applicable

### Feature Requests

Use the feature request template and include:

- **Problem description**
- **Proposed solution**
- **Alternative solutions considered**
- **Additional context**

### Questions

- Check existing documentation first
- Search existing issues
- Use GitHub Discussions for general questions
- Be specific about your use case

## Security Vulnerabilities

**Do not create public issues for security vulnerabilities.**

Instead:

1. Email security issues to: [security@crispframework.org](mailto:security@crispframework.org)
2. Include detailed description
3. Provide steps to reproduce
4. Allow time for fix before disclosure

## Development Guidelines

### Architecture Principles

- **CQRS separation** - Maintain clear command/query separation
- **Pipeline pattern** - Use behaviors for cross-cutting concerns
- **Performance first** - Pre-compile pipelines, avoid reflection
- **Framework agnostic** - Core should not depend on ASP.NET Core

### Naming Conventions

- **Commands** - End with "Command" (e.g., `CreateTodoCommand`)
- **Queries** - End with "Query" (e.g., `GetTodoQuery`)
- **Handlers** - End with "Handler" (e.g., `CreateTodoCommandHandler`)
- **Behaviors** - End with "Behavior" (e.g., `ValidationBehavior`)

### Error Handling

- Use Result<T> pattern for operation results
- Throw exceptions only for exceptional cases
- Provide meaningful error messages
- Include correlation IDs for tracking

### Dependencies

- Minimize external dependencies
- Use abstractions for external services
- Prefer Microsoft.Extensions.* packages
- Avoid packages with licensing restrictions

## Release Process

### Version Numbering

We follow [Semantic Versioning](https://semver.org/):

- **Major** (X.0.0) - Breaking changes
- **Minor** (X.Y.0) - New features, backward compatible
- **Patch** (X.Y.Z) - Bug fixes, backward compatible

### Release Notes

Include in release notes:

- New features
- Bug fixes
- Breaking changes
- Performance improvements
- Migration instructions

## Getting Help

- **Documentation** - Check docs/ folder
- **Examples** - See samples/ folder
- **Discussions** - Use GitHub Discussions
- **Issues** - Create GitHub issue for bugs
- **Stack Overflow** - Tag with `crisp-framework`

## Recognition

Contributors will be recognized in:

- CONTRIBUTORS.md file
- Release notes
- Project documentation
- Conference presentations

## License

By contributing to CRISP Framework, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to CRISP Framework! Your efforts help make this project better for everyone.