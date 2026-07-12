using System.Linq.Expressions;
using BytLabs.Application.DataAccess;
using BytLabs.Application.Exceptions;
using BytLabs.Application.UserContext;
using BytLabs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BytLabs.DataAccess.EntityFramework;

/// <summary>
/// Entity Framework implementation of the generic repository pattern for aggregate roots.
/// Provides CRUD operations and batch processing. Persistence is deferred to the unit of work
/// (changes are flushed by <see cref="EfUnitOfWork.CommitAsync"/>).
/// </summary>
/// <typeparam name="TEntity">The aggregate root entity type.</typeparam>
/// <typeparam name="TIdentity">The type of the entity's identifier.</typeparam>
internal sealed class EfRepository<TEntity, TIdentity> : IRepository<TEntity, TIdentity>
    where TEntity : class, IAggregateRoot<TIdentity>
{
    private readonly DbContext _database;
    private readonly IUserContextProvider _userContextProvider;
    private readonly DbSet<TEntity> _dbSet;

    public EfRepository(DbContext database, IUserContextProvider userContextProvider)
    {
        _database = database;
        _userContextProvider = userContextProvider;
        _dbSet = database.Set<TEntity>();
    }

    /// <inheritdoc />
    public async Task<TEntity> GetByIdAsync(TIdentity id, CancellationToken cancellationToken)
    {
        var entity = await FindByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new EntityNotFoundException($"Entity with id={id} not found.");

        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity?> FindByIdAsync(TIdentity id, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(x => x.Id!.Equals(id))
            .IncludeAggregateEntities(_database)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> SingleOrDefaultAsync(CancellationToken cancellationToken)
    {
        var count = await _dbSet.CountAsync(cancellationToken);
        if (count > 1)
            throw new InvalidOperationException("More than one document found.");

        return await _dbSet.IncludeAggregateEntities(_database).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<TEntity>> FindAllAsync(List<TIdentity> ids, CancellationToken cancellationToken)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        return await _dbSet
            .Where(x => ids.Contains(x.Id))
            .IncludeAggregateEntities(_database)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> FindAllByAsync(
        Expression<Func<TEntity, bool>> filterExpression, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(filterExpression)
            .IncludeAggregateEntities(_database)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken)
    {
        entity.AuditInfo.CreatedAt = DateTime.UtcNow;
        entity.AuditInfo.CreatedBy = _userContextProvider.GetUserId();

        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        UpdateAuditData(entity);
        _dbSet.Update(entity);
        return await Task.FromResult(entity);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task InsertBatchAsync(List<TEntity> aggregates, CancellationToken cancellationToken)
    {
        if (aggregates == null)
            throw new ArgumentNullException(nameof(aggregates));

        foreach (var aggregate in aggregates)
        {
            aggregate.AuditInfo.CreatedAt = DateTime.UtcNow;
            aggregate.AuditInfo.CreatedBy = _userContextProvider.GetUserId();
        }

        await _dbSet.AddRangeAsync(aggregates, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateBatchAsync(List<TEntity> aggregates, CancellationToken cancellationToken)
    {
        if (aggregates == null)
            throw new ArgumentNullException(nameof(aggregates));

        foreach (var aggregate in aggregates)
            UpdateAuditData(aggregate);

        _dbSet.UpdateRange(aggregates);
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteBatchAsync(List<TIdentity> ids, CancellationToken cancellationToken)
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        var entities = await _dbSet.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(entities);
    }

    /// <inheritdoc />
    public async Task DeleteBatchAsync(List<TEntity> agregates, CancellationToken cancellationToken)
    {
        _dbSet.RemoveRange(agregates);
        await Task.CompletedTask;
    }

    private void UpdateAuditData(TEntity aggregate)
    {
        aggregate.AuditInfo.LastModifiedAt = DateTime.UtcNow;
        aggregate.AuditInfo.LastModifiedBy = _userContextProvider.GetUserId();
    }
}
