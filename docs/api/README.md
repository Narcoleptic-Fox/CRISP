# CRISP Framework API Reference

This directory contains comprehensive API documentation for all CRISP framework components.

## 📚 API Documentation Structure

### Core Framework
- **[Crisp.Core](crisp-core.md)** - Core abstractions and interfaces
  - [Commands & Queries](crisp-core.md#commands--queries)
  - [Pipeline System](crisp-core.md#pipeline-system)
  - [Result Types](crisp-core.md#result-types)
  - [Validation](crisp-core.md#validation)
  - [Events](crisp-core.md#events)
  - [Exceptions](crisp-core.md#exceptions)

### Runtime & Performance
- **[Crisp.Runtime](crisp-runtime.md)** - High-performance dispatchers and pipeline execution
  - [Pre-compiled Dispatchers](crisp-runtime.md#dispatchers)
  - [Pipeline Behaviors](crisp-runtime.md#pipeline-behaviors)
  - [Event Publishing](crisp-runtime.md#event-publishing)

### Web Integration
- **[Crisp.AspNetCore](crisp-aspnetcore.md)** - ASP.NET Core integration
  - [Endpoint Mapping](crisp-aspnetcore.md#endpoints)
  - [Middleware](crisp-aspnetcore.md#middleware)
  - [Health Checks](crisp-aspnetcore.md#health-checks)
  - [Security](crisp-aspnetcore.md#security)
  - [Extensions](crisp-aspnetcore.md#extensions)

### Blazor Components
- **[Crisp.Blazor](crisp-blazor.md)** - Blazor integration and state management
  - [State Management](crisp-blazor.md#state-management)
  - [Blazor Dispatchers](crisp-blazor.md#dispatchers)
  - [Notifications](crisp-blazor.md#notifications)
  - [Configuration](crisp-blazor.md#configuration)

## 🚀 Quick Navigation

### Common Tasks
- **Creating Commands:** [ICommand\<T\>](crisp-core.md#icommand)
- **Creating Queries:** [IQuery\<T\>](crisp-core.md#iquery)
- **Adding Validation:** [Validation System](crisp-core.md#validation)
- **Error Handling:** [Exception Types](crisp-core.md#exceptions)
- **Pipeline Behaviors:** [IPipelineBehavior](crisp-core.md#ipipelinebehavior)

### Framework Setup
- **ASP.NET Core:** [Service Registration](crisp-aspnetcore.md#service-registration)
- **Blazor:** [Blazor Configuration](crisp-blazor.md#configuration)
- **Health Checks:** [Health Monitoring](crisp-aspnetcore.md#health-checks)

### Advanced Features
- **Caching:** [CachingBehavior](crisp-runtime.md#cachingbehavior)
- **Resilience:** [RetryBehavior & CircuitBreaker](crisp-runtime.md#resilience)
- **Security:** [Authorization & Rate Limiting](crisp-aspnetcore.md#security)

## 📖 Usage Examples

Each API reference page includes practical examples showing how to use the components effectively in real applications.

## 🔗 Related Documentation

- [Getting Started Guide](../getting-started.md)
- [Core Concepts](../concepts/)
- [Examples](../examples/)
- [Migration Guide](../migration-guide.md)