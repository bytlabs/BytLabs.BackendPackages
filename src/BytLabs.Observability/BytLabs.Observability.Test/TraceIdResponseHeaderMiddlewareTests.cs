using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using BytLabs.Observability.Middlewares;

namespace BytLabs.Observability.Test;

public class TraceIdResponseHeaderMiddlewareTests
{
    [Fact]
    public async Task GIVEN_TraceIdResponseHeaderMiddleware_WHEN_ConfiguredWithCustomHeader_THEN_ShouldAddCustomHeader()
    {
        // Arrange
        const string customHeaderName = "X-BytLabs-TracedId";

        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        WebApplication app = builder.Build();

        app.UseTraceIdResponseHeader(options =>
        {
            options.HeaderName = customHeaderName;
        });

        app.Map("/", context => context.Response.WriteAsync("Hello World!"));

        await app.StartAsync();

        HttpClient client = app.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.Contains(customHeaderName), $"Response should contain header '{customHeaderName}'");

        string? traceId = response.Headers.GetValues(customHeaderName).FirstOrDefault();
        Assert.False(string.IsNullOrEmpty(traceId), "TraceId header should have a value");
    }

    [Fact]
    public async Task GIVEN_TraceIdResponseHeaderMiddleware_WHEN_Configured_THEN_ShouldAddDefaultHeader()
    {
        // Arrange
        const string headerName = "TraceId";

        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        WebApplication app = builder.Build();

        app.UseTraceIdResponseHeader();

        app.Map("/", context => context.Response.WriteAsync("Hello World!"));

        await app.StartAsync();

        HttpClient client = app.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.Contains(headerName), $"Response should contain header '{headerName}'");

        string? traceId = response.Headers.GetValues(headerName).FirstOrDefault();
        Assert.False(string.IsNullOrEmpty(traceId), "TraceId header should have a value");
    }
}