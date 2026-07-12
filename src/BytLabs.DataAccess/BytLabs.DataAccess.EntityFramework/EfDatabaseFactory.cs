using System.Collections.Concurrent;
using BytLabs.DataAccess.EntityFramework.Configuration;
using BytLabs.Infrastructure.Exceptions;
using BytLabs.Multitenancy;
using GuardClauses;
using Microsoft.EntityFrameworkCore;

namespace BytLabs.DataAccess.EntityFramework;

/// <summary>
/// Factory for creating per-tenant <typeparamref name="TDbContext"/> instances.
/// Resolves the tenant's connection string from configuration and caches the built
/// <see cref="DbContextOptions{TContext}"/> per tenant.
/// </summary>
/// <typeparam name="TDbContext">The consumer's DbContext type. Must expose a
/// <c>TDbContext(DbContextOptions&lt;TDbContext&gt;)</c> constructor.</typeparam>
public class EfDatabaseFactory<TDbContext> where TDbContext : DbContext
{
    private readonly EfDatabaseConfiguration _configuration;
    private readonly Action<DbContextOptionsBuilder, string> _configureProvider;
    private static readonly ConcurrentDictionary<string, DbContextOptions<TDbContext>> OptionsCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EfDatabaseFactory{TDbContext}"/> class.
    /// </summary>
    /// <param name="configuration">The EF database configuration containing the tenants array.</param>
    /// <param name="configureProvider">
    /// Delegate that applies the EF provider (e.g. <c>UseNpgsql</c>) to the options builder using the
    /// tenant's connection string.
    /// </param>
    public EfDatabaseFactory(
        EfDatabaseConfiguration configuration,
        Action<DbContextOptionsBuilder, string> configureProvider)
    {
        GuardClause.ArgumentIsNotNull(configuration, nameof(configuration));
        GuardClause.ArgumentIsNotNull(configureProvider, nameof(configureProvider));

        _configuration = configuration;
        _configureProvider = configureProvider;
    }

    /// <summary>
    /// Builds a <typeparamref name="TDbContext"/> for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The tenant to resolve a context for.</param>
    /// <returns>A new DbContext configured for the tenant's connection string.</returns>
    /// <exception cref="InfrastructureException">Thrown when the tenant has no configured connection.</exception>
    public TDbContext GetDbContextForTenant(TenantId tenantId)
    {
        GuardClause.ArgumentIsNotNull(tenantId, nameof(tenantId));

        var tenant = _configuration.Tenants.FirstOrDefault(t => t.TenantId == tenantId.Value);
        if (tenant is null)
            throw new InfrastructureException($"No Entity Framework connection configured for tenant '{tenantId.Value}'.");

        var options = OptionsCache.GetOrAdd(tenant.TenantId, _ =>
        {
            var builder = new DbContextOptionsBuilder<TDbContext>();
            _configureProvider(builder, tenant.ConnectionString);
            return builder.Options;
        });

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
    }
}
