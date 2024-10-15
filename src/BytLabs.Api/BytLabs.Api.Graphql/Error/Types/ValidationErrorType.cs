using HotChocolate.Types;

namespace BytLabs.Api.Graphql.Error.Types
{
    /// <summary>
    /// Defines the GraphQL type for ValidationError.
    /// </summary>
    public class ValidationErrorType : ObjectType<ValidationError>
    {
        /// <summary>
        /// Configures the GraphQL type for ValidationError.
        /// </summary>
        /// <param name="descriptor">The descriptor used to configure the type.</param>
        protected override void Configure(IObjectTypeDescriptor<ValidationError> descriptor)
        {
            descriptor.BindFieldsImplicitly();
        }
    }
}