using System.Diagnostics.CodeAnalysis;

namespace BytLabs.Multitenancy.Resolvers
{
    /// <summary>
    /// A simple tenant ID resolver that returns a predefined tenant ID value.
    /// Useful for testing or as a fallback resolver.
    /// </summary>
    public class ValueTenantIdResolver : ITenantIdResolver
    {
        private readonly TenantId? _tenantId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTenantIdResolver"/> class.
        /// </summary>
        /// <param name="tenantId">The predefined tenant ID to return.</param>
        public ValueTenantIdResolver(TenantId? tenantId)
        {
            _tenantId = tenantId;
        }

        /// <summary>
        /// Attempts to get the predefined tenant ID.
        /// </summary>
        /// <param name="tenantId">The predefined tenant ID if set; otherwise, null.</param>
        /// <returns>true if a tenant ID was set; otherwise, false.</returns>
        public bool TryGetCurrent([NotNullWhen(true)] out TenantId? tenantId)
        {
            tenantId = _tenantId;
            return _tenantId is not null;
        }
    }
}
