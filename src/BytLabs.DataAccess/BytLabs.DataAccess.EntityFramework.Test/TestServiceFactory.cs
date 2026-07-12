using BytLabs.Application;
using BytLabs.Application.UserContext;
using BytLabs.DataAccess.EntityFramework.Configuration;
using BytLabs.DataAccess.EntityFramework.Test.Domain;
using BytLabs.Multitenancy;
using BytLabs.Multitenancy.Resolvers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BytLabs.DataAccess.EntityFramework.Test;

public static class TestServiceFactory
{
    /// <summary>
    /// Builds a service provider backed by a shared, open SQLite in-memory connection.
    /// The caller owns the connection and must dispose it when done.
    /// </summary>
    /// <remarks>
    /// Each call uses a unique tenant id. <see cref="EfDatabaseFactory{TDbContext}"/> caches the built
    /// options per tenant id in a static dictionary (a production optimization, as in the MongoDB
    /// factory); a unique id per test prevents one test reusing another test's cached options — which
    /// would bind a disposed connection and its dropped in-memory database.
    /// </remarks>
    public static ServiceProvider Create(SqliteConnection connection)
    {
        var tenantId = "tenant-" + Guid.NewGuid().ToString("N");

        var services = new ServiceCollection();

        services.AddFakeLogging();
        services.AddMultitenancy()
            .AddResolver<ITenantIdResolver>(_ => new ValueTenantIdResolver(new TenantId(tenantId)));
        services.AddCQS(new[] { typeof(TestServiceFactory).Assembly }, _ => { });

        // EfRepository and the domain-event decorator depend on IUserContextProvider.
        var userContext = new Mock<IUserContextProvider>();
        userContext.Setup(x => x.GetUserId()).Returns("test-user");
        services.AddSingleton(userContext.Object);

        var config = new EfDatabaseConfiguration
        {
            UseTransactions = true,
            Tenants =
            {
                new EfTenantConfiguration { TenantId = tenantId, ConnectionString = connection.ConnectionString }
            }
        };

        services.AddEntityFrameworkDatabase<TestDbContext>(config,
            (options, _) => options.UseSqlite(connection))
            .AddEfRepository<OrderAggregate, Guid>()
            .AddEfRepository<ProductAggregate, Guid>();

        var provider = services.BuildServiceProvider();

        // Create schema once on the shared connection.
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        db.Database.EnsureCreated();

        return provider;
    }

    /// <summary>Opens a new shared in-memory SQLite connection (kept open for the DB's lifetime).</summary>
    public static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }
}
