using System.Reflection;
using BytLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BytLabs.DataAccess.EntityFramework;

/// <summary>
/// <see cref="ModelBuilder"/> helpers for wiring BytLabs aggregates into an EF model.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Scans the given assemblies for concrete aggregate roots (types implementing
    /// <see cref="IAggregateRoot{TId}"/>) and unmaps their <c>DomainEvents</c> property, so it is
    /// never persisted to a column. This is the EF equivalent of the MongoDB class-map unmapping.
    /// </summary>
    /// <param name="modelBuilder">The model builder (typically from <c>OnModelCreating</c>).</param>
    /// <param name="assemblies">Assemblies to scan for aggregate roots. At least one is required.</param>
    /// <exception cref="ArgumentException">Thrown when no assemblies are supplied.</exception>
    public static ModelBuilder IgnoreDomainEvents(this ModelBuilder modelBuilder, params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
            throw new ArgumentException("At least one assembly must be supplied.", nameof(assemblies));

        foreach (var assembly in assemblies)
        {
            var aggregateRootTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && ImplementsAggregateRoot(t));

            foreach (var type in aggregateRootTypes)
            {
                modelBuilder.Entity(type).Ignore(nameof(IAggregateRoot<object>.DomainEvents));
            }
        }

        return modelBuilder;
    }

    private static bool ImplementsAggregateRoot(Type type) =>
        type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAggregateRoot<>));
}
