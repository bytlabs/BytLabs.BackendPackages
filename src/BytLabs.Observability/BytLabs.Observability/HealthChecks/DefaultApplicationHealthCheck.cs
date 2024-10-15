using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BytLabs.Observability.HealthChecks
{
    /// <summary>
    /// Provides a basic health check implementation that always returns a healthy status.
    /// </summary>
    public class DefaultApplicationHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Performs the health check, returning a healthy result.
        /// </summary>
        /// <param name="context">A context object associated with the current health check.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the health check.</param>
        /// <returns>A task that represents the asynchronous health check operation.</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken()) =>
            Task.FromResult(HealthCheckResult.Healthy("OK"));
    }
}