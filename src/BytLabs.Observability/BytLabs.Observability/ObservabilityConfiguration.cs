using System.ComponentModel.DataAnnotations;

namespace BytLabs.Observability
{
    /// <summary>
    /// Configuration settings for observability features including logging, metrics, and tracing.
    /// </summary>
    public class ObservabilityConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the service being monitored.
        /// Used to identify the service in logs, metrics, and traces.
        /// </summary>
        [Required]
        public string ServiceName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the URL of the OpenTelemetry collector.
        /// This is where telemetry data will be sent.
        /// </summary>
        public string? CollectorUrl { get; set; } = null!;

        /// <summary>
        /// Gets the URI of the OpenTelemetry collector, constructed from the CollectorUrl.
        /// </summary>
        public Uri? CollectorUri => CollectorUrl is null? null : new(CollectorUrl);

        /// <summary>
        /// Gets or sets the timeout in milliseconds for telemetry operations.
        /// Default value is 1000 milliseconds (1 second).
        /// </summary>
        public int Timeout { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the logging configuration settings.
        /// </summary>
        public Logs Logs { get; set; } = new Logs();
    }

    /// <summary>
    /// Configuration settings specific to logging functionality.
    /// </summary>
    public class Logs
    {
        /// <summary>
        /// Gets or sets whether OpenTelemetry logging is enabled.
        /// When true, logs will be sent to the OpenTelemetry collector.
        /// Default is false.
        /// </summary>
        public bool OpenTelemetryEnabled { get; set; } = false;
    }
}