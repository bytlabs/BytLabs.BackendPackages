using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BytLabs.Application.CQS.Commands;
using BytLabs.Application.CQS.Decorators;
using BytLabs.Application.CQS.Queries;
using BytLabs.Application.Mapping;
using BytLabs.Application.UserContext;
using FluentValidation;

using MediatR;
using MediatR.Pipeline;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BytLabs.Application
{
    /// <summary>
    /// Provides extension methods for IServiceCollection to configure application services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        private static readonly HashSet<string> RegisteredAssemblies = new();

        /// <summary>
        /// Configures CQRS infrastructure with MediatR, AutoMapper, and validation pipeline.
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        /// <param name="assemblies">Assemblies to scan for handlers and validators</param>
        /// <param name="options">Optional MediatR configuration options</param>
        /// <returns>The configured service collection</returns>
        public static IServiceCollection AddCQS(this IServiceCollection services, Assembly[] assemblies, Action<MediatRServiceConfiguration>? options = null)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.TryAddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.TryAddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));

            var assemblyList = assemblies.ToList();
            assemblyList.Add(typeof(ServiceCollectionExtensions).Assembly);

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblies(assemblyList.ToArray());
                options?.Invoke(configuration);
            });

            services
                .AddLoggingDecorator()
                .AddCommandsValidationDecorator(assemblies)
                .AddQueriesValidationDecorator(assemblies);

            return services;
        }

        /// <summary>
        /// Configures user context resolution services.
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        /// <returns>A builder for further user context configuration</returns>
        /// <remarks>
        /// Enables user identification through multiple sources by registering necessary providers
        /// and accessors in the dependency injection container.
        /// </remarks>
        public static UserContextBuilder AddUserContextProviders(this IServiceCollection services)
        {
            return new UserContextBuilder(services);
        }

        /// <summary>
        /// Adds command validation pipeline using FluentValidation.
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        /// <param name="assemblies">Assemblies to scan for validators</param>
        private static IServiceCollection AddCommandsValidationDecorator(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (!services.Any(x => x.ImplementationType == typeof(CommandValidationDecorator<,>)))
            {
                services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CommandValidationDecorator<,>));
                services.AddValidatorsFromAssemblies(assemblies);
            }

            return services;
        }

        /// <summary>
        /// Adds query validation pipeline using FluentValidation.
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        /// <param name="validatorsAssemblies">Assemblies to scan for validators</param>
        private static IServiceCollection AddQueriesValidationDecorator(this IServiceCollection services, params Assembly[] validatorsAssemblies)
        {
            if (!services.Any(x => x.ImplementationType == typeof(QueryValidationDecorator<,>)))
            {
                services.AddScoped(typeof(IPipelineBehavior<,>), typeof(QueryValidationDecorator<,>));
                services.AddValidatorsFromAssemblies(validatorsAssemblies);
            }

            return services;
        }

        /// <summary>
        /// Adds request/response logging pipeline.
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        private static IServiceCollection AddLoggingDecorator(this IServiceCollection services)
        {
            if (!services.Any(x => x.ImplementationType == typeof(LoggingDecorator<,>)))
                services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingDecorator<,>));

            return services;
        }
    }
}
