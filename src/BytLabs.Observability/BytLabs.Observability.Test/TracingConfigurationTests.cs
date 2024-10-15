using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BytLabs.Observability.Test;

public class TracingConfigurationTests
{
    [Fact]
    public void GIVEN_AddTracing_WHEN_Configured_THEN_ShouldProvideOpenTelemetryTracing()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        ObservabilityConfiguration observabilityConfig = new ObservabilityConfiguration
        {
            ServiceName = "TestService",
            CollectorUrl = "http://localhost:4317",
            Timeout = 1000
        };

        // Act
        builder.Services.AddTracing(observabilityConfig);
        WebApplication app = builder.Build();
        TracerProvider? tracerProvider = app.Services.GetService<TracerProvider>();

        // Assert
        Assert.NotNull(tracerProvider);

    }
}