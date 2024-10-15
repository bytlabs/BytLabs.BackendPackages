using BytLabs.Application.UserContext.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BytLabs.Application.UserContext;

/// <summary>
/// Provides a fluent interface for configuring user context resolution.
/// Manages the registration of resolvers in the dependency injection container.
/// </summary>
public sealed class UserContextBuilder
{
    /// <summary>
    /// Initializes a new instance of the UserContextBuilder.
    /// </summary>
    /// <param name="services">Service collection for dependency registration</param>
    public UserContextBuilder(IServiceCollection services)
    {
        Services = services;
        Services.TryAddSingleton<IUserContextProvider, UserContextProvider>();
    }

    private IServiceCollection Services { get; }

   

    public UserContextBuilder AddResolver(IUserContextResolver resolver)
    {
        Services.TryAddSingleton(resolver);
        return this;
    }

    /// <summary>
    /// Registers a resolver in the service collection.
    /// </summary>
    /// <param name="userContextResolver">The resolver to register</param>
    /// <returns>The builder instance for method chaining</returns>
    public UserContextBuilder AddResolver<T>() where T : class, IUserContextResolver
    {
        Services.TryAddSingleton<IUserContextResolver, T>();
        return this;
    }

    public UserContextBuilder AddResolver<T>(Func<IServiceProvider, T> factory)
        where T : class, IUserContextResolver
    {
        Services.TryAddSingleton<IUserContextResolver>(factory);
        return this;
    }
}