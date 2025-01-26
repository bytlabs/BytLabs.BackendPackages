namespace BytLabs.Api.Graphql.ObjectTypes
{
    /// <summary>
    /// Base GraphQL type for DTOs.
    /// </summary>
    /// <typeparam name="T">The DTO type to create a GraphQL type for</typeparam>
    public class DtoType<T> : ObjectType<T>
    {
        /// <summary>
        /// Configures the GraphQL type descriptor for the DTO.
        /// </summary>
        /// <param name="descriptor">The descriptor to configure</param>
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            descriptor.Name(typeof(T).Name.Replace("Dto", ""));
            descriptor.BindFieldsImplicitly();
        }
    }
}
