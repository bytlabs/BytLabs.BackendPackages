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

    /// <summary>
    /// Abandons the current transaction, so that nothing it did is kept.
    /// </summary>
    /// <exception cref="InfrastructureException">No transaction was ever opened.</exception>
    /// <remarks>
    /// A transaction the database has already ended is <b>not</b> an error here. A deadlock victim,
    /// a statement timeout or a reset connection all end one without this class being told, and the
    /// provider then refuses a rollback on it. Nothing is committed, which is all a rollback has to
    /// guarantee, so there is nothing left to do but let go of it.
    /// <para>
    /// A rollback runs in the failure path, so anything it throws replaces the failure that put it
    /// there — and the caller is left reading about the tidying up rather than the fault.
    /// </para>
    /// <para>
    /// Rolling back without ever having opened a transaction is a different thing: that is a
    /// mistake in the calling code, and it still says so.
    /// </para>
    /// </remarks>
    public async Task RollbackAsync()
    {
        if (Session == null)
            throw new InfrastructureException("Transaction was not initialized.");

        try
        {
            await Session.RollbackAsync();
        }
        catch (InvalidOperationException)
        {
            // How every provider says the transaction has already finished. Asking first would mean
            // reaching for the underlying DbTransaction, which is relational-only.
        }

        // The database has forgotten the writes; the context has not. Everything saved inside the
        // transaction is still tracked as Unchanged, so the next read in this scope is answered from
        // the identity map with values no row has any more. Let go of all of it.
        _database.ChangeTracker.Clear();

        Session.Dispose();
        Session = null;
    }
}
