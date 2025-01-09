---
_layout: landing
---

# BytLabs Backend Packages

Welcome to BytLabs Backend Packages documentation. This collection of .NET libraries helps you build scalable, maintainable, and robust backend applications.

## Quick Navigation

- [Getting Started](docs/getting-started.md)
- [API Documentation](api/BytLabs.Api.yml)
- [Introduction](docs/introduction.md)

## Key Features

### 🏢 Multi-tenancy Support
Built-in support for multi-tenant applications with flexible tenant resolution strategies.

### 📦 Domain-Driven Design
Tools and patterns for implementing DDD, including aggregates, entities, and domain events.

### 🗄️ Data Access
MongoDB integration with transaction support and repository pattern implementation.

### ⚡ CQRS Pattern
Command Query Responsibility Segregation with MediatR integration.

### 📊 Observability
Integrated logging, metrics, and tracing capabilities using OpenTelemetry.

## Getting Started

```csharp
// Add services to your application
services.AddMultitenancy()
.AddMongoDatabase()
.AddCQRS()
.AddObservability();
```


## Package Overview

| Package | Description |
|---------|------------|
| `BytLabs.Application` | CQRS, validation, and application services |
| `BytLabs.Domain` | Domain model building blocks |
| `BytLabs.DataAccess` | Data persistence and MongoDB integration |
| `BytLabs.Multitenancy` | Multi-tenant infrastructure |
| `BytLabs.Observability` | Monitoring and logging tools |

## Support

- 📚 [Documentation](docs/getting-started.md)
- 💬 [Discussions](https://github.com/bytlabs/BytLabs.BackendPackages/discussions)
- 🐛 [Issue Tracker](https://github.com/bytlabs/BytLabs.BackendPackages/issues)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.