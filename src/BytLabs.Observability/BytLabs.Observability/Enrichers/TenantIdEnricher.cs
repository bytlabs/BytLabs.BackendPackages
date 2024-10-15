using BytLabs.Multitenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;

namespace BytLabs.Observability.Enrichers;

/// <summary>
/// Provides extension methods for enriching logs with tenant ID information.
/// </summary>
public static class TenantIdEnricher
{
    private const string TenantIdPropertyName = "TenantId";

    /// <summary>
    /// Enriches the logger context with the tenant ID from the current request.
    /// </summary>
    /// <param name="app">The web application to add the enricher to.</param>
    /// <returns>The web application with the tenant ID enricher configured.</returns>
    public static void EnrichLoggerWithTenantId(this WebApplication app) =>
        app.Use(async (context, next) =>
        {
            var provider = context.RequestServices.GetService<ITenantIdProvider>();
            var tenantId = provider?.GetTenantId().Value;
            using (LogContext.PushProperty(TenantIdPropertyName, tenantId))
            {
                await next.Invoke();
            }
        });
}