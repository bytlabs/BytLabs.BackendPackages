using BytLabs.Application.DataAccess;
using BytLabs.Infrastructure.Exceptions;
using MongoDB.Driver;

namespace BytLabs.DataAccess.MongoDB;

/// <summary>
/// MongoDB implementation of the Unit of Work pattern.
/// Manages database transactions and session lifecycle.
/// </summary>
public class MongoUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Gets the current MongoDB client session handle, if a transaction is active
    /// </summary>
    public IClientSessionHandle? Session { get; private set; }
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Initializes a new instance of the MongoUnitOfWork class
    /// </summary>
    /// <param name="mongoDatabase">The MongoDB database instance</param>
    public MongoUnitOfWork(IMongoDatabase mongoDatabase)
    {
        _database = mongoDatabase;
    }

    public void Dispose()
    {
        Session?.Dispose();
    }

    public async Task OpenTransactionAsync()
    {
        Session = await _database.Client.StartSessionAsync();
        Session?.StartTransaction();
    }


    public async Task CommitAsync()
    {
        if (Session == null || Session.IsInTransaction == false)
            throw new InfrastructureException("Transaction was not initialized."); 
        await Session?.CommitTransactionAsync()!;
        Session?.Dispose();
        Session = null;
    }

    /// <summary>
    /// Abandons the current transaction, so that nothing it did is kept.
    /// </summary>
    /// <exception cref="InfrastructureException">No transaction was ever opened.</exception>
    /// <remarks>
    /// A session with no transaction left in it is <b>not</b> an error here. The server ends a
    /// transaction itself when it aborts one — losing a write conflict is enough, and a write
    /// conflict is an ordinary event — so by the time a failing command reaches its rollback the
    /// transaction can already be gone. Nothing is committed, which is all a rollback has to
    /// guarantee, so there is nothing left to do but let go of the session.
    /// <para>
    /// Throwing here instead would replace whatever the command actually failed on: the caller is
    /// told "Transaction was not initialized" and never learns about the write conflict that
    /// started it. Calling <c>AbortTransaction</c> anyway does not work either — the driver
    /// refuses it once the transaction has ended.
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

        if (Session.IsInTransaction)
            await Session.AbortTransactionAsync();

        Session.Dispose();
        Session = null;
    }
}
