# BytLabs.Observability

Logging, metrics, tracing, and health checks built on Serilog + OpenTelemetry. Wired automatically by
[BytLabs.Api](BytLabs.Api.md)'s builder steps (`WithLogging`/`WithMetrics`/`WithTracing`/`WithHealthChecks`),
but can be used directly.

## Install

```xml
<PackageReference Include="BytLabs.Observability" />
```

## What's inside

| Type / method | Purpose |
|---|---|
| `ObservabilityConfiguration` | `ServiceName`, `CollectorUrl`, `Timeout`, `Logs.OpenTelemetryEnabled` |
| `AddLogging(config, configure?)` | Serilog: console + enrichers (env, span, app name); optional OTel log export |
| `AddMetrics(config, configure?)` | OpenTelemetry metrics → OTLP collector |
| `AddTracing(config, configure?)` | OpenTelemetry tracing → OTLP collector |
| `HealthChecks.ServiceExtensions` | `AddHealthChecks(...)`, `UseHealthChecks()`, `AddCheckForReady/Startup/AppHealth<T>` |
| Middlewares | `TraceIdResponseHeaderMiddleware`, `TenantIdEnricherMiddleware` |
| Default checks | `DefaultApplicationHealthCheck`, `DefaultApplicationReadyCheck`, `DefaultApplicationStartupCheck` |

## Configuration

```jsonc
{
  "ObservabilityConfiguration": {
    "ServiceName": "microservice-template",
    "CollectorUrl": "http://localhost:4317",
    "Timeout": 1000,
    "Logs": { "OpenTelemetryEnabled": false }
  }
}
```

## Usage

Via the API builder (recommended):

```csharp
ApiServiceBuilder.CreateBuilder(builder)
    .WithHttpContextAccessor(...).WithMultiTenantContext(...)
    .WithLogging()      // Serilog console + enrichers; OTel logs if enabled
    .WithMetrics()      // OTel metrics
    .WithTracing()      // OTel traces
    .WithHealthChecks() // default liveness/readiness/startup
    ...;
```

`BuildWebApp()` then adds `UseSerilogRequestLogging()`, the trace-id response header, tenant-id log
enrichment, and the health endpoints.

### Health checks

Three tagged checks and endpoints are registered by default:

| Endpoint | Tag | Default check |
|---|---|---|
| `/healthz` | `health` | `DefaultApplicationHealthCheck` (liveness) |
| `/readyz` | `ready` | `DefaultApplicationReadyCheck` (readiness) |
| `/startupz` | `startup` | `DefaultApplicationStartupCheck` |

Add your own checks under a probe by tag:

```csharp
.WithHealthChecks(hc => hc.AddCheckForReady<MyDependencyCheck>("my-dependency"))
```

### Logging & tracing

Serilog logs to the console with environment, trace/span, and `ApplicationName` enrichers. Set
`Logs.OpenTelemetryEnabled = true` to also export logs to the OTLP collector at
`{CollectorUrl}/v1/logs`. Metrics and traces export to the collector via OpenTelemetry. The
`TraceIdResponseHeaderMiddleware` returns the trace id in a response header for correlation, and
`TenantIdEnricherMiddleware` adds the tenant id to the logging scope.

## Related packages
- [BytLabs.Api](BytLabs.Api.md) — calls these from its builder steps and middleware.
- [BytLabs.Multitenancy](BytLabs.Multitenancy.md) — source of the tenant id used in log enrichment.
