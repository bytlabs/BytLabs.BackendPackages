---
name: bytlabs-service-setup
description: Use when creating a new .NET service on the BytLabs packages, or adding BytLabs to an existing service - covers package references, the ApiServiceBuilder host chain in Program.cs, infrastructure DI registration, and configuration binding.
---

# Setting up a BytLabs service

## Which packages to reference

Requirements: **.NET 8 SDK**, and **MongoDB ≥ 4.2** if using the Mongo provider.

Packages are versioned together. With central package management
(`Directory.Packages.props`):

```xml
<PropertyGroup>
  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  <BytLabsPackageVersion>5.2.0</BytLabsPackageVersion>
</PropertyGroup>
<ItemGroup>
  <PackageVersion Include="BytLabs.Domain"             Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.Application"        Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.DataAccess"         Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.DataAccess.MongoDB" Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.Api"                Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.Api.Graphql"        Version="$(BytLabsPackageVersion)" />
</ItemGroup>
```

Choose **one** persistence package: `BytLabs.DataAccess.MongoDB` (default) or
`BytLabs.DataAccess.EntityFramework`. `BytLabs.Api.Graphql` and `BytLabs.States.Domain` are
optional. `BytLabs.Multitenancy` and `BytLabs.Observability` come in transitively via
`BytLabs.Api`.

## The builder chain — the order is compiler-enforced

`ApiServiceBuilder` is a step-interface state machine. Each `With…` call returns the interface for
the *next* step only, so the order cannot be rearranged and a wrong order is a compile error, not
a runtime surprise:

```
CreateBuilder
  → WithHttpContextAccessor   (IInitialStep      → IMultiTenantStep)
  → WithMultiTenantContext    (IMultiTenantStep  → ILoggingStep)
  → WithLogging               (ILoggingStep      → IMetricsStep)
  → WithMetrics               (IMetricsStep      → ITracingStep)
  → WithTracing               (ITracingStep      → IHealthCheckStep)
  → WithHealthChecks          (IHealthCheckStep  → IAdditionalConfigurationStep)
  → WithServiceConfiguration  (…                 → IWebApplicationBuilder)
  → BuildWebApp               (returns WebApplication)
```

Every `With…` takes an optional configure delegate. The working `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = ApiServiceBuilder.CreateBuilder(builder)
    .WithHttpContextAccessor(uc => uc.AddResolver<HttpUserContextResolver>())
    .WithMultiTenantContext(mt => mt.AddResolver<FromHeaderTenantIdResolver>())
    .WithLogging().WithMetrics().WithTracing().WithHealthChecks()
    .WithServiceConfiguration(services =>
    {
        services.AddInfrastructure(builder.Configuration);   // your DI (below)
        services.AddGraphQLService()
            .AddDynamicDataTypes()
            .AddMutationType<Mutation>().AddQueryType<Query>();
    })
    .BuildWebApp(app => { app.UseAuthentication(); app.UseAuthorization(); app.MapGraphQL(); });

app.Run();
```

All your own middleware and endpoint mapping goes inside the `BuildWebApp` delegate.

> **Careful with the README's quick tour.** It chains `AddMongoDbQuerySettings()`,
> `AddCommandTypes()` and `AddDtoTypes()`, which are **not** package methods — they are
> consumer-side grouping helpers from the service template that wrap repeated
> `AddCommandType<T>()` / `AddDtoType<T>()` calls. Calling them in a service that has not defined
> them will not compile. See `bytlabs-graphql` for the real package surface.

## Infrastructure registration

```csharp
services.AddCQS(new[] { typeof(CreateProductCommand).Assembly });
services.AddAutoMapper(typeof(ProductMappingProfile));
services.AddMongoDatabase(config.GetConfiguration<MongoDatabaseConfiguration>())
    .AddMongoRepository<Product, Guid>();
```

`AddCQS` must receive every assembly containing handlers or validators. See `bytlabs-cqrs`. For
the Entity Framework alternative to the last two lines, see `bytlabs-data-access`.

## Configuration binding

`GetConfiguration<T>()` binds and validates a configuration section **named after the type**:

```csharp
var mongo = builder.Configuration.GetConfiguration<MongoDatabaseConfiguration>();
```

reads the `"MongoDatabaseConfiguration"` section:

```json
{
  "MongoDatabaseConfiguration": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "my-service"
  },
  "ObservabilityConfiguration": {
    "ServiceName": "my-service",
    "CollectorUrl": "http://localhost:4317"
  }
}
```

If the section name does not match the type name exactly, binding fails.

## Adding BytLabs to an existing service

In this order — each step leaves the service building and testable:

1. Add the package references above.
2. Replace the host bootstrap in `Program.cs` with the `ApiServiceBuilder` chain, moving all
   existing middleware and endpoint registration into the `BuildWebApp` delegate and all existing
   DI into `WithServiceConfiguration`.
3. Register a tenant resolver. Single-tenant services can use `ValueTenantIdResolver` with a fixed
   value rather than adopting tenancy immediately.
4. Move persistence behind `IRepository<TAggregate, TId>`, one aggregate at a time.
5. Move business logic into aggregate methods last — this is the largest change and the easiest to
   defer.

Do not try to convert the whole domain in one pass.

## Common failures

| Symptom | Cause |
|---|---|
| Chain method not found / does not compile | Steps called out of order. The compiler is telling you the sequence; follow the order above. |
| Handlers do not resolve at runtime | `AddCQS` never called, or the handler's assembly not passed to it. |
| `FailedToResolveTenantIdException` on every request | No tenant resolver registered in `WithMultiTenantContext`. |
| Configuration binding throws or yields defaults | Section name does not match the configuration type name. |

For any signature not shown here, use `bytlabs-api-lookup` — do not guess.
