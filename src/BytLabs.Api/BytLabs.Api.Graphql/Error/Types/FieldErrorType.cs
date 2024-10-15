namespace BytLabs.Api.Graphql.Error.Types
{
    /// <summary>
    /// Defines the GraphQL type for FieldError.
    /// </summary>
    public class FieldErrorType : ObjectType<FieldError>
    {
        /// <summary>
        /// Configures the GraphQL type for FieldError.
        /// </summary>
        /// <param name="descriptor">The descriptor used to configure the type.</param>
        protected override void Configure(IObjectTypeDescriptor<FieldError> descriptor)
        {
            descriptor.BindFieldsImplicitly();
        }
    }
}