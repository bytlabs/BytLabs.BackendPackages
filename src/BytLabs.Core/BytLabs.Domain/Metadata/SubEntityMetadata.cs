namespace BytLabs.Domain.Metadata
{
    /// <summary>
    /// Represents metadata information about a sub-entity within an aggregate.
    /// Inherits from EntityMetadata to maintain consistent metadata structure.
    /// </summary>
    public class SubEntityMetadata : EntityMetadata
    {
        /// <summary>
        /// Initializes a new instance of the SubEntityMetadata class.
        /// </summary>
        /// <param name="type">The type name of the sub-entity</param>
        /// <param name="id">The unique identifier of the sub-entity</param>
        public SubEntityMetadata(string type, string id) : base(type, id)
        {

        }
    }
}