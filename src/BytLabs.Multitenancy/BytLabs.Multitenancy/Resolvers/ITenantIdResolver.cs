using System.Diagnostics.CodeAnalysis;

namespace BytLabs.Multitenancy.Resolvers
{
    public interface ITenantIdResolver
    {
        public bool TryGetCurrent([NotNullWhen(true)] out TenantId? tenantId);
    }
}
