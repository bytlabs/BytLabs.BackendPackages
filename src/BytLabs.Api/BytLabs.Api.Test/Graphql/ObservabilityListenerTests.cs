using BytLabs.Hotchocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BytLabs.Api.Test.Graphql;

public class ObservabilityListenerTests
{
    // The diagnostic listener (ErrorLoggingDiagnosticsEventListener) takes ILogger<T>. In HotChocolate
    // v16 diagnostic listeners are activated from the schema service provider, so building the executor
    // must be able to resolve that application service — otherwise it throws
    // "Unable to resolve service for type ILogger<...> while attempting to activate ...".
    [Fact]
    public async Task GIVEN_observability_WHEN_building_executor_THEN_diagnostic_listener_activates()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();

        IRequestExecutor executor = await services
            .AddGraphQLService()
            .AddQueryType<TestQuery>()
            .BuildRequestExecutorAsync();

        Assert.NotNull(executor);
    }

    public class TestQuery
    {
        public string Hello() => "world";
    }
}
