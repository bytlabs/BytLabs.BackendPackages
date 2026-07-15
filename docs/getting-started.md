---
title: Getting Started
---

# Getting Started

This guide takes you from nothing to a running, multi-tenant, observable GraphQL microservice built on
BytLabs Backend Packages. It is intentionally short: follow the steps top to bottom, then dive into the
[Library Reference](libraries/index.md) for depth.

---

## Prerequisites

- **.NET 8 SDK**
- **MongoDB 4.2 or newer** (the driver is 3.x) — a local Docker container is fine
- An **OpenTelemetry collector** only if you want to export metrics/traces (optional)

---

## Choose your path

- **Start from the template (recommended).** The [BytLabs.MicroserviceTemplate](https://github.com/bytlabs/BytLabs.MicroserviceTemplate) is a working service
  that doubles as a recipe catalog. Copy it, rename it, and start modelling your domain. This is the
  fastest way to get every convention right.
- **Add the packages to an existing service.** Follow the steps below.

---

## 1. Install the packages

Packages are versioned together. With central package management, set one version in
`Directory.Packages.props`:

```xml
<PropertyGroup>
  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  <BytLabsPackageVersion>4.2.0</BytLabsPackageVersion>
</PropertyGroup>
<ItemGroup>
  <PackageVersion Include="BytLabs.Domain"             Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.Application"        Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.DataAccess"         Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.DataAccess.MongoDB" Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.Api"               Version="$(BytLabsPackageVersion)" />
  <PackageVersion Include="BytLabs.Api.Graphql"       Version="$(BytLabsPackageVersion)" />
</ItemGroup>
```

Reference each package from the project that needs it (Domain → `BytLabs.Domain`, Application →
`BytLabs.Application`, Infrastructure → `BytLabs.DataAccess.MongoDB`, Api → `BytLabs.Api` +
`BytLabs.Api.Graphql`).

---

## 2. Configure `appsettings.json`

```jsonc
{
  "ObservabilityConfiguration": {
    "ServiceName": "my-service",
    "CollectorUrl": "http://localhost:4317",
    "Timeout": 1000
  },
  "MongoDatabaseConfiguration": {
    "DatabaseName": "myService",
    "ConnectionString": "mongodb://localhost:27017?retryWrites=false",
    "UseTransactions": false
  }
}
```

Both sections are bound and validated at startup. Keep `UseTransactions: false` unless your MongoDB is
a replica set.

---

## 3. Bootstrap the host (`Program.cs`)

`ApiServiceBuilder` wires the standard concerns — user context, multitenancy, logging, metrics,
tracing, health, and GraphQL — in one fluent chain.

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = ApiServiceBuilder.CreateBuilder(builder)
    .WithHttpContextAccessor(uc => uc.AddResolver<HttpUserContextResolver>())
    .WithMultiTenantContext(mt => mt.AddResolver<FromHeaderTenantIdResolver>())
    .WithLogging()
    .WithMetrics()
    .WithTracing()
    .WithHealthChecks()
    .WithServiceConfiguration(services =>
    {
        services.AddInfrastructure(builder.Configuration);   // step 4
        services.AddGraphQLService()
            .AddMongoDbQuerySettings()
            .AddCommandTypes()
            .AddDtoTypes()
            .AddMutationType<Mutation>()
            .AddQueryType<Query>();
    })
    .BuildWebApp(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGraphQL();
    });

app.Run();

public partial class Program { } // enables WebApplicationFactory in acceptance tests
```

---

## 4. Wire infrastructure

Register CQRS, AutoMapper, the MongoDB database, and a repository per aggregate. `AddCQS`
automatically adds the validation and logging pipeline behaviors.

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    services.AddCQS(new[] { typeof(CreateProductCommand).Assembly });
    services.AddAutoMapper(typeof(ProductMappingProfile));

    services.AddMongoDatabase(configuration.GetConfiguration<MongoDatabaseConfiguration>())
        .AddMongoRepository<Product, Guid>();

    return services;
}
```

---

## 5. Build your first feature

**Aggregate** — business logic and invariants live here; state changes raise domain events.

```csharp
public sealed class Product : AggregateRootBase<Guid>, ISoftDeletable
{
    public string Name { get; private set; }
    public bool IsDeleted { get; private set; }

    private Product(Guid id, string name) : base(id) => Name = name;

    public static Product Create(Guid id, string name)
    {
        var product = new Product(id, name);
        product.AddDomainEvent(new ProductCreated(id, name));
        return product;
    }

    public void Remove() { IsDeleted = true; AddDomainEvent(new ProductRemoved(Id)); }
}
```

**Command + handler** — the write side; the handler persists through `IRepository<T, TId>`.

```csharp
public record CreateProductCommand(Guid Id, string Name) : ICommand<ProductDto>;

public class CreateProductCommandHandler(IRepository<Product, Guid> repo, IMapper mapper)
    : ICommandHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
        => mapper.Map<ProductDto>(await repo.InsertAsync(Product.Create(request.Id, request.Name), ct));
}
```

**Expose it over GraphQL** — a mutation forwards the command; a query projects from MongoDB.

```csharp
public partial class Mutation
{
    [Error(typeof(BusinessError))]
    [Error(typeof(ValidationError))]
    public Task<ProductDto> CreateProduct(CreateProductCommand input, [Service] IMediator mediator, CancellationToken ct)
        => mediator.Send(input, ct);
}

public partial class Query
{
    [UsePaging, UseProjection, UseFiltering(Type = typeof(Product)), UseSorting(Type = typeof(Product))]
    public IExecutable<ProductDto> GetProducts([Service] IMongoDatabase db)
        => db.GetCollection<Product>()
             .Aggregate()
             .Project(Builders<Product>.Projection.As<ProductDto>())
             .AsExecutable();
}
```

That is a complete, validated, multi-tenant GraphQL feature. (Add a `CreateProductCommandValidator`
deriving from `AbstractValidator<CreateProductCommand>` and it runs automatically.)

---

## 6. Run and try it

Start MongoDB and the service:

```bash
docker run -d -p 27017:27017 mongo:7.0
dotnet run --project src/MyService.Api
```

Call the GraphQL endpoint (a `Tenant` header selects the tenant database):

```bash
curl -X POST http://localhost:8080/graphql/ \
  -H "Content-Type: application/json" \
  -H "Tenant: acme" \
  -d '{"query":"mutation { createProduct(input:{ id:\"...\", name:\"Widget\" }) { product { id name } errors { __typename } } }"}'
```

The GraphQL IDE is available at `/graphql/` in the browser.

---

## How multitenancy works

Tenant isolation is automatic. The `Tenant` header is resolved into a tenant id, and the service uses
a separate MongoDB database per tenant, named `"{DatabaseName}-{tenantId}"` (e.g.
`myService-acme`). Your domain code carries no tenant field and no per-query tenant filter.

---

## Testing

- **Unit tests** exercise aggregates directly (xUnit + FluentAssertions) — no infrastructure required.
- **Acceptance tests** boot the API in-process with `WebApplicationFactory` and call it through the
  generated GraphQL client against a real MongoDB.

The `BytLabs.MicroserviceTemplate` includes ready-to-copy examples of both.

---

## Next steps

- Browse the [Library Reference](libraries/index.md) for the full API of every package.
- Start from the `BytLabs.MicroserviceTemplate` and its recipe catalog for end-to-end patterns
  (dynamic data, soft-delete, sub-entities, authorization, advanced GraphQL).
