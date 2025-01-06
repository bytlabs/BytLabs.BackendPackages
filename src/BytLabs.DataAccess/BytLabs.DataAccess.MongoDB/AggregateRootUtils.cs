using BytLabs.Domain.Entities;

namespace BytLabs.DataAccess.MongoDB;

public static class AggregateRootUtils
{
    public static bool IsInheritingFromAggregateRootBase<TEntity, TIdentity>()
    {
        return typeof(TEntity).IsAssignableTo(typeof(AggregateRootBase<TIdentity>));
    }

    public static bool IsImplementingIAggregateRoot<TEntity, TIdentity>()
    {
        return typeof(TEntity).GetMember(nameof(IAggregateRoot<TIdentity>.DomainEvents)).First().DeclaringType == typeof(TEntity);
    }
}
