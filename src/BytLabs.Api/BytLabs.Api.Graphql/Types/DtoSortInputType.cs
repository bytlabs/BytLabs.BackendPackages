using HotChocolate.Data.Sorting;

namespace BytLabs.Api.Graphql
{
    /// <summary>
    /// Base GraphQL sort input type for DTOs.
    /// </summary>
    /// <typeparam name="T">The DTO type to create sorting for</typeparam>
    public class DtoSortInputType<T> : SortInputType<T>
    {
        /// <summary>
        /// Configures the sort input type descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor to configure</param>
        protected override void Configure(ISortInputTypeDescriptor<T> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Name(typeof(T).Name.Replace("Dto", "") + "SortInput");
        }
    }
}
