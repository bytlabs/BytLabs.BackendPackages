using BytLabs.DataAccess.MongDB.Configuration;
using BytLabs.Multitenancy;
using GuardClauses;
using MongoDB.Driver;
using System.Collections.Concurrent;

namespace BytLabs.DataAccess.MongDB
{
    /// <summary>
    /// Factory for creating MongoDB database instances with multi-tenancy support.
    /// Manages database connections and caches database instances.
    /// </summary>
    public class MongoDatabaseFactory
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoDatabaseConfiguration _databaseConfiguration;
        private static readonly ConcurrentDictionary<string, IMongoDatabase> Databases = new ConcurrentDictionary<string, IMongoDatabase>();

        /// <summary>
        /// Initializes a new instance of the MongoDatabaseFactory class
        /// </summary>
        /// <param name="mongoClient">The MongoDB client instance</param>
        /// <param name="databaseConfiguration">Configuration for the MongoDB database</param>
        public MongoDatabaseFactory(IMongoClient mongoClient, MongoDatabaseConfiguration databaseConfiguration)
        {
            GuardClause.ArgumentIsNotNull(mongoClient, nameof(mongoClient));
            GuardClause.ArgumentIsNotNull(databaseConfiguration, nameof(databaseConfiguration));

            _mongoClient = mongoClient;
            _databaseConfiguration = databaseConfiguration;
        }

        /// <summary>
        /// Gets or creates a MongoDB database instance for the specified tenant
        /// </summary>
        /// <param name="tenantId">The tenant identifier</param>
        /// <param name="databaseConfiguration">Additional database configuration options</param>
        /// <returns>A MongoDB database instance for the tenant</returns>
        public IMongoDatabase GetDatabaseForTenant(TenantId tenantId, DatabaseConfiguration databaseConfiguration)
        {
            GuardClause.ArgumentIsNotNull(tenantId, nameof(tenantId));
            string multiTenantDatabaseName = MongoDatabaseHelper.GetDatabaseNameForTenant(_databaseConfiguration.DatabaseName, tenantId, databaseConfiguration);

            return Databases.GetOrAdd(multiTenantDatabaseName, dbName =>
            {
                return _mongoClient.GetDatabase(dbName);
            });
        }
    }
}