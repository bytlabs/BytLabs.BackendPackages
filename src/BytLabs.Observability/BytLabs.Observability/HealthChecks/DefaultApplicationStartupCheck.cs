using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BytLabs.Observability.HealthChecks;

/// <summary>
/// Provides a basic startup check implementation that always returns a started status.
/// </summary>
public class DefaultApplicationStartupCheck : IHealthCheck
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApplicationStartupCheck"/> class.
    /// </summary>
    public DefaultApplicationStartupCheck()
    {
    }

    /// <summary>
    /// Performs the startup check, returning a healthy result indicating the application has started.
    /// </summary>
    /// <param name="context">A context object associated with the current health check.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the health check.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
        CancellationToken cancellationToken = new CancellationToken()) =>
        Task.FromResult(HealthCheckResult.Healthy("Started"));
}
