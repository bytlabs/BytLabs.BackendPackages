namespace BytLabs.DataAccess.UnitOfWork
{
    /// <summary>
    /// Configures the behavior of the Unit of Work pattern implementation.
    /// </summary>
    public class UnitOfWorkOptions
    {
        /// <summary>
        /// Gets or sets whether database transactions should be used.
        /// </summary>
        /// <remarks>
        /// When set to true (default), the Unit of Work will manage database transactions.
        /// Set to false when:
        /// - Working with databases that don't support transactions
        /// - Transaction management is handled externally
        /// - Performance optimization is needed for read-only operations
        /// </remarks>
        public bool UseTransactions { get; set; } = true;
    }
}