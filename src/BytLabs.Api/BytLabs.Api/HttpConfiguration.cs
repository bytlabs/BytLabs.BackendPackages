using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.Api
{
    /// <summary>
    /// Provides configuration extensions for HTTP-related services.
    /// </summary>
    public static class HttpConfiguration
    {
        private const string AuthorizationHeaderName = "Authorization";
        private const string TenantHeaderName = "Tenant";

        /// <summary>
        /// Adds header propagation configuration to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddHeadersPropagations(this IServiceCollection serviceCollection) =>
            serviceCollection.AddHeaderPropagation(options =>
            {
                options.Headers.Add(AuthorizationHeaderName);
                options.Headers.Add(TenantHeaderName);
            });
    }
}
