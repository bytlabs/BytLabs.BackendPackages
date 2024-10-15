using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.Multitenancy
{
    public static class ServiceCollectionExtensions
    {
        public static MultitenancyBuilder AddMultitenancy(this IServiceCollection serviceCollection)
        {
            return new MultitenancyBuilder(serviceCollection);
        }
    }
}
