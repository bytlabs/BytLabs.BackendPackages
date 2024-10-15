using BytLabs.Application.DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.UnitOfWork
{
    /// <summary>
   /// Defines a factory for creating Unit of Work instances.
   /// </summary>
   /// <remarks>
   /// This factory is responsible for:
   /// - Creating new Unit of Work instances
   /// - Ensuring proper initialization of transactions
   /// - Managing the lifecycle of database connections
   /// - Providing a consistent way to begin database operations
   /// 
   /// The factory pattern is used to abstract the creation and setup of Unit of Work instances,
   /// allowing for different implementations and configurations while maintaining a consistent interface.
   /// </remarks>
    public class UnitOfWorkFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public UnitOfWorkFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

       /// <summary>
       /// Creates and initializes a new Unit of Work instance.
       /// </summary>
       /// <returns>A configured Unit of Work instance with an initialized transaction.</returns>
       /// <remarks>
       /// This method:
       /// - Creates a new Unit of Work instance
       /// - Initializes any required database connections
       /// - Opens a new transaction if transactions are enabled
       /// - Returns a ready-to-use Unit of Work
       /// 
       /// </remarks>

        public async Task<IUnitOfWork> CreateUnitOfWork()
        {
            var result = (IUnitOfWork)_serviceProvider.GetRequiredService(typeof(IUnitOfWork))!;
            if (result == null)
                throw new("UnitOfWork not found.");

            await result.OpenTransactionAsync();
            return result;
        }
    }
}