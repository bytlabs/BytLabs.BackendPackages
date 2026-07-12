using BytLabs.Application.DataAccess;
using BytLabs.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BytLabs.DataAccess.EntityFramework;

/// <summary>
/// Entity Framework implementation of the Unit of Work pattern.
/// Manages the ambient database transaction and flushes tracked changes on commit.
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Gets the current EF transaction, if one is active.
    /// </summary>
    public IDbContextTransaction? Session { get; private set; }

    private readonly DbContext _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfUnitOfWork"/> class.
    /// </summary>
    /// <param name="database">The EF database context for the current request/tenant.</param>
    public EfUnitOfWork(DbContext database)
    {
        _database = database;
    }

    public void Dispose()
    {
        Session?.Dispose();
    }

    public async Task OpenTransactionAsync()
    {
        Session = await _database.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (Session == null)
            throw new InfrastructureException("Transaction was not initialized.");

        await _database.SaveChangesAsync();
        await Session.CommitAsync();
        Session.Dispose();
        Session = null;
    }

    public async Task RollbackAsync()
    {
        if (Session == null)
            throw new InfrastructureException("Transaction was not initialized.");

        await Session.RollbackAsync();
        Session.Dispose();
        Session = null;
    }
}
