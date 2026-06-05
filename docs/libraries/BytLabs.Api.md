# BytLabs.Api

The ASP.NET Core host builder. `ApiServiceBuilder` is a fluent, step-by-step pipeline that wires the
standard cross-cutting concerns (user context, multitenancy, logging, metrics, tracing, health
checks) and builds the `WebApplication` with the standard middleware. Also provides config binding and
the HTTP-based user/tenant resolvers.

## Install

```xml
<PackageReference Include="BytLabs.Api" />
```

## What's inside

| Type | Purpose |
|------|---------|
| `ApiServiceBuilder` (+ `ApiBuilderSteps`) | Fluent host builder enforcing configuration order |
| `ConfigurationExtensions.GetConfiguration<T>()` | Bind + validate a config section (by type name) |
| `TenantProvider.FromHeaderTenantIdResolver` | Resolves the tenant from a request header |
| `UserContextResolvers.HttpUserContextResolver` | Resolves the user id from the HTTP context |
| `HttpConfiguration` | Header propagation helpers used by the builder |

## Usage — `Program.cs`

The builder is a typed step chain (each method returns the next allowed step), so the order is
enforced at compile time:

```csharp
var builder = WebApplication.CreateBuilder(args);

var webAppBuilder = ApiServiceBuilder.CreateBuilder(builder)
    .WithHttpContextAccessor(userContext =>
        userContext.AddResolver<HttpUserContextResolver>())          // who is acting
    .WithMultiTenantContext(mt =>
        mt.AddResolver<FromHeaderTenantIdResolver>())                // which tenant
    .WithLogging()                                                   // Serilog
    .WithMetrics()                                                   // OpenTelemetry metrics
    .WithTracing()                                                   // OpenTelemetry tracing
    .WithHealthChecks()                                              // liveness/readiness/startup
    .WithServiceConfiguration(services =>
    {
        services.AddInfrastructure(builder.Configuration);
        services.AddGraphQLService()/* ... */;                      // from BytLabs.Api.Graphql
    });

var app = webAppBuilder.BuildWebApp(app =>
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapGraphQL();
});

app.Run();

public partial class Program { } // for WebApplicationFactory in acceptance tests
```

### What each step does
- **WithHttpContextAccessor** — adds header propagation, `IHttpContextAccessor`, and user-context
  providers (`AddUserContextProviders`); configure resolvers in the callback.
- **WithMultiTenantContext** — `AddMultitenancy()`; register `ITenantIdResolver`s (see
  [BytLabs.Multitenancy](BytLabs.Multitenancy.md)).
- **WithLogging / WithMetrics / WithTracing** — read `ObservabilityConfiguration` and configure
  Serilog + OpenTelemetry (see [BytLabs.Observability](BytLabs.Observability.md)).
- **WithHealthChecks** — registers the default liveness/readiness/startup checks.
- **WithServiceConfiguration** — your DI registrations (infrastructure, GraphQL, auth, etc.).
- **BuildWebApp** — builds the app and adds standard middleware: header propagation, Serilog request
  logging, health-check endpoints, trace-id response header, tenant-id log enrichment, and the
  developer exception page in Development. The callback is where you add app-specific middleware.

### Strongly-typed configuration

`GetConfiguration<T>()` binds the section named after the type and validates DataAnnotations:

```csharp
var cfg = builder.Configuration.GetConfiguration<MongoDatabaseConfiguration>();
// reads "MongoDatabaseConfiguration" section, throws if missing/invalid
```

### Tenant & user resolution
- `FromHeaderTenantIdResolver` reads the tenant from a request header (the template/tests send a
  `Tenant` header). Register it in `WithMultiTenantContext`.
- `HttpUserContextResolver` resolves the acting user id from the HTTP context for audit stamping.
  Register it in `WithHttpContextAccessor`.

## Related packages
- [BytLabs.Api.Graphql](BytLabs.Api.Graphql.md) — GraphQL server setup layered on this host.
- [BytLabs.Observability](BytLabs.Observability.md) — logging/metrics/tracing/health the steps configure.
- [BytLabs.Multitenancy](BytLabs.Multitenancy.md) — tenant resolution registered via `WithMultiTenantContext`.
