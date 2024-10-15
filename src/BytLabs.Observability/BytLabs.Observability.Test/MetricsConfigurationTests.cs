using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace BytLabs.Observability.Test;

public class MetricsConfigurationTests
{
    [Fact]
    public void GIVEN_AddMetrics_WHEN_Configured_THEN_ShouldAddOpenTelemetryMetrics()
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
        builder.Services.AddMetrics(observabilityConfig);
        WebApplication app = builder.Build();
        MeterProvider? meterProvider = app.Services.GetService<MeterProvider>();

        // Assert
        Assert.NotNull(meterProvider);

    }
}