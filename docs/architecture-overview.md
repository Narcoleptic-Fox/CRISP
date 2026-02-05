# CRISP Architecture Overview

## What is CRISP?

**CRISP** (Command Response Interface Service Pattern) is an architectural pattern that combines the benefits of CQRS (Command Query Responsibility Segregation) with direct service contracts, eliminating the complexity and performance overhead of traditional mediator patterns.

## Core Principles

### 1. **Command Response Segregation**
- **Commands**: Write operations that modify state
- **Queries**: Read operations that retrieve data
- Clear separation between read and write operations

### 2. **Interface-Driven Design**
- Service contracts define operation boundaries
- Explicit interfaces for every operation type
- Compile-time verification of contracts

### 3. **Service-Based Implementation**
- Direct service injection (no mediator)
- Better performance and IDE support
- Simpler testing and debugging

### 4. **Pattern Consistency**
- Standardized approach to all operations
- Predictable structure across features
- Easy to learn and maintain

## Three-Layer Architecture

CRISP organizes code into three distinct layers:

```
┌─────────────────────────┐
│    Application Layer    │  ← Feature orchestration, mapping
│   (Server/Client)       │
├─────────────────────────┤
│  Infrastructure Layer   │  ← Mutable entities, service implementations
│   (ServiceDefaults)     │
├─────────────────────────┤
│      Core Layer         │  ← Immutable contracts, validation, events
│       (Core)            │
└─────────────────────────┘
```

### Core Layer (Foundation)
- **Purpose**: Immutable domain contracts and business rules
- **Contents**: 
  - Domain models (records)
  - Commands and queries (contracts)
  - Domain events
  - Core validation logic
- **Dependencies**: None (pure domain)

### Infrastructure Layer (Persistence)
- **Purpose**: Data persistence and external service integration
- **Contents**:
  - Mutable Entity Framework entities
  - Database configurations
  - Service implementations
  - External service clients
- **Dependencies**: Core

### Application Layer (Orchestration)
- **Purpose**: Feature implementation and user interaction
- **Contents**:
  - Feature endpoints and handlers
  - Manual mapping between layers
  - Application-specific validation
  - UI components and services
- **Dependencies**: Core, Infrastructure

## Key Differentiators

### vs. Traditional CQRS (MediatR)

**Traditional CQRS:**
```csharp
// Handler-based with mediator
public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public Task<User> Handle(GetUserQuery request, CancellationToken ct) { }
}

// Usage via mediator (reflection-based)
var user = await _mediator.Send(new GetUserQuery(id));
```

**CRISP:**
```csharp
// Service contract-based
public class GetUserService : IQueryService<GetUserQuery, User>
{
    public ValueTask<User> Send(GetUserQuery query, CancellationToken ct) { }
}

// Direct service injection
var user = await _getUserService.Send(new GetUserQuery(id));
```

### vs. Traditional Clean Architecture

**Clean Architecture:**
```
Controllers → Use Cases → Entities
    ↓           ↓           ↓
  Web       Application   Domain
```

**CRISP:**
```
Features → Service Contracts → Domain Contracts
   ↓              ↓               ↓
Application → Infrastructure → Core
```

## Benefits of CRISP

### Performance Benefits
- **No Reflection**: Direct service calls instead of mediator dispatch
- **ValueTask**: Optimized async operations
- **Minimal Overhead**: Clean service contracts

### Developer Experience
- **Better IntelliSense**: Full IDE support for navigation
- **Explicit Dependencies**: Clear service requirements in constructors
- **Compile-time Safety**: Type checking for all operations
- **Simplified Testing**: Direct service mocking

### Maintainability
- **Feature-Focused**: Vertical slice organization
- **Consistent Patterns**: Standardized operation structures
- **Clear Boundaries**: Well-defined layer responsibilities
- **Manual Mapping**: Explicit, testable transformations

## When to Use CRISP

### ✅ Ideal For:
- Medium to large applications
- Teams wanting CQRS benefits without mediator complexity
- Applications requiring high performance
- Projects needing clear architectural boundaries
- Domain-driven development

### ❌ Consider Alternatives For:
- Very simple CRUD applications
- Prototype/proof-of-concept projects
- Teams unfamiliar with CQRS concepts
- Applications with minimal business logic

## Getting Started

1. **Read the Layer Documentation**:
   - [Core Layer](core-layer.md)
   - [Infrastructure Layer](infrastructure-layer.md)
   - [Application Layer](application-layer.md)

2. **Understand Service Contracts**:
   - [CQRS with Service Contracts](cqrs-service-contracts.md)

3. **Build Your First Feature**:
   - [Feature Development Guide](feature-development-guide.md)

4. **Explore Examples**:
   - [Examples Directory](examples/)

## Next Steps

Continue reading with the [Core Layer](core-layer.md) documentation to understand how domain contracts work in CRISP.
