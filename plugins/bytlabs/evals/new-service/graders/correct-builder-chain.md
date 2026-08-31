Pass if the response:
- uses ApiServiceBuilder.CreateBuilder(builder) and calls the steps in this exact order:
  WithHttpContextAccessor, WithMultiTenantContext, WithLogging, WithMetrics, WithTracing,
  WithHealthChecks, WithServiceConfiguration, BuildWebApp
- registers a tenant resolver inside WithMultiTenantContext
- calls AddCQS with the assembly containing the handlers
- puts middleware and endpoint mapping inside the BuildWebApp delegate

Fail if the chain order differs, if AddCQS is missing, or if it calls AddLogging/AddMetrics/
AddTracing directly in addition to the chain steps.
