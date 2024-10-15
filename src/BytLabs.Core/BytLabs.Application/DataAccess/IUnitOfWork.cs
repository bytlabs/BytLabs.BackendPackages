namespace BytLabs.Application.DataAccess
{
    /// <summary>
    /// Represents the Unit of Work pattern for managing database transactions.
    /// Ensures data consistency by treating multiple operations as a single atomic unit.
    /// Implements IDisposable to properly manage database resources.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Begins a new database transaction asynchronously.
        /// Must be called before performing any database operations that require transaction support.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task OpenTransactionAsync();

        /// <summary>
        /// Commits all changes made within the current transaction to the database.
        /// All operations will be persisted if successful, or none if an error occurs.
        /// </summary>
        /// <returns>A task representing the asynchronous commit operation</returns>
        Task CommitAsync();

        /// <summary>
        /// Rolls back all changes made within the current transaction.
        /// Restores the database to its state before the transaction began.
        /// </summary>
        /// <returns>A task representing the asynchronous rollback operation</returns>
        Task RollbackAsync();
    }
}