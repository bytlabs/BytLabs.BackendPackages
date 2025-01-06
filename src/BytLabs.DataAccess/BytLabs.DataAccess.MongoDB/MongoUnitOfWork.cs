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

    public async Task RollbackAsync()
    {
        if (Session == null || Session.IsInTransaction == false)
            throw new InfrastructureException("Transaction was not initialized.");
        await Session?.AbortTransactionAsync()!;
        Session?.Dispose();
        Session = null;
    }
}
