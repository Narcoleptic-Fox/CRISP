# ADR-001: CRISP Architecture Pattern

**Status:** Accepted  
**Date:** 2025-08-13  
**Authors:** CRISP Team

## Context

Modern web applications require clear architectural patterns that provide good separation of concerns, maintainability, and performance. Traditional approaches like Clean Architecture with MediatR have some drawbacks:

### Problems with Traditional CQRS + MediatR

1. **Performance Overhead**: Reflection-based handler dispatch adds latency
2. **Hidden Dependencies**: Mediator obscures actual service dependencies
3. **Complex Debugging**: Reflection makes stack traces harder to follow
4. **Testing Complexity**: Mocking mediator instead of actual services
5. **IDE Limitations**: Poor IntelliSense and navigation support

### Domain Confusion

Many developers struggle with where to place entities in Domain-Driven Design:
- Should entities be mutable or immutable?
- Where do EF entities belong in Clean Architecture?
- How to handle the impedance mismatch between domain and persistence?

## Decision

We adopt **CRISP** (Command Response Interface Service Pattern) with a three-layer architecture:

### 1. Core Layer - Immutable Domain Contracts
- **Purpose**: Pure domain logic and contracts
- **Types**: Immutable records for all domain models
- **Validation**: Business rules implemented in domain models
- **Dependencies**: None (foundation layer)

### 2. Infrastructure Layer - Mutable Persistence Entities
- **Purpose**: Data persistence and external service integration
- **Types**: Mutable classes optimized for Entity Framework
- **Services**: Direct service implementations of Core contracts
- **Dependencies**: Core only

### 3. Application Layer - Feature Orchestration
- **Purpose**: User interaction and feature coordination
- **Implementation**: Endpoints, components, and manual mapping
- **Dependencies**: Core and Infrastructure

### CQRS Implementation
- **No Mediator**: Direct service injection for performance
- **Service Contracts**: Explicit interfaces for all operations
- **ValueTask**: Performance-optimized async operations
- **Compile-time Safety**: Type-safe service contracts

## Rationale

### Performance Benefits
- **Direct Service Calls**: Eliminates reflection overhead
- **ValueTask Usage**: Optimal async performance
- **Minimal Abstractions**: Clean service boundaries without excess layers

### Developer Experience
- **Explicit Dependencies**: Clear service requirements in constructors
- **Better IDE Support**: Full IntelliSense and navigation
- **Simplified Testing**: Direct service mocking
- **Clear Architecture**: Obvious where code belongs

### Maintainability
- **Manual Mapping**: Explicit, testable transformations
- **Feature Organization**: Vertical slices for business capabilities
- **Consistent Patterns**: Standardized operation structures
- **Clear Boundaries**: Well-defined layer responsibilities

## Consequences

### Positive
- ✅ Better performance than mediator-based CQRS
- ✅ Clearer architectural boundaries and dependencies
- ✅ Improved developer productivity and debugging
- ✅ Easier testing with direct service dependencies
- ✅ Better IDE support and code navigation
- ✅ Explicit control over all mappings and transformations

### Negative
- ❌ More manual work (no automatic mapping)
- ❌ Larger number of service interfaces to manage
- ❌ Learning curve for teams familiar with MediatR
- ❌ Need to maintain consistency across feature implementations

### Neutral
- 🔄 Different approach from mainstream Clean Architecture
- 🔄 Requires discipline to maintain architectural boundaries
- 🔄 Manual service registration (can be automated with source generators)

## Implementation Guidelines

### Core Layer Rules
1. Use immutable records for all domain models
2. Implement validation within domain models
3. No dependencies on other layers
4. Define service contracts but not implementations

### Infrastructure Layer Rules
1. Use mutable classes for EF entities
2. Implement Core service contracts
3. Handle database concerns and external services
4. Manual mapping between contracts and entities

### Application Layer Rules
1. Organize code by features (vertical slices)
2. Handle user interaction and orchestration
3. Map between external DTOs and Core contracts
4. Implement application-specific validation

### Service Contract Rules
1. Use explicit service interfaces for all operations
2. Prefer ValueTask for async operations
3. Include cancellation token support
4. Handle errors consistently across services

## Monitoring

We will monitor the effectiveness of this decision by tracking:

- **Development Velocity**: Time to implement new features
- **Performance Metrics**: API response times and throughput
- **Code Quality**: Cyclomatic complexity and test coverage
- **Developer Satisfaction**: Team feedback on the architecture
- **Maintenance Burden**: Time spent on debugging and refactoring

## Future Considerations

- **Source Generators**: Automate service registration and mapping
- **Analyzers**: Enforce architectural rules at compile time
- **Templates**: Provide scaffolding for new features
- **Documentation**: Maintain comprehensive examples and guides

## Related ADRs

- [ADR-002: Manual Mapping Strategy](adr-002-manual-mapping.md)
- [ADR-003: Service Registration Patterns](adr-003-service-registration.md)
- [ADR-004: Error Handling Strategy](adr-004-error-handling.md)
