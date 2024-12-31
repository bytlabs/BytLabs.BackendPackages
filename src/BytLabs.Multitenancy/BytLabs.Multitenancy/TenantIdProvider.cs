using BytLabs.Multitenancy.Exceptions;
using BytLabs.Multitenancy.Resolvers;
using Microsoft.Extensions.Logging;

namespace BytLabs.Multitenancy
{
    /// <summary>
    /// Default implementation of ITenantIdProvider that uses multiple resolvers to identify the current tenant.
    /// </summary>
    internal sealed class TenantIdProvider : ITenantIdProvider
    {
        private readonly IEnumerable<ITenantIdResolver> _resolvers;
        private readonly ILogger<TenantIdProvider> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantIdProvider"/> class.
        /// </summary>
        /// <param name="resolvers">Collection of tenant ID resolvers to use.</param>
        /// <param name="logger">Logger for tenant resolution operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when resolvers or logger is null.</exception>
        public TenantIdProvider(
            IEnumerable<ITenantIdResolver> resolvers,
            ILogger<TenantIdProvider> logger)
        {
            _resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current tenant identifier by trying each resolver in sequence.
        /// </summary>
        /// <returns>The resolved tenant identifier.</returns>
        /// <exception cref="FailedToResolveTenantIdException">Thrown when no resolver can identify the tenant.</exception>
        public TenantId GetTenantId()
        {
            foreach (var resolver in _resolvers)
            {
                try
                {
                    if (resolver.TryGetCurrent(out var tenantId))
                    {
                        _logger.LogTrace("Tenant {Tenant} was identified", tenantId);
                        return tenantId;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Exception caught during tenant identification in {TenantIdResolver}", resolver.GetType().Name);
                }
            }

            throw new FailedToResolveTenantIdException("Unable to resolve TenantId with given resolvers.");
        }
    }
}
