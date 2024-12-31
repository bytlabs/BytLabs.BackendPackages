using BytLabs.Api.Graphql.Error.Types;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.Api.Graphql
{
    /// <summary>
    /// Provides extension methods for configuring the GraphQL request executor builder.
    /// </summary>
    public static class RequestExecutorBuilderExtension
    {

        /// <summary>
        /// Adds a command type to the GraphQL schema as an input type.
        /// </summary>
        /// <typeparam name="TCommand">The command type to add.</typeparam>
        /// <param name="requestExecutorBuilder">The GraphQL request executor builder.</param>
        /// <returns>The configured request executor builder with added command type.</returns>
        /// <remarks>
        /// The input type name will be derived from the command type name by replacing "Command" with "Input".
        /// For example, "CreateUserCommand" becomes "CreateUserInput".
        /// </remarks>
        public static IRequestExecutorBuilder AddCommandType<TCommand>(this IRequestExecutorBuilder requestExecutorBuilder)
        {
            requestExecutorBuilder.AddInputObjectType<TCommand>(x => x.Name(typeof(TCommand).Name.Replace("Command", "Input")));
            return requestExecutorBuilder;
        }

        /// <summary>
        /// Adds custom error types to the GraphQL schema.
        /// </summary>
        /// <param name="requestExecutorBuilder">The GraphQL request executor builder.</param>
        /// <returns>The configured request executor builder with added error types.</returns>
        /// <remarks>
        /// This method adds the following error types:
        /// - <see cref="BusinessErrorType"/>
        /// - <see cref="ValidationErrorType"/>
        /// - <see cref="FieldErrorType"/>
        /// Note: This should not be used in combination with error attributes.
        /// </remarks>
        internal static IRequestExecutorBuilder AddErrorTypes(this IRequestExecutorBuilder requestExecutorBuilder)
        {
            requestExecutorBuilder
                .AddType<BusinessErrorType>()
                .AddType<ValidationErrorType>()
                .AddType<FieldErrorType>();
                
            return requestExecutorBuilder;
        }
    }
}
