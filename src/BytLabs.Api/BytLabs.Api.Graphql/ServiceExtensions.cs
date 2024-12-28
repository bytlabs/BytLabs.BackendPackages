using BytLabs.Api.Graphql.Observability;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.Api.Graphql
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers Hot Chocolate GraphQL server with added Observability, Auth, Mutation conventions and query settings.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public static IRequestExecutorBuilder AddGraphQLService(
            this IServiceCollection serviceCollection,
            string? schemaName = null)
        {
            return serviceCollection
                .AddGraphQLServer(schemaName)
                .AddBytLabsDefaults()
                .AddQueryType()
                .AddMutationType()
                .AddErrorTypes()
                ;
        }

        /// <summary>
        /// Registers Observability, Auth, Mutation conventions and query settings.
        /// </summary>
        /// <param name="requestExecutorBuilder"></param>
        /// <returns></returns>
        public static IRequestExecutorBuilder AddBytLabsDefaults(
            this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .AddObservability()
                .AddQuerySettings()
                .AddMutationConventions()
                .AddDefaultRuntimeTypeMappings()
                .AddAuthorization();
        }

        public static IRequestExecutorBuilder AddMutationConventions(
            this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .AddMutationConventions(new MutationConventionOptions
                {
                    InputArgumentName = "input",
                    InputTypeNamePattern = $"{{MutationName}}Input",
                    PayloadTypeNamePattern = $"{{MutationName}}Payload",
                    PayloadErrorTypeNamePattern = $"{{MutationName}}Error",
                    PayloadErrorsFieldName = "errors",
                    ApplyToAllMutations = true
                })
                ;
        }

        public static IRequestExecutorBuilder AddDefaultRuntimeTypeMappings(
            this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .BindRuntimeType<Guid, IdType>()
                .BindRuntimeType<ulong, StringType>();
        }

        public static IRequestExecutorBuilder AddQuerySettings(
            this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .SetPagingOptions(new()
                {
                    MaxPageSize = 50
                })
                .AddProjections()
                .AddFiltering()
                .AddSorting()
                .AddQueryableCursorPagingProvider();
        }
    }
}
