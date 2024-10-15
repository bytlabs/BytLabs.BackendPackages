using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace BytLabs.Observability.Middlewares;

/// <summary>
/// Middleware that adds the current trace ID to the response headers.
/// </summary>
public class TraceIdResponseHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TraceIdResponseHeaderOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraceIdResponseHeaderMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">Configuration options for the middleware.</param>
    public TraceIdResponseHeaderMiddleware(RequestDelegate next, IOptions<TraceIdResponseHeaderOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Processes an HTTP request by adding the trace ID to response headers.
    /// </summary>
    /// <param name="context">The context for the current HTTP request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [DebuggerStepThrough]
    public async Task Invoke(HttpContext context)
    {
        string traceId = Activity.Current!.TraceId.ToHexString();
        using (LogContext.PushProperty("TraceId", traceId))
        {
            if (!context.Response.Headers.ContainsKey(_options.HeaderName))
            {
                context.Response.Headers.Append(_options.HeaderName, traceId);
            }

            await _next(context);
        }
    }
}

/// <summary>
/// Extension methods for registering the TraceIdResponseHeaderMiddleware.
/// </summary>
public static class TraceIdResponseHeaderMiddlewareRegistration
{
    /// <summary>
    /// Adds the TraceIdResponseHeaderMiddleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Optional action to configure the middleware options.</param>
    /// <returns>The application builder with the middleware configured.</returns>
    public static IApplicationBuilder UseTraceIdResponseHeader(this IApplicationBuilder builder, 
        Action<TraceIdResponseHeaderOptions>? configure = null)
    {
        var options = new TraceIdResponseHeaderOptions();
        configure?.Invoke(options);

        builder.UseMiddleware<TraceIdResponseHeaderMiddleware>(Options.Create(options));

        return builder;
    }
}