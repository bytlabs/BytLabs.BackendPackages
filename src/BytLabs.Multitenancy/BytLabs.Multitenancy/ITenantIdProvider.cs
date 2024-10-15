using System.Diagnostics.CodeAnalysis;

namespace BytLabs.Multitenancy
{
    /// <summary>
    /// Defines a contract for retrieving the current tenant identifier.
    /// </summary>
    public interface ITenantIdProvider
    {
        /// <summary>
        /// Gets the current tenant identifier.
        /// </summary>
        /// <returns>The current tenant identifier.</returns>
        /// <exception cref="FailedToResolveTenantId">Thrown when tenant ID cannot be resolved.</exception>
        TenantId GetTenantId();
    }
}