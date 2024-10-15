using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace BytLabs.Observability.HealthChecks;

public static class ServiceExtensions
{
    public const string ReadyCheckName = "ready";
    public const string HealthCheckName = "health";
    public const string StartupCheckName = "startup";

    private const string HealthCheckEndpoint = "/healthz";
    private const string ReadyCheckEndpoint = "/readyz";
    private const string StartupCheckEndpoint = "/startupz";

    /// <summary>
    /// Provides extension methods to easily set up health checks and monitoring for the application.
    /// </summary>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, Action<IHealthChecksBuilder>? healthCheckBuilder)
    {
        IHealthChecksBuilder hcBuilder = HealthCheckServiceCollectionExtensions.AddHealthChecks(services).AddApplicationStatus("api_status", tags: new[]
                                                                                                             {
                                                                                                                 "health",
                                                                                                                 "api"
                                                                                                             });

        healthCheckBuilder?.Invoke(hcBuilder);
        services.AddHealthChecks()
            .AddCheck<DefaultApplicationStartupCheck>(StartupCheckName, tags: new List<string>
                                                                       {
                                                                           StartupCheckName
                                                                       })
            .AddCheck<DefaultApplicationReadyCheck>(ReadyCheckName, tags: new List<string>
                                                                   {
                                                                       ReadyCheckName
                                                                   })
            .AddCheck<DefaultApplicationHealthCheck>(HealthCheckName, tags: new List<string>
                                                                     {
                                                                         HealthCheckName
                                                                     });

        return services;
    }

    /// <summary>
    /// Registers a new readiness health check for a specific service.
    /// This check should verify that the application is ready to handle traffic,
    /// ensuring all necessary services, databases, or external systems are operational.
    /// </summary>
    /// <param name="healthChecksBuilder">The health checks builder to which the readiness check will be added.</param>
    /// <param name="uniqueName">A unique name for the health check.</param>
    /// <typeparam name="TCheck">The type of health check to add, must implement IHealthCheck.</typeparam>
    /// <returns>The updated health checks builder with the new readiness check added.</returns>
    public static IHealthChecksBuilder AddCheckForReady<TCheck>(this IHealthChecksBuilder healthChecksBuilder, string uniqueName) where TCheck : class, IHealthCheck
    {
        healthChecksBuilder.AddCheck<TCheck>(uniqueName, tags: new[] { ReadyCheckName });
        return healthChecksBuilder;
    }
    /// <summary>
    /// Registers a new startup health check for a specific component of the application.
    /// This check should confirm that startup tasks, such as initial data loads or
    /// configuration validations, have been completed successfully.
    /// </summary>
    /// <param name="healthChecksBuilder">The health checks builder to which the startup check will be added.</param>
    /// <param name="uniqueName">A unique name for the health check.</param>
    /// <typeparam name="TCheck">The type of health check to add, must implement IHealthCheck.</typeparam>
    /// <returns>The updated health checks builder with the new startup check added.</returns>
    public static IHealthChecksBuilder AddCheckForStartup<TCheck>(this IHealthChecksBuilder healthChecksBuilder, string uniqueName) where TCheck : class, IHealthCheck
    {
        healthChecksBuilder.AddCheck<TCheck>(uniqueName, tags: new[] { StartupCheckName });
        return healthChecksBuilder;
    }
    /// <summary>
    /// Registers a new general application health check.
    /// This type of check is broader and assesses the overall health status of the application,
    /// often used for liveness probes to determine if the application is running correctly.
    /// </summary>
    /// <param name="healthChecksBuilder">The health checks builder to which the general health check will be added.</param>
    /// <param name="uniqueName">A unique name for the health check.</param>
    /// <typeparam name="TCheck">The type of health check to add, must implement IHealthCheck.</typeparam>
    /// <returns>The updated health checks builder with the new general health check added.</returns>
    public static IHealthChecksBuilder AddCheckForAppHealth<TCheck>(this IHealthChecksBuilder healthChecksBuilder, string uniqueName) where TCheck : class, IHealthCheck
    {
        healthChecksBuilder.AddCheck<TCheck>(uniqueName, tags: new[] { HealthCheckName });
        return healthChecksBuilder;
    }

    public static IApplicationBuilder UseHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks(HealthCheckEndpoint,
                            new HealthCheckOptions
                            {
                                Predicate = registration => registration.Tags.Contains(HealthCheckName)
                            });
        app.UseHealthChecks(ReadyCheckEndpoint,
                            new HealthCheckOptions
                            {
                                Predicate = registration => registration.Tags.Contains(ReadyCheckName)
                            });
        app.UseHealthChecks(StartupCheckEndpoint,
                            new HealthCheckOptions
                            {
                                Predicate = registration => registration.Tags.Contains(StartupCheckName)
                            });
        return app;
    }
}