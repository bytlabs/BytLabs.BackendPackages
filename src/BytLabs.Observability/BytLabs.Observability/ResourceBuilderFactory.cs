using OpenTelemetry.Resources;

namespace BytLabs.Observability
{
    /// <summary>
    /// Factory class for creating OpenTelemetry resource builders with standard configuration.
    /// </summary>
    internal static class ResourceBuilderFactory
    {
        /// <summary>
        /// Creates a configured ResourceBuilder with service and environment information.
        /// </summary>
        /// <param name="observabilityConfiguration">Configuration settings for observability features.</param>
        /// <returns>A configured ResourceBuilder instance with service name and environment variables.</returns>
        /// <remarks>
        /// The created ResourceBuilder includes:
        /// - Service name from configuration
        /// - Environment variables detected automatically
        /// - Standard OpenTelemetry resource attributes
        /// </remarks>
        internal static ResourceBuilder Create(ObservabilityConfiguration observabilityConfiguration)
        {
            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
                .AddEnvironmentVariableDetector()
                .AddService(observabilityConfiguration.ServiceName);
            return resourceBuilder;
        }
    }
}