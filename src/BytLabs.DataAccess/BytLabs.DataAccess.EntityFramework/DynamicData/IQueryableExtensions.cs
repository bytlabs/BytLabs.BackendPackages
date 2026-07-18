using System.Linq.Expressions;
using System.Text.Json;
using BytLabs.Application.DynamicData;
using BytLabs.Domain.DynamicData;

namespace BytLabs.DataAccess.EntityFramework.DynamicData;

/// <summary>
/// Dynamic-data (jsonb) <b>sorting</b> for the EF/PostgreSQL query path. Npgsql translates
/// <c>e.Data.GetProperty("x").GetString()</c> to <c>data-&gt;&gt;'x'</c>, so ordering pushes down to SQL.
/// (Filtering lives in <c>QueryableDynamicDataFilterFieldHandler</c>.)
/// </summary>
public static class IQueryableExtensions
{
    private static readonly System.Reflection.MethodInfo GetPropertyMethod =
        typeof(JsonElement).GetMethod(nameof(JsonElement.GetProperty), new[] { typeof(string) })!;

    /// <summary>
    /// Applies <see cref="SortInput{T}"/> ordering. A <c>Path</c> of a CLR property sorts that column;
    /// a <c>data.&lt;key&gt;</c> path (or an unmatched name) sorts by the jsonb sub-value. Mirrors the
    /// MongoDB <c>AppySortingWithDynamicData</c>.
    /// </summary>
    public static IQueryable<T> AppySortingWithDynamicData<T>(this IQueryable<T> source, List<SortInput<T>>? order)
        where T : class, IHaveDynamicData
    {
        if (order is null || order.Count == 0) return source;

        var q = source;
        var first = true;
        foreach (var sort in order)
        {
            if (string.IsNullOrWhiteSpace(sort.Path)) continue;
            var param = Expression.Parameter(typeof(T), "e");
            var key = BuildSortKey<T>(sort.Path, param);
            var lambda = Expression.Lambda(key, param);
            var ascending = sort.By == SortOrder.Asc;
            var method = first
                ? ascending ? "OrderBy" : "OrderByDescending"
                : ascending ? "ThenBy" : "ThenByDescending";
            q = q.Provider.CreateQuery<T>(Expression.Call(
                typeof(Queryable), method, new[] { typeof(T), key.Type }, q.Expression, Expression.Quote(lambda)));
            first = false;
        }
        return q;
    }

    private static Expression BuildSortKey<T>(string path, ParameterExpression param) where T : IHaveDynamicData
    {
        // Explicit data path.
        if (path.StartsWith("data.", StringComparison.OrdinalIgnoreCase))
            return JsonPathText(param, path[5..].Split('.', StringSplitOptions.RemoveEmptyEntries));

        // Matching CLR property (case-insensitive) sorts the column.
        var prop = typeof(T).GetProperty(path,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
        if (prop is not null) return Expression.Property(param, prop);

        // Otherwise treat as a top-level dynamic-data key.
        return JsonPathText(param, new[] { path });
    }

    private static Expression JsonPathText(ParameterExpression param, string[] segments)
    {
        Expression element = Expression.Property(param, nameof(IHaveDynamicData.Data));
        foreach (var segment in segments)
            element = Expression.Call(element, GetPropertyMethod, Expression.Constant(segment));
        return Expression.Call(element, nameof(JsonElement.GetString), Type.EmptyTypes);
    }
}
