using BytLabs.Multitenancy;
using BytLabs.Multitenancy.Resolvers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace BytLabs.Api.TenantProvider
{
    /// <summary>
    /// Resolves tenant IDs from HTTP headers in incoming requests.
    /// Implements the <see cref="ITenantIdResolver"/> interface for header-based tenant resolution.
    /// </summary>
    public class FromHeaderTenantIdResolver : ITenantIdResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FromHeaderTenantIdResolver> _logger;
        private readonly string _headerName;
        private const string Tenant = "Tenant";

        /// <summary>
        /// Initializes a new instance of the <see cref="FromHeaderTenantIdResolver"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor for accessing the current request.</param>
        /// <param name="logger">The logger instance for diagnostic logging.</param>
        /// <param name="headerName">Optional custom header name. If not specified, defaults to "Tenant".</param>
        /// <exception cref="ArgumentNullException">Thrown when httpContextAccessor or logger is null.</exception>
        public FromHeaderTenantIdResolver(
            IHttpContextAccessor httpContextAccessor,
            ILogger<FromHeaderTenantIdResolver> logger,
            string? headerName = null)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _headerName = headerName ?? Tenant;
        }

        /// <summary>
        /// Attempts to resolve the current tenant ID from the HTTP request headers.
        /// </summary>
        /// <param name="tenantId">When this method returns, contains the resolved tenant ID if successful; otherwise, null.</param>
        /// <returns>
        /// true if the tenant ID was successfully resolved from the headers; otherwise, false.
        /// Returns false in cases such as missing context, WebSocket requests, or invalid header values.
        /// </returns>
        public bool TryGetCurrent([NotNullWhen(true)] out TenantId? tenantId)
        {
            tenantId = null;
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext == null)
                {
                    _logger.LogTrace("Request http context is null");
                    return false;
                }

                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    _logger.LogTrace("Cannot determine tenant for web socket request");
                    return false;
                }

                if (httpContext.Request == null)
                {
                    _logger.LogTrace("Http request is null");
                    return false;
                }

                if (httpContext.Request.Headers == null)
                {
                    _logger.LogTrace("Http headers are null");
                    return false;
                }

                var headerExists = httpContext.Request.Headers.TryGetValue(_headerName, out StringValues header);
                if (!headerExists)
                {
                    _logger.LogTrace("Failed to fetch {HeaderName} header from the http request", _headerName);
                    return false;
                }

                if (header == StringValues.Empty)
                {
                    _logger.LogTrace("{HeaderName} header is empty", _headerName);
                    return false;
                }

                string tenantIdString = GetTenantIdFromStringValues(header);
                tenantId = new TenantId(tenantIdString);
                _logger.LogTrace("Tenant '{Tenant}' extracted from the http header", tenantId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "Tenant wasn't identified");
                return false;
            }
        }

        /// <summary>
        /// Extracts the tenant ID from the header string values.
        /// </summary>
        /// <param name="values">The header string values to process.</param>
        /// <returns>The first non-empty tenant ID value from the header.</returns>
        private static string GetTenantIdFromStringValues(StringValues values) => 
            values.ToString().Split(",").First().Trim();
    }
}
