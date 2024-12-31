using BytLabs.Multitenancy;
using BytLabs.Multitenancy.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;

namespace BytLabs.Observability.Middlewares;

/// <summary>
/// Provides extension methods for enriching logs with tenant ID information.
/// </summary>
public static class TenantIdEnricherMiddleware
{
    private const string TenantIdPropertyName = "TenantId";

    /// <summary>
    /// Enriches the logger context with the tenant ID from the current request.
    /// </summary>
    /// <param name="app">The web application to add the enricher to.</param>
    /// <returns>The web application with the tenant ID enricher configured.</returns>
    public static IApplicationBuilder UseLoggerWithTenantId(this IApplicationBuilder app) =>
         app.Use(async (context, next) =>
         {
             try
             {
                 var provider = context.RequestServices.GetService<ITenantIdProvider>();
                 var tenantId = provider?.GetTenantId().Value;
                 using (LogContext.PushProperty(TenantIdPropertyName, tenantId))
                 {
                     await next.Invoke();
                 }
             }
             catch (FailedToResolveTenantIdException)
             {
                 await next.Invoke();
             }
         });
}