using Microsoft.EntityFrameworkCore;

namespace BytLabs.DataAccess.EntityFramework;

/// <summary>
/// Query helpers for Entity Framework aggregate loading.
/// </summary>
public static class EfExtensions
{
    /// <summary>
    /// Eager-loads all reference/collection navigations of <typeparamref name="T"/> so that an
    /// aggregate root is materialized together with its owned/child entities.
    /// </summary>
    public static IQueryable<T> IncludeAggregateEntities<T>(this IQueryable<T> query, DbContext context)
        where T : class
    {
        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType is null)
            return query;

        foreach (var nav in entityType.GetNavigations())
        {
            // Skip owned navigations (e.g. AuditInfo): EF always loads owned types and rejects
            // an explicit Include() on them.
            if (nav.TargetEntityType.ClrType.IsClass && !nav.TargetEntityType.IsOwned())
            {
                query = query.Include(nav.Name);
            }
        }

        return query;
    }
}
