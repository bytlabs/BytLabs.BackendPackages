using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenTelemetry;

namespace BytLabs.Observability
{
    /// <summary>
    /// Provides extension methods for configuring logging in the application.
    /// </summary>
    public static class LoggingRegistration
    {
        /// <summary>
        /// Adds and configures Serilog logging to the application.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <param name="observabilityConfiguration">Configuration for observability features.</param>
        /// <returns>The web application builder with logging configured.</returns>
        public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder,
            ObservabilityConfiguration observabilityConfiguration)
        {
            builder.Host
                .UseSerilog((context, provider, options) =>
                {
                    options
                        .ReadFrom.Configuration(builder.Configuration)
                        .Enrich.WithEnvironmentName()
                        .Enrich.WithSpan()
                        .Enrich.WithProperty("ApplicationName", observabilityConfiguration.ServiceName)
                        .WriteTo.Console();

                    if (observabilityConfiguration.Logs.OpenTelemetryEnabled)
                    {
                        SendLogsToOpenTelemetryCollector(observabilityConfiguration, options);
                    }
                });
            return builder;
        }

        /// <summary>
        /// Configures the OpenTelemetry log exporter.
        /// </summary>
        /// <param name="observabilityConfiguration">Configuration for observability features.</param>
        /// <param name="options">The logger configuration to modify.</param>
        private static void SendLogsToOpenTelemetryCollector(
            ObservabilityConfiguration observabilityConfiguration, 
            LoggerConfiguration options)
        {
            options.WriteTo.OpenTelemetry(cfg =>
            {
                cfg.Endpoint = $"{observabilityConfiguration.CollectorUrl}/v1/logs";
                cfg.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField;
                cfg.ResourceAttributes = new Dictionary<string, object> {
                                                                            {
                                                                                "service.name", observabilityConfiguration.ServiceName
                                                                            }
                                                                        };
            });
        }
    }
}