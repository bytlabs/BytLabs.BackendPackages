Pass if the response:
- keeps the existing middleware and endpoint registrations, moving them into the
  BuildWebApp delegate rather than discarding them
- moves existing DI into WithServiceConfiguration
- registers a tenant resolver, and mentions ValueTenantIdResolver as the option for a
  service not yet adopting multitenancy
- sequences the migration incrementally rather than rewriting the domain at once

Fail if it drops existing middleware, or if it requires converting the whole domain model
before the service will build.
