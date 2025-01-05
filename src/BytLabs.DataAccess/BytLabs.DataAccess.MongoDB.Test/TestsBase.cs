using BytLabs.Application;
using BytLabs.DataAccess.MongDB;
using BytLabs.DataAccess.MongDB.Configuration;
using BytLabs.Multitenancy;
using BytLabs.Multitenancy.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.MongoDB.Test
{
    public abstract class TestsBase
    {
        protected static void ConfigureServices(IServiceCollection services)
        {
            services.AddFakeLogging();
            services.AddUserContextProviders();
            services.AddMultitenancy()
                        .AddResolver<ITenantIdResolver>(provider => new ValueTenantIdResolver(new TenantId("bytlabs")));
            services.AddCQS([typeof(TransactionTests).Assembly], _ => { });
            services.AddMongoDatabase(new MongoDatabaseConfiguration
            {
                ConnectionString = "mongodb://localhost:27017?retryWrites=false",
                DatabaseName = "test",
                UseTransactions = false
            })
                .AddMongoRepository<OrderAggregate, Guid>();
        }
    }
}
