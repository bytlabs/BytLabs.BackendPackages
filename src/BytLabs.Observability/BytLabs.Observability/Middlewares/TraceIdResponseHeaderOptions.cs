namespace BytLabs.Observability.Middlewares;

/// <summary>
/// Configuration options for the TraceIdResponseHeaderMiddleware.
/// </summary>
public class TraceIdResponseHeaderOptions
{
    /// <summary>
    /// Gets or sets the name of the header that will contain the trace ID.
    /// Default value is "TraceId".
    /// </summary>
    public string HeaderName { get; set; } = "TraceId";
}
