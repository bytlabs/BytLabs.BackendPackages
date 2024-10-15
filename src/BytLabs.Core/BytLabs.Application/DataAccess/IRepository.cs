using BytLabs.Domain.Entities;
using System.Linq.Expressions;

namespace BytLabs.Application.DataAccess;

/// <summary>
/// Represents a generic repository interface for data persistence operations.
/// Provides CRUD operations and batch processing capabilities for aggregate roots.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of aggregate root entity</typeparam>
/// <typeparam name="TIdentity">The type of the entity's identifier</typeparam>
public interface IRepository<TAggregateRoot, TIdentity> where TAggregateRoot : IAggregateRoot<TIdentity>
{
    /// <summary>
    /// Retrieves an entity by its identifier. Throws an exception if the entity is not found.
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The requested entity</returns>
    /// <exception cref="BytLabs.Application.Exceptions.EntityNotFoundException">Thrown when the entity does not exist</exception>
    Task<TAggregateRoot> GetByIdAsync(TIdentity id, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to find an entity by its identifier. Returns null if the entity is not found.
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The found entity or null if not found</returns>
    Task<TAggregateRoot> FindByIdAsync(TIdentity id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single entity from the repository. Returns null if no entity exists.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The single entity or null if none exists</returns>
    /// <exception cref="InvalidOperationException">Thrown when multiple entities are found</exception>
    Task<TAggregateRoot> SingleOrDefaultAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The created entity with any generated values (e.g., ID)</returns>
    Task<TAggregateRoot> InsertAsync(TAggregateRoot entity, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated values</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The updated entity</returns>
    Task<TAggregateRoot> UpdateAsync(TAggregateRoot entity, CancellationToken cancellationToken);

    /// <summary>
    /// Performs a soft delete on an entity by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    Task DeleteAsync(TIdentity id, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves multiple entities by their identifiers.
    /// </summary>
    /// <param name="ids">List of unique identifiers</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>List of found entities. May be empty if no entities are found.</returns>
    Task<List<TAggregateRoot>> FindAllAsync(List<TIdentity> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves entities based on a filter expression.
    /// </summary>
    /// <param name="filterExpression">Lambda expression defining the filter criteria</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of entities matching the filter criteria</returns>
    Task<IEnumerable<TAggregateRoot>> FindAllByAsync(Expression<Func<TAggregateRoot, bool>> filterExpression,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates multiple entities in a single operation.
    /// </summary>
    /// <param name="aggregates">List of entities to update</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    Task UpdateBatchAsync(List<TAggregateRoot> aggregates, CancellationToken cancellationToken);

    /// <summary>
    /// Creates multiple entities in a single operation.
    /// </summary>
    /// <param name="aggregates">List of entities to create</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    Task InsertBatchAsync(List<TAggregateRoot> aggregates, CancellationToken cancellationToken);

    /// <summary>
    /// Performs a soft delete on multiple entities by their identifiers.
    /// </summary>
    /// <param name="ids">List of unique identifiers for entities to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    Task DeleteBatchAsync(List<TIdentity> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Performs a soft delete on multiple entities.
    /// </summary>
    /// <param name="agregates">List of entities to delete</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    Task DeleteBatchAsync(List<TAggregateRoot> agregates, CancellationToken cancellationToken);
}
