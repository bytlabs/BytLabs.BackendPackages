using BytLabs.Api.Graphql.InputTypes;
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
            this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddGraphQLServer()
                .AddBytLabsDefaults();
        }

        public static IRequestExecutorBuilder AddDynamicDataTypes(
            this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .AddType<DataFilterInputType>();
        }

        public static IRequestExecutorBuilder AddDefaultQuerySettings(
            this IRequestExecutorBuilder requestExecutorBuilder, string? name)
        {
            return requestExecutorBuilder
                .ModifyPagingOptions(config =>
                {
                    config.MaxPageSize = 50;
                })
                .AddProjections(name)
                .AddFiltering(name)
                .AddSorting(name)
                .AddQueryableCursorPagingProvider(name);
        }

        /// <summary>
        /// Registers Observability, Auth, Mutation conventions and query settings.
        /// </summary>
        /// <param name="requestExecutorBuilder"></param>
        /// <returns></returns>
        internal static IRequestExecutorBuilder AddBytLabsDefaults(
            this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .AddObservability()
                .AddMutationConventions()
                .AddDefaultRuntimeTypeMappings()
                .AddAuthorization();
        }

        internal static IRequestExecutorBuilder AddMutationConventions(
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

        internal static IRequestExecutorBuilder AddDefaultRuntimeTypeMappings(
            this IRequestExecutorBuilder requestExecutorBuilder)
        {
            return requestExecutorBuilder
                .BindRuntimeType<Guid, IdType>()
                .BindRuntimeType<ulong, StringType>();
        }
    }
}
