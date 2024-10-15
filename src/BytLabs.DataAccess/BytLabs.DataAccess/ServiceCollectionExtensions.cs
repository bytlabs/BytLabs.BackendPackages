using BytLabs.Application.DataAccess;
using BytLabs.DataAccess.Decorators;
using BytLabs.DataAccess.DomainEvents;
using BytLabs.DataAccess.UnitOfWork;
using BytLabs.Domain.Entities;
using GuardClauses;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BytLabs.DataAccess
{
    /// <summary>
    /// Provides extension methods for configuring database-related services in the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures a repository to publish domain events when aggregates are modified.
        /// </summary>
        /// <typeparam name="TAggregateRoot">The type of aggregate root entity.</typeparam>
        /// <typeparam name="TIdentity">The type of the aggregate's identifier.</typeparam>
        /// <param name="services">The service collection to add the decorator to.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// This method should be called by database implementation libraries when registering
        /// repositories that implement IRepository{TEntity, TIdentity}. It ensures that:
        /// - Domain events are automatically published when aggregates are modified
        /// - Event handling follows the proper order of operations
        /// - Events are published only after successful persistence
        /// </remarks>
        public static IServiceCollection AddDomainEventsDecorator<TAggregateRoot, TIdentity>(this IServiceCollection services)
            where TAggregateRoot : IAggregateRoot<TIdentity>
        {
            services.Decorate<IRepository<TAggregateRoot, TIdentity>, DomainEventDispatcherDecorator<TAggregateRoot, TIdentity>>();
            return services;
        }

        /// <summary>
        /// Configures core database services and behaviors based on the provided configuration.
        /// </summary>
        /// <typeparam name="TDatabaseConfiguration">The type of database configuration.</typeparam>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="databaseConfiguration">The database configuration settings.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// This method sets up:
        /// - Database configuration in the DI container
        /// - Unit of work pattern implementation
        /// - Transaction management (if enabled)
        /// - Core database services required by the application
        /// 
        /// The configuration type must inherit from DatabaseConfiguration to ensure
        /// proper setup of basic database behaviors.
        /// </remarks>
        public static IServiceCollection AddDatabase<TDatabaseConfiguration>(this IServiceCollection services, TDatabaseConfiguration databaseConfiguration)
            where TDatabaseConfiguration : DatabaseConfiguration
        {
            GuardClause.ArgumentIsNotNull(databaseConfiguration, nameof(databaseConfiguration));
            services.TryAddSingleton<DatabaseConfiguration>(databaseConfiguration);
            services.AddSingleton(new UnitOfWorkOptions
            {
                UseTransactions = databaseConfiguration.UseTransactions,
            });
            services.AddScoped<UnitOfWorkFactory>();
            if (databaseConfiguration.UseTransactions)
            {
                services.AddTransactionDecorator();
            }

            return services;
        }

        /// <summary>
        /// Adds decortor to create transaction for command handlers.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection with the transaction pipeline behavior added.</returns>
        private static IServiceCollection AddTransactionDecorator(this IServiceCollection services)
        {
            services.TryAddScoped<ICommandTransactionManager, CommandTransactionManager>();
            if (services.All(x => x.ImplementationType != typeof(CommandTransactionDecorator<,>)))
                services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CommandTransactionDecorator<,>));

            return services;
        }
    }
}
