using System.Linq.Expressions;
using BytLabs.Application.DataAccess;
using BytLabs.Application.Exceptions;
using BytLabs.Application.UserContext;
using BytLabs.Domain.Entities;
using MongoDB.Driver;
using SharpCompress.Common;

namespace BytLabs.DataAccess.MongoDB;

/// <summary>
/// MongoDB implementation of the generic repository pattern for aggregate roots.
/// Provides CRUD operations and batch processing capabilities.
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type</typeparam>
/// <typeparam name="TIdentity">The type of the entity's identifier</typeparam>
internal sealed class MongoRepository<TEntity, TIdentity>(
    IMongoCollection<TEntity> collection,
    IUserContextProvider userContextProvider,
    MongoUnitOfWork unitOfWork) : IRepository<TEntity, TIdentity> where TEntity : IAggregateRoot<TIdentity>
{
    /// <inheritdoc />
    public async Task<TEntity> FindByIdAsync(TIdentity id, CancellationToken cancellationToken)
    {
        var query = unitOfWork.Session != null
            ? collection.Find(unitOfWork.Session, x => x.Id!.Equals(id))
            : collection.Find(x => x.Id!.Equals(id));
        TEntity? entity = await query.FirstOrDefaultAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc />

    public async Task<IEnumerable<TEntity>> FindAllByAsync(Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken)
    {
        using IAsyncCursor<TEntity>? result = unitOfWork.Session != null
            ? await collection.FindAsync(unitOfWork.Session, filterExpression, null, cancellationToken)
            : await collection.FindAsync(filterExpression, null, cancellationToken);
        return await result.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> SingleOrDefaultAsync(CancellationToken cancellationToken)
    {
        var emptyFilter = Builders<TEntity>.Filter.Empty;
        var count = unitOfWork.Session != null
            ? await collection.CountDocumentsAsync(unitOfWork.Session, emptyFilter, cancellationToken: cancellationToken)
            : await collection.CountDocumentsAsync(emptyFilter, cancellationToken: cancellationToken);
        if (count > 1)
        {
            throw new InvalidOperationException("More than one document found.");
        }

        var findOptions = new FindOptions<TEntity> { Limit = 1 };
        using IAsyncCursor<TEntity> result = unitOfWork.Session != null
            ? await collection.FindAsync(unitOfWork.Session, emptyFilter, findOptions, cancellationToken)
            : await collection.FindAsync(emptyFilter, findOptions, cancellationToken);
        return await result.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> GetByIdAsync(TIdentity id, CancellationToken cancellationToken)
    {
        TEntity? entity = await FindByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new EntityNotFoundException($"Entity with id={id} not found.");

        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken)
    {
        entity.AuditInfo.CreatedAt = DateTime.UtcNow;
        entity.AuditInfo.CreatedBy = userContextProvider.GetUserId();

        if (unitOfWork.Session != null)
        {
            await collection.InsertOneAsync(unitOfWork.Session, entity, null, cancellationToken);
        }
        else
        {
            await collection.InsertOneAsync(entity, null, cancellationToken);
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        FilterDefinition<TEntity>? filter = Builders<TEntity>.Filter.Eq("_id", entity.Id);

        entity.AuditInfo.LastModifiedAt = DateTime.UtcNow;
        entity.AuditInfo.LastModifiedBy = userContextProvider.GetUserId();

        if (unitOfWork.Session != null)
        {
            await collection.ReplaceOneAsync(unitOfWork.Session, filter, entity, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {

        FilterDefinition<TEntity>? filter = Builders<TEntity>.Filter.Eq("_id", entity.Id);

        if (unitOfWork.Session != null)
        {
            await collection.FindOneAndDeleteAsync(unitOfWork.Session, filter, cancellationToken: cancellationToken);
        }
        else
        {
            await collection.FindOneAndDeleteAsync(filter, cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<List<TEntity>> FindAllAsync(List<TIdentity> ids, CancellationToken cancellationToken)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        // Convert the Predicate<T> to a FilterDefinition<T>
        FilterDefinition<TEntity>? filter = new FilterDefinitionBuilder<TEntity>().Where(x => ids.Contains(x.Id));

        // Fetch all items matching the filter/predicate
        IAsyncCursor<TEntity>? result = unitOfWork.Session != null
            ? await collection.FindAsync(unitOfWork.Session, filter, null, cancellationToken)
            : await collection.FindAsync(filter, null, cancellationToken);

        return await result.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateBatchAsync(List<TEntity> aggregates, CancellationToken cancellationToken)
    {
        if (aggregates == null)
            throw new ArgumentNullException(nameof(aggregates));

        // Create a list to hold our update operations
        List<WriteModel<TEntity>> updates = new List<WriteModel<TEntity>>();

        foreach (TEntity aggregate in aggregates)
        {
            FilterDefinition<TEntity>? filter = Builders<TEntity>.Filter.Eq("_id", aggregate.Id);
            UpdateAuditData(aggregate);
            // Define an update operation to replace the document with our aggregate
            ReplaceOneModel<TEntity> update = new(filter, aggregate);
            updates.Add(update);
        }

        // Execute the bulk write operation
        if (updates.Any())
        {
            if (unitOfWork.Session != null)
            {
                await collection.BulkWriteAsync(unitOfWork.Session, updates, cancellationToken: cancellationToken);
            }
            else
            {
                await collection.BulkWriteAsync(updates, cancellationToken: cancellationToken);
            }
        }
    }

    /// <summary>
    /// Bulk Insert
    /// </summary>
    /// <param name="aggregates"></param>
    /// <param name="cancellationToken"></param>
    public async Task InsertBatchAsync(List<TEntity> aggregates, CancellationToken cancellationToken)
    {
        if (aggregates == null)
            throw new ArgumentNullException(nameof(aggregates));

        // Create a list to hold our inserts operations
        List<WriteModel<TEntity>> inserts = [];

        foreach (TEntity aggregate in aggregates)
        {
            // Define an update operation to replace the document with our aggregate
            UpdateAuditData(aggregate);
            InsertOneModel<TEntity> insert = new(aggregate);
            inserts.Add(insert);
        }

        // Execute the bulk write operation
        if (inserts.Any())
        {
            if (unitOfWork.Session != null)
            {
                await collection.BulkWriteAsync(unitOfWork.Session, inserts, cancellationToken: cancellationToken);
            }
            else
            {
                await collection.BulkWriteAsync(inserts, cancellationToken: cancellationToken);
            }
        }
    }
    private void UpdateAuditData(TEntity aggregate)
    {
        aggregate.AuditInfo.LastModifiedAt = DateTime.UtcNow;
        aggregate.AuditInfo.LastModifiedBy = userContextProvider.GetUserId();
    }

    /// <summary>
    /// Bulk Insert
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="cancellationToken"></param>
    public async Task DeleteBatchAsync(List<TIdentity> ids, CancellationToken cancellationToken)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        // Create a list to hold our inserts operations
        List<WriteModel<TEntity>> deletes = [];

        foreach (TIdentity aggregateId in ids)
        {
            FilterDefinition<TEntity>? filter = Builders<TEntity>.Filter.Eq("_id", aggregateId);

            // Define an update operation to replace the document with our aggregate
            DeleteOneModel<TEntity> delete = new(filter);
            deletes.Add(delete);
        }

        // Execute the bulk write operation
        if (deletes.Any())
        {
            if (unitOfWork.Session != null)
            {
                await collection.BulkWriteAsync(unitOfWork.Session, deletes, cancellationToken: cancellationToken);
            }
            else
            {
                await collection.BulkWriteAsync(deletes, cancellationToken: cancellationToken);
            }
        }
    }

    /// <summary>
    /// Bulk Insert
    /// </summary>
    /// <param name="agregates"></param>
    /// <param name="cancellationToken"></param>
    public async Task DeleteBatchAsync(List<TEntity> agregates, CancellationToken cancellationToken)
    {
        await DeleteBatchAsync(agregates.Select(x => x.Id).ToList(), cancellationToken);
    }
}
