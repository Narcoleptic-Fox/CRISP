# CRISP Architecture Documentation

Welcome to the CRISP Architecture documentation. CRISP (Command Response Interface Service Pattern) is a modern architectural pattern that implements CQRS through direct service contracts, organized in a three-layer architecture.

## 📚 Documentation Index

- **[Architecture Overview](architecture-overview.md)** - High-level introduction to CRISP principles
- **[Core Layer](core-layer.md)** - Immutable domain contracts and validation
- **[Infrastructure Layer](infrastructure-layer.md)** - Mutable entities and service implementations  
- **[Application Layer](application-layer.md)** - Feature orchestration and manual mapping
- **[CQRS with Service Contracts](cqrs-service-contracts.md)** - Detailed CQRS implementation
- **[Feature Development Guide](feature-development-guide.md)** - How to build features in CRISP
- **[Migration Guide](migration-guide.md)** - Migrating from other patterns to CRISP
- **[Examples](examples/)** - Practical code examples and patterns
- **[ADRs](adrs/)** - Architectural Decision Records

## 🚀 Quick Start

1. **Understand the Pattern**: Start with [Architecture Overview](architecture-overview.md)
2. **See It in Action**: Check out [Examples](examples/)
3. **Build Your First Feature**: Follow the [Feature Development Guide](feature-development-guide.md)

## 🎯 Key Benefits

- 🚀 **Better Performance** - No reflection overhead from mediator patterns
- 🔍 **Better IDE Support** - Full IntelliSense and navigation
- 🧪 **Simpler Testing** - Direct service mocking
- 📝 **Explicit Dependencies** - Clear service contracts
- ⚡ **Compile-time Safety** - Type-safe operations
- 🏗️ **Feature-Focused** - Vertical slice organization

## 🤝 Contributing

When adding new features or modifying the architecture, please:
1. Update relevant documentation
2. Add examples for new patterns
3. Create ADRs for significant decisions
4. Follow the CRISP principles outlined in this documentation
