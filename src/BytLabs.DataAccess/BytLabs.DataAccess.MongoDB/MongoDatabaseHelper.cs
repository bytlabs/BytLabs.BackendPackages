using BytLabs.Domain.Entities;
using BytLabs.Multitenancy;
using GuardClauses;
using Humanizer;
using MongoDB.Driver;

namespace BytLabs.DataAccess.MongoDB
{
    /// <summary>
    /// Helper methods for MongoDB database operations and naming conventions
    /// </summary>
    public static class MongoDatabaseHelper
    {
        /// <summary>
        /// Creates a collection name for an aggregate root type following MongoDB naming conventions
        /// </summary>
        /// <typeparam name="TAggregate">The aggregate root type</typeparam>
        /// <returns>The formatted collection name</returns>
        public static string CreateCollectionName<TAggregate>() where TAggregate : IEntity
        {
            string collectionName = typeof(TAggregate).Name.Replace("Aggregate", "");

            return System.Text.RegularExpressions.Regex.Replace(collectionName, "([a-z])([A-Z])", "$1-$2").
                ToLowerInvariant().Pluralize(); ;
        }

        /// <summary>
        /// Generates a database name for a specific tenant following naming conventions
        /// </summary>
        /// <param name="databaseName">The base database name</param>
        /// <param name="tenantId">The tenant identifier</param>
        /// <param name="configuration">Database configuration options</param>
        /// <returns>The tenant-specific database name</returns>
        public static string GetDatabaseNameForTenant(string databaseName, TenantId tenantId, DatabaseConfiguration configuration)
        {
            GuardClause.ArgumentIsNotNull(databaseName, nameof(databaseName));
            GuardClause.ArgumentIsNotNull(tenantId, nameof(tenantId));
            GuardClause.ArgumentIsNotNull(tenantId.Value, nameof(tenantId.Value));
            var databaseNameForTenant = $"{databaseName}-{tenantId.Value}";

            if (configuration.IgnoreDatabaseNamingConvention)
            {
                return databaseNameForTenant;
            }
            return databaseNameForTenant.ToLower();
        }
    }
}