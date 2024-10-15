---
title: Getting Started
---

# Getting Started

## Installation

Add the required NuGet packages to your project:
```bash

#for domain layer
dotnet add package BytLabs.Domain

#For application layer
dotnet add package BytLabs.Application

# for infrastructure layer
dotnet add package BytLabs.DataAccess
dotnet add package BytLabs.DataAccess.MongoDB
dotnet add package BytLabs.Multitenancy
dotnet add package BytLabs.Observability
```

## Basic Setup

### 1. Configure CQRS

```csharp
services.AddCQS(options => {
    options.AddCommandHandler<YourCommand, YourCommandHandler>();
    options.AddQueryHandler<YourQuery, YourQueryHandler>();
});
```

### 2. Configure Multitenancy
```csharp
services.AddMultitenancy(options => {
    options.AddResolver<YourTenantResolver>();
});
```


### 3. Set Up MongoDB Database

```csharp
services.AddMongoDatabase(new MongoDatabaseConfiguration {
    ConnectionString = "your_connection_string",
    DatabaseName = "your_database",
    UseTransactions = true
});
```

### 4. Enable Observability

```csharp
services.AddObservability(options => {
options.ServiceName = "YourService";
options.Version = "1.0.0";
});
```



## Example Usage

### Creating a Command Handler

```csharp

public record CreateUserCommand(string Name, string Email) : ICommand<Guid>;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken token)
    {
        // Implementation
    }
}
```


### Working with Repositories

The framework provides a generic repository interface `IRepository<TEntity, TKey>` for MongoDB operations. Here's how to register and use repositories:

First, register the repository for your entity type:

```csharp
services.AddMongoRepository<User, Guid>();
```

Next, inject the repository into your command handler:

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IRepository<User, Guid> _userRepository;

    public CreateUserCommandHandler(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    ...
}
```


## Next Steps

- Review the API documentation for detailed information about each package
- Check out the sample projects in the repository
- Join our community for support and discussions
