# BytLabs.Multitenancy

Tenant resolution primitives. Resolves a `TenantId` from the current request through a chain of
pluggable resolvers; the data-access layer uses it to select a per-tenant database (database-per-tenant
isolation).

## Install

```xml
<PackageReference Include="BytLabs.Multitenancy" />
```

## What's inside

| Type | Purpose |
|------|---------|
| `TenantId` | Immutable tenant identifier (`string Value`) |
| `ITenantIdProvider` | `TenantId GetTenantId()` — the resolved current tenant |
| `ITenantIdResolver` | `bool TryGetCurrent(out TenantId?)` — one resolution strategy |
| `MultitenancyBuilder` | Fluent registration of resolvers (`AddResolver<T>()`, instance, or factory) |
| `ValueTenantIdResolver` | Returns a fixed tenant (testing / fallback) |
| `AddMultitenancy()` | Registers the provider and returns the builder |
| `FailedToResolveTenantIdException` | Thrown when no resolver can identify the tenant |

## How it works

`TenantIdProvider` tries each registered `ITenantIdResolver` in order and returns the first
`TenantId` found; if none succeed it throws `FailedToResolveTenantIdException`. Resolver exceptions
are logged and skipped.

## Usage

Registered for you by [BytLabs.Api](BytLabs.Api.md)'s `WithMultiTenantContext`:

```csharp
.WithMultiTenantContext(mt => mt.AddResolver<FromHeaderTenantIdResolver>())
```

Or directly on a service collection:

```csharp
services.AddMultitenancy()
    .AddResolver<FromHeaderTenantIdResolver>()              // from BytLabs.Api: reads a request header
    .AddResolver(sp => new ValueTenantIdResolver(new TenantId("bytlabs"))); // fallback
```

Consume the resolved tenant:

```csharp
public class Foo(ITenantIdProvider tenants)
{
    public void Bar() { TenantId tenant = tenants.GetTenantId(); /* ... */ }
}
```

### Custom resolver

```csharp
public class FromClaimTenantIdResolver(IHttpContextAccessor http) : ITenantIdResolver
{
    public bool TryGetCurrent([NotNullWhen(true)] out TenantId? tenantId)
    {
        var value = http.HttpContext?.User.FindFirst("tenant")?.Value;
        tenantId = string.IsNullOrEmpty(value) ? null : new TenantId(value);
        return tenantId is not null;
    }
}
```

### Where the tenant is used

[BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) resolves `IMongoDatabase` per request as
`"{DatabaseName}-{tenantId}"`, so tenant isolation is **physical and transparent** — no tenant field
on entities, no per-query filter. Tests/clients select the tenant by sending the resolver's header
(e.g. `Tenant: Test`).

## Related packages
- [BytLabs.Api](BytLabs.Api.md) — wires this via `WithMultiTenantContext` and ships `FromHeaderTenantIdResolver`.
- [BytLabs.DataAccess.MongoDB](BytLabs.DataAccess.MongoDB.md) — consumes `ITenantIdProvider` for database selection.
