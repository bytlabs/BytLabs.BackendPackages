using System.ComponentModel.DataAnnotations;

namespace BytLabs.DataAccess.EntityFramework.Configuration;

/// <summary>
/// Configuration settings for Entity Framework database connections.
/// Multitenancy is physical: each tenant maps to its own connection string.
/// </summary>
public class EfDatabaseConfiguration : DatabaseConfiguration
{
    /// <summary>
    /// The per-tenant connection configuration. Each entry maps a tenant id to the
    /// connection string used to build that tenant's DbContext.
    /// </summary>
    [Required]
    public List<EfTenantConfiguration> Tenants { get; set; } = new();
}

/// <summary>
/// Connection configuration for a single tenant.
/// </summary>
public class EfTenantConfiguration
{
    /// <summary>
    /// The tenant identifier (matches <see cref="BytLabs.Multitenancy.TenantId.Value"/>).
    /// </summary>
    [Required]
    public string TenantId { get; set; } = null!;

    /// <summary>
    /// The connection string for this tenant's database.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = null!;
}
