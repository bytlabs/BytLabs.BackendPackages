---
name: bytlabs-cross-cutting
description: Use when working on tenancy or observability in a BytLabs service - tenant resolvers, TenantId, database-per-tenant, Serilog and OpenTelemetry logging, metrics and tracing, and liveness/readiness/startup health checks.
---

# Tenancy and observability

Both are wired by the `ApiServiceBuilder` chain — see `bytlabs-service-setup` for the enforced
order. This skill covers what those steps configure and how to extend them.

## Multitenancy (`BytLabs.Multitenancy`)

| Type | Purpose |
|------|---------|
| `TenantId` | Immutable tenant identifier (`string Value`) |
| `ITenantIdProvider` | `TenantId GetTenantId()` — the resolved current tenant |
| `ITenantIdResolver` | `bool TryGetCurrent(out TenantId?)` — one resolution strategy |
| `MultitenancyBuilder` | Fluent registration: `AddResolver<T>()`, instance, or factory |
| `ValueTenantIdResolver` | Returns a fixed tenant (testing / single-tenant fallback) |
| `AddMultitenancy()` | Registers the provider and returns the builder |
| `FailedToResolveTenantIdException` | Thrown when no resolver can identify the tenant |

### How resolution works

Resolvers are tried **in registration order**; the first returning `true` wins. If none succeeds,
`FailedToResolveTenantIdException` is thrown at request time.

```csharp
.WithMultiTenantContext(mt => mt.AddResolver<FromHeaderTenantIdResolver>())
```

`FromHeaderTenantIdResolver` lives in `BytLabs.Api` and reads the tenant header.
`ValueTenantIdResolver` pins a fixed tenant — use it for tests, or for a single-tenant service
that is not ready to adopt tenancy.

A custom resolver implements `ITenantIdResolver`:

```csharp
public class FromClaimTenantIdResolver : ITenantIdResolver
{
    private readonly IHttpContextAccessor _accessor;
    public FromClaimTenantIdResolver(IHttpContextAccessor accessor) => _accessor = accessor;

    public bool TryGetCurrent(out TenantId? tenantId)
    {
        var claim = _accessor.HttpContext?.User.FindFirst("tenant_id")?.Value;
        tenantId = string.IsNullOrEmpty(claim) ? null : new TenantId(claim);
        return tenantId is not null;
    }
}
```

Register it with `mt.AddResolver<FromClaimTenantIdResolver>()`. Order matters — put the most
specific strategy first and any fallback last.

### Tenancy is ambient

The data layer reads `ITenantIdProvider` and selects the database (Mongo derives
`"{DatabaseName}-{tenantId}"`; EF looks up the tenant's connection string). **Never thread a
tenant id through a command, a query, or a repository call.** See `bytlabs-data-access`.

## Observability (`BytLabs.Observability`)

| Member | Purpose |
|---|---|
| `ObservabilityConfiguration` | `ServiceName`, `CollectorUrl`, `Timeout`, `Logs.OpenTelemetryEnabled` |
| `AddLogging(config, configure?)` | Serilog: console + enrichers (environment, span, app name); optional OTel log export |
| `AddMetrics(config, configure?)` | OpenTelemetry metrics → OTLP collector |
| `AddTracing(config, configure?)` | OpenTelemetry tracing → OTLP collector |
| `AddHealthChecks(...)` / `UseHealthChecks()` | Health-check registration and endpoint mapping |
| `AddCheckForReady<TCheck>()` / `AddCheckForStartup<TCheck>()` / `AddCheckForAppHealth<TCheck>()` | Add a custom check to a specific probe |
| `UseTraceIdResponseHeader()` / `UseLoggerWithTenantId()` | Middleware registration |
| `TraceIdResponseHeaderMiddleware`, `TenantIdEnricherMiddleware` | The middleware themselves |
| `DefaultApplicationHealthCheck`, `DefaultApplicationReadyCheck`, `DefaultApplicationStartupCheck` | Defaults |

Configuration binds from the `"ObservabilityConfiguration"` section:

```json
{
  "ObservabilityConfiguration": {
    "ServiceName": "my-service",
    "CollectorUrl": "http://localhost:4317"
  }
}
```

### Wiring

These are the `WithLogging()`, `WithMetrics()`, `WithTracing()` and `WithHealthChecks()` steps of
the builder chain. Each takes an optional delegate for service-specific additions:

```csharp
.WithLogging(cfg => cfg.Enrich.WithProperty("component", "orders"))
.WithMetrics(m => m.AddMeter("Orders"))
.WithTracing(t => t.AddSource("Orders"))
.WithHealthChecks(h => h.AddCheckForReady<MongoReadyCheck>())
```

Do not call `AddLogging`/`AddMetrics`/`AddTracing` directly when using `ApiServiceBuilder` — the
chain already does, and calling twice double-registers exporters.

### Health checks

Liveness, readiness and startup are **separate** probes, and putting a check on the wrong one
causes real incidents — a dependency check on liveness makes an orchestrator restart a healthy pod
during a transient outage.

| Probe | Helper | Answers |
|---|---|---|
| App health (liveness) | `AddCheckForAppHealth<T>` | Is the process alive? Keep it dependency-free. |
| Ready | `AddCheckForReady<T>` | Can it serve traffic? Dependencies belong here. |
| Startup | `AddCheckForStartup<T>` | Has one-time init finished? |

### What you get for free

Do not re-implement these:

- Logs enriched with environment, span and application name.
- `TraceIdResponseHeaderMiddleware` returns the trace id to callers, so a user-reported error can
  be found in traces.
- `TenantIdEnricherMiddleware` puts the tenant on every log line.

For any signature not shown here, use `bytlabs-api-lookup` — do not guess.
