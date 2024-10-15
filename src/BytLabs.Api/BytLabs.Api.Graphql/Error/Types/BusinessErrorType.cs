using BytLabs.Api.Graphql.Error;
using HotChocolate.Types;

namespace BytLabs.Api.Graphql.Error.Types
{
    /// <summary>
    /// Defines the GraphQL type for BusinessError.
    /// </summary>
    public class BusinessErrorType : ObjectType<BusinessError>
    {
        /// <summary>
        /// Configures the GraphQL type for BusinessError.
        /// </summary>
        /// <param name="descriptor">The descriptor used to configure the type.</param>
        protected override void Configure(IObjectTypeDescriptor<BusinessError> descriptor)
        {
            descriptor.BindFieldsImplicitly();
        }
    }
}