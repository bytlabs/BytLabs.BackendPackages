using BytLabs.Application.DataAccess;
using BytLabs.DataAccess.EntityFramework.Configuration;
using BytLabs.Domain.Entities;
using BytLabs.Infrastructure.Exceptions;
using BytLabs.Multitenancy;
using GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BytLabs.DataAccess.EntityFramework;

/// <summary>
/// Dependency-injection extensions for the Entity Framework data-access package.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the EF database stack: per-tenant DbContext resolution, unit of work,
    /// command transactions (via the base package), and a DbContext health check.
    /// </summary>
    /// <typeparam name="TDbContext">The consumer's DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The EF database configuration (tenants + transaction settings).</param>
    /// <param name="configureProvider">
    /// Delegate applying the EF provider using a tenant connection string, e.g.
    /// <c>(options, conn) =&gt; options.UseNpgsql(conn)</c>.
    /// </param>
    /// <exception cref="InfrastructureException">Thrown when <c>UseTransactions</c> is false.</exception>
    public static IServiceCollection AddEntityFrameworkDatabase<TDbContext>(
        this IServiceCollection services,
        EfDatabaseConfiguration config,
        Action<DbContextOptionsBuilder, string> configureProvider)
        where TDbContext : DbContext
    {
        GuardClause.ArgumentIsNotNull(config, nameof(config));
        GuardClause.ArgumentIsNotNull(configureProvider, nameof(configureProvider));

        if (!config.UseTransactions)
            throw new InfrastructureException(
                "BytLabs.DataAccess.EntityFramework requires UseTransactions to be true: " +
                "SaveChanges runs inside the unit-of-work commit driven by the command transaction decorator.");

        services.AddSingleton(config);

        // Base wiring: unit-of-work options + command transaction decorator.
        services.AddDatabase(config);

        // Per-tenant DbContext factory.
        services.AddSingleton(new EfDatabaseFactory<TDbContext>(config, configureProvider));

        // Scoped DbContext resolved for the current tenant.
        services.AddScoped<DbContext>(sp =>
        {
            var factory = sp.GetRequiredService<EfDatabaseFactory<TDbContext>>();
            var tenantIdProvider = sp.GetRequiredService<ITenantIdProvider>();
            return factory.GetDbContextForTenant(tenantIdProvider.GetTenantId());
        });

        // Allow consumers to inject their concrete DbContext type (same scoped instance).
        services.AddScoped(sp => (TDbContext)sp.GetRequiredService<DbContext>());

        // Unit of work.
        services.AddScoped<EfUnitOfWork>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EfUnitOfWork>());

        // Health check (provider-agnostic).
        services.AddHealthChecks().AddDbContextCheck<TDbContext>();

        return services;
    }

    /// <summary>
    /// Registers the data-access services for an aggregate root:
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="IRepository{TEntity,TIdentity}"/> (write side) — tracked, audited, domain-event
    /// dispatching; use it in command handlers.
    /// </description></item>
    /// <item><description>
    /// <see cref="IQueryable{TEntity}"/> (read side) — a no-tracking <c>DbSet</c> for the current
    /// tenant's <c>DbContext</c>; use it in query handlers / resolvers.
    /// </description></item>
    /// </list>
    /// </summary>
    public static IServiceCollection AddEfRepository<TEntity, TIdentity>(this IServiceCollection services)
        where TEntity : class, IAggregateRoot<TIdentity>
    {
        // Write side: tracked repository + domain-event dispatch decorator.
        services.TryAddScoped<IRepository<TEntity, TIdentity>, EfRepository<TEntity, TIdentity>>();
        services.AddDomainEventsDecorator<TEntity, TIdentity>();

        // Read side: a no-tracking queryable over the current tenant's DbContext, for queries only.
        services.TryAddScoped<IQueryable<TEntity>>(sp =>
            sp.GetRequiredService<DbContext>().Set<TEntity>().AsNoTracking());

        return services;
    }
}
