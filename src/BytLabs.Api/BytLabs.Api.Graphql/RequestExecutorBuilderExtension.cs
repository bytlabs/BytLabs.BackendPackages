using BytLabs.Api.Graphql.Error.Business;
using BytLabs.Api.Graphql.Error.Validation;
using BytLabs.Api.Graphql.InputTypes;
using BytLabs.Api.Graphql.ObjectTypes;
using BytLabs.Domain.Entities;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace BytLabs.Api.Graphql
{
    /// <summary>
    /// Provides extension methods for configuring the GraphQL request executor builder.
    /// </summary>
    public static class RequestExecutorBuilderExtension
    {
        public static IRequestExecutorBuilder AddAggregateFilterType<TAggregate, TId>(this IRequestExecutorBuilder requestExecutorBuilder)
            where TAggregate : IAggregateRoot<TId>
        {
            return requestExecutorBuilder
                .AddType<AggregateFilterInput<TAggregate, TId>>();
        }

        public static IRequestExecutorBuilder AddAggregateSortType<TAggregate, TId>(this IRequestExecutorBuilder requestExecutorBuilder)
            where TAggregate : IAggregateRoot<TId>
        {
            return requestExecutorBuilder
                .AddType<AggregateSortInput<TAggregate, TId>>();
        }


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
            requestExecutorBuilder.AddInputObjectType<TCommand>(x =>
            {
                var inputName = typeof(TCommand).Name
                                .Replace("Command", "Input")
                                .Replace("Dto", "Input");
                x.Name(inputName);
            });
            return requestExecutorBuilder;
        }

        /// <summary>
        /// Adds a DTO (Data Transfer Object) type to the GraphQL schema.
        /// </summary>
        /// <typeparam name="T">The DTO type to add.</typeparam>
        /// <param name="requestExecutorBuilder">The GraphQL request executor builder.</param>
        /// <returns>The configured request executor builder with added DTO type.</returns>
        /// <remarks>
        /// This method registers a DTO type using the <see cref="DtoType{T}"/> wrapper to expose it in the GraphQL schema.
        /// </remarks>
        public static IRequestExecutorBuilder AddDtoType<T>(this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .AddType<DtoType<T>>();
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
