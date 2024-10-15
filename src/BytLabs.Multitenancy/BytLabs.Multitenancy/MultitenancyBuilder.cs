using BytLabs.Multitenancy.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BytLabs.Multitenancy
{
    /// <summary>
    /// Builder class for configuring multi-tenancy services.
    /// </summary>
    public sealed class MultitenancyBuilder
    {
        /// <summary>
        /// Gets the service collection being configured.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public MultitenancyBuilder(IServiceCollection services)
        {
            Services = services;
            Services.TryAddSingleton<ITenantIdProvider, TenantIdProvider>();
        }

        /// <summary>
        /// Adds a tenant ID resolver instance to the service collection.
        /// </summary>
        /// <param name="resolver">The resolver instance to add.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MultitenancyBuilder AddResolver(ITenantIdResolver resolver)
        {
            Services.TryAddSingleton(resolver);
            return this;
        }

        /// <summary>
        /// Adds a tenant ID resolver type to the service collection.
        /// </summary>
        /// <typeparam name="T">The type of resolver to add.</typeparam>
        /// <returns>The builder instance for method chaining.</returns>
        public MultitenancyBuilder AddResolver<T>()
            where T : class, ITenantIdResolver
        {
            Services.TryAddSingleton<ITenantIdResolver, T>();
            return this;
        }

        /// <summary>
        /// Adds a tenant ID resolver using a factory function.
        /// </summary>
        /// <typeparam name="T">The type of resolver to add.</typeparam>
        /// <param name="factory">The factory function to create the resolver.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public MultitenancyBuilder AddResolver<T>(Func<IServiceProvider, T> factory)
            where T : class, ITenantIdResolver
        {
            Services.TryAddSingleton<ITenantIdResolver>(factory);
            return this;
        }
    }
}
