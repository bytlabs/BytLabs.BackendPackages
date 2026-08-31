Pass if the response:
- implements ITenantIdResolver with bool TryGetCurrent(out TenantId?)
- registers it through MultitenancyBuilder inside WithMultiTenantContext
- notes that resolvers are tried in registration order and the first success wins
- mentions FailedToResolveTenantIdException as the outcome when none resolves

Fail if it implements ITenantIdProvider instead of ITenantIdResolver, or if it passes the
tenant id explicitly into commands or repositories afterwards.
