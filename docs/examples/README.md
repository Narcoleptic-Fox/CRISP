# CRISP Architecture Examples

This directory contains practical examples demonstrating CRISP architecture patterns and implementations.

## Examples Overview

### Basic Examples
- **[Simple CRUD Feature](simple-crud/)** - Basic Create, Read, Update, Delete operations
- **[Validation Patterns](validation-patterns/)** - Core and Application validation examples
- **[Manual Mapping](manual-mapping/)** - Contract ↔ Entity transformation patterns

### Advanced Examples
- **[Complex Queries](complex-queries/)** - Advanced filtering, sorting, and pagination
- **[Business Logic](business-logic/)** - Domain rules and business process implementation
- **[Event Handling](event-handling/)** - Domain events and event-driven patterns
- **[Error Handling](error-handling/)** - Comprehensive error handling strategies

### Integration Examples
- **[API Integration](api-integration/)** - External service integration patterns
- **[File Upload](file-upload/)** - File handling and storage
- **[Background Tasks](background-tasks/)** - Async processing and job queues
- **[Authentication](authentication/)** - Identity and authorization patterns

### Testing Examples
- **[Unit Testing](unit-testing/)** - Testing Core domain logic
- **[Integration Testing](integration-testing/)** - End-to-end feature testing
- **[Performance Testing](performance-testing/)** - Load testing and benchmarks

### UI Examples
- **[Blazor Components](blazor-components/)** - Reusable component patterns
- **[Forms and Validation](forms-validation/)** - Client-side form handling
- **[Data Tables](data-tables/)** - Advanced table components with filtering

## How to Use These Examples

1. **Start with Simple CRUD** to understand the basic patterns
2. **Review Validation Patterns** to see how business rules are implemented
3. **Explore Advanced Examples** for complex scenarios
4. **Check Testing Examples** for quality assurance patterns
5. **Reference Integration Examples** for external service patterns

## Example Structure

Each example follows this structure:
```
example-name/
├── README.md           # Example overview and explanation
├── Core/               # Domain contracts and validation
├── Infrastructure/     # Entity mappings and service implementations
├── Application/        # Endpoints, components, and orchestration
└── Tests/              # Unit and integration tests
```

## Contributing Examples

When adding new examples:

1. **Follow the established structure** above
2. **Include comprehensive README** explaining the pattern
3. **Add both positive and negative test cases**
4. **Document any architectural decisions**
5. **Keep examples focused** on specific patterns
6. **Include performance considerations** where relevant

## Quick Reference

### Most Common Patterns
- [Basic Service Implementation](simple-crud/README.md#service-implementation)
- [Manual Mapping Techniques](manual-mapping/README.md#mapping-patterns)
- [Validation Strategies](validation-patterns/README.md#validation-approaches)
- [Error Handling](error-handling/README.md#error-patterns)

### Performance Patterns
- [Optimized Queries](complex-queries/README.md#performance-optimization)
- [Caching Strategies](caching/README.md#cache-patterns)
- [Background Processing](background-tasks/README.md#async-patterns)

### Testing Patterns
- [Service Testing](unit-testing/README.md#service-tests)
- [Integration Testing](integration-testing/README.md#feature-tests)
- [Mock Strategies](testing-patterns/README.md#mocking-approaches)
