# BytLabs.Infrastructure

A minimal/placeholder package. It currently provides only a shared exception type for the
infrastructure layer; it has no registration or runtime behavior yet.

## Install

```xml
<PackageReference Include="BytLabs.Infrastructure" />
```

## What's inside

| Type | Purpose |
|------|---------|
| `Exceptions.InfrastructureException` | Exception for failures in the infrastructure layer |

## Usage

```csharp
throw new InfrastructureException("Failed to reach the external payment gateway.");
```

> Status: reserved for future cross-cutting infrastructure helpers. Most services do not need to
> reference it directly today.

## Related packages
- [BytLabs.DataAccess](BytLabs.DataAccess.md), [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — the concrete infrastructure implementations services actually use.
