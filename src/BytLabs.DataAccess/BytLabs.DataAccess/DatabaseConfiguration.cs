using System.ComponentModel.DataAnnotations;

namespace BytLabs.DataAccess;

/// <summary>
/// Provides configuration settings for database operations and behavior.
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Gets or sets whether database transactions should be enabled.
    /// </summary>
    /// <remarks>
    /// Some databases like MongoDB require specific configurations (e.g., multiple replicas) 
    /// to support transactions. Set this to false if your database configuration doesn't 
    /// support transactions or if you want to disable transaction management.
    /// </remarks>
    [Required]
    public bool UseTransactions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to bypass the standard database naming convention.
    /// </summary>
    /// <remarks>
    /// When set to true, the system will preserve the exact database names as specified,
    /// without applying any standardized naming conventions. This is useful for:
    /// - Working with legacy databases
    /// - Maintaining specific naming requirements
    /// - Integration with external systems that expect specific database names
    /// </remarks>
    public bool IgnoreDatabaseNamingConvention { get; set; }
}