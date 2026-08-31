---
name: bytlabs-api-lookup
description: Use when you need the exact signature, members, or existence of a BytLabs type or extension method - before writing code against BytLabs.Domain, BytLabs.Application, BytLabs.DataAccess, BytLabs.Api, BytLabs.Api.Graphql, BytLabs.Multitenancy, BytLabs.Observability, or BytLabs.States.Domain. Reads ground truth from the installed package instead of guessing or decompiling.
---

# Looking up the BytLabs API

BytLabs packages ship their full API surface and reference documentation inside the nupkg. Read
them. Do not decompile the DLL, and do not infer a signature from a type name.

## Procedure

### 1. Resolve the installed version

Central package management is the BytLabs convention, so check it first:

```bash
grep -rn "BytLabs" Directory.Packages.props 2>/dev/null || grep -rn "BytLabs" --include=*.csproj .
```

Look for a `$(BytLabsPackageVersion)` property — the packages are versioned together, so one
version usually covers all of them.

### 2. Read the API surface

```
<cache>/<package-id-lowercased>/<version>/lib/net8.0/<PackageId>.xml
```

`<cache>` is `$NUGET_PACKAGES` when set, otherwise `~/.nuget/packages`
(`%USERPROFILE%\.nuget\packages` on Windows). **Directory names are lowercased; the `.xml` file
keeps the original casing.**

This file has one `<member>` per public type and member, with the full signature in the `name`
attribute and the doc comment as its body. Grep it for the type you need:

```bash
grep -A12 'name="T:BytLabs.Api.ApiServiceBuilder"' \
  ~/.nuget/packages/bytlabs.api/5.2.0/lib/net8.0/BytLabs.Api.xml
```

Member name prefixes: `T:` type, `M:` method, `P:` property, `F:` field, `E:` event. Generic
arity is a backtick-suffixed count, and type arguments inside a signature appear positionally, so
`IRepository<TAggregateRoot, TIdentity>` is:

```
T:BytLabs.Application.DataAccess.IRepository`2
M:BytLabs.Application.DataAccess.IRepository`2.InsertAsync(`0,System.Threading.CancellationToken)
```

**Namespaces are deeper than the type tables below suggest** — `IRepository` lives in
`BytLabs.Application.DataAccess`, `ICommand` in `BytLabs.Application.CQS.Commands`, `IQuery` in
`BytLabs.Application.CQS.Queries`. Grep for the bare type name rather than guessing a full path:

```bash
grep -oE 'name="T:[^"]*IRepository[^"]*"' \
  ~/.nuget/packages/bytlabs.application/5.2.0/lib/net8.0/BytLabs.Application.xml
```

These files run to thousands of lines. Always grep for what you need; never read one whole.

### 3. Read the prose reference

```
<cache>/<package-id-lowercased>/<version>/docs/<PackageId>.md
```

Registration examples, usage patterns, and gotchas — the same reference published at
https://github.com/BytLabs/BytLabs.BackendPackages/tree/main/docs/libraries.

### 4. Fallback only if the package is not restored

Run `dotnet restore` first. If it still is not there, read the docs on GitHub for the tag closest
to the installed version — and tell the user the answer may not match their version.

**Versions before 5.2.0 shipped neither the `.xml` nor the `docs/` folder.** If the cache entry
has only a DLL and a README, the service is on an older release: say so, use the GitHub docs, and
flag that the answer is not version-verified.

## Package IDs

Directory names in the cache are these, lowercased.

| PackageId | Contains |
|---|---|
| `BytLabs.Domain` | `Entity<TId>`, `AggregateRootBase<TId>`, `ValueObject`, domain events, `ISoftDeletable`, `IAuditable`, `IHaveDynamicData`, `BusinessRule<T>` |
| `BytLabs.Application` | `ICommand`, `IQuery`, handlers, `IRepository<,>`, `IUnitOfWork`, `AddCQS`, user context, dynamic-data filter inputs |
| `BytLabs.DataAccess` | `DatabaseConfiguration`, `AddDatabase<TConfig>`, `UnitOfWorkFactory`, transaction and domain-event decorators |
| `BytLabs.DataAccess.MongoDB` | `AddMongoDatabase`, `AddMongoRepository<,>`, `MongoDatabaseConfiguration`, dynamic-data filter extensions |
| `BytLabs.DataAccess.EntityFramework` | `AddEntityFrameworkDatabase<TDbContext>`, `AddEfRepository<,>`, `EfDatabaseConfiguration` |
| `BytLabs.Api` | `ApiServiceBuilder`, `ApiBuilderSteps`, `GetConfiguration<T>()`, tenant and user-context resolvers |
| `BytLabs.Api.Graphql` | `AddGraphQLService`, `AddCommandType<T>`, `AddDtoType<T>`, `DtoType<T>`, typed error results |
| `BytLabs.Multitenancy` | `TenantId`, `ITenantIdProvider`, `ITenantIdResolver`, `MultitenancyBuilder`, `AddMultitenancy` |
| `BytLabs.Observability` | `ObservabilityConfiguration`, `AddLogging`, `AddMetrics`, `AddTracing`, health checks, middlewares |
| `BytLabs.States.Domain` | `StatefulAggregateBase<...>`, `StateBase`, `TransitionBase`, `Trigger`, `TransitionRule` |
| `BytLabs.Infrastructure` | Shared infrastructure exception (near-empty placeholder) |

**`BytLabs.Api.Graphql` is the GraphQL package.** Versions `5.2.0-alpha.123` through
`5.2.0-alpha.125` were published as `BytLabs.Hotchocolate`, with the assembly and namespaces
renamed to match, by an accidental project rename. That was reverted in `5.2.0`. If a service
references `BytLabs.Hotchocolate` or has `using BytLabs.Hotchocolate...`, tell them to move back
to `BytLabs.Api.Graphql` — the namespaces revert with it.

## Rules

- Never state a BytLabs signature you have not read from the `.xml` or the `.md`.
- If the installed version is newer than any skill you are working from, the installed package
  wins. Say so when it changes your answer.
- The type may genuinely not exist. Report that rather than inventing a plausible name.
- Method names in this codebase are not always spelled the way you expect
  (`ExcludeSoftDeletedEntites`, `AppySortingWithDynamicData`). Copy what the `.xml` says; do not
  "correct" it.
