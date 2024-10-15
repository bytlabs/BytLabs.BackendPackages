using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BytLabs.DataAccess.MongDB.Configuration
{
    /// <summary>
    /// Configuration settings for MongoDB database connections.
    /// </summary>
    public class MongoDatabaseConfiguration : DatabaseConfiguration
    {
        /// <summary>
        /// The name of the MongoDB database.
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// The MongoDB connection string.
        /// </summary>
        [Required]
        public string ConnectionString { get; set; } = null!;
    }
}