using HotChocolate.Data.Filters;

namespace BytLabs.Api.Graphql
{
    /// <summary>
    /// Base GraphQL filter input type for DTOs.
    /// </summary>
    /// <typeparam name="T">The DTO type to create filters for</typeparam>
    public class DtoFilterInputType<T> : FilterInputType<T>
    {
        /// <summary>
        /// Configures the filter input type descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor to configure</param>
        protected override void Configure(IFilterInputTypeDescriptor<T> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Name(typeof(T).Name.Replace("Dto", "") + "FilterInput");
        }
    }
}
