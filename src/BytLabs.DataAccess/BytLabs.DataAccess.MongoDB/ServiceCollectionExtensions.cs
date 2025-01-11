using BytLabs.Application.DataAccess;
using BytLabs.DataAccess.MongoDB.Configuration;
using BytLabs.DataAccess.MongoDB.Conventions;
using BytLabs.Domain.Entities;
using BytLabs.Multitenancy;
using GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Security.Principal;

namespace BytLabs.DataAccess.MongoDB
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDatabase(this IServiceCollection service,
            MongoDatabaseConfiguration config)
        {
            GuardClause.ArgumentIsNotNull(config, nameof(config));

            service.AddSingleton(config);

            if (config.IgnoreDatabaseNamingConvention == false)
            {
                MongoDbConventions.RegisterMongoConventions();
            }

            RegisterBaseEntityMongoClassMap();

            RegisterMongoDbServices(service, config);

            AddHealthChecks(service, config);


            return service;
        }

        public static IServiceCollection AddMongoRepository<TEntity, TIdentity>(this IServiceCollection services,
            string? collectionName = null, bool autoMap = true, Action<BsonClassMap<TEntity>>? configureEntity = null)
            where TEntity : IAggregateRoot<TIdentity>
        {
            RegisterMongoClassMapForEntities<TEntity, TIdentity>(autoMap, configureEntity);

            //convert collection name into MongoRepositoryOption to find name by generic entity type
            var dbCollectionName = collectionName ?? MongoDatabaseHelper.CreateCollectionName<TEntity>();
            services.TryAddSingleton(new MongoRepositoryOptions<TEntity>() { CollectionName = dbCollectionName });

            //DI generic mongo collection
            services.TryAddScoped(sp =>
            {
                var mongoRepositoryOptions = sp.GetRequiredService<MongoRepositoryOptions<TEntity>>();
                var mongoDatabase = sp.GetRequiredService<IMongoDatabase>();
                var mongoCollection = mongoDatabase.GetCollection<TEntity>(mongoRepositoryOptions.CollectionName);
                return mongoCollection;
            });

            //Add generic mongo repository implementation
            services.TryAddScoped<IRepository<TEntity, TIdentity>, MongoRepository<TEntity, TIdentity>>();

            //Adds domain event dispatcher decorator around the generic mongo repository implementation
            services.AddDomainEventsDecorator<TEntity, TIdentity>();

            return services;
        }

        private static void RegisterMongoClassMapForEntities<TEntity, TIdentity>(bool autoMap, Action<BsonClassMap<TEntity>>? configureEntity) where TEntity : IAggregateRoot<TIdentity>
        {
            BsonClassMap.TryRegisterClassMap<TEntity>(cm =>
            {
                if (autoMap)
                {
                    cm.AutoMap();
                }

                if (AggregateRootUtils.IsImplementingIAggregateRoot<TEntity, TIdentity>())
                {
                    cm.UnmapProperty(it => it.DomainEvents);
                }

                configureEntity?.Invoke(cm);
            });
        }

        private static void RegisterBaseEntityMongoClassMap()
        {
            BsonClassMap.TryRegisterClassMap<Entity<string>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetSerializer(new StringSerializer(BsonType.String));

            });
            BsonClassMap.TryRegisterClassMap<Entity<Guid>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetSerializer(new GuidSerializer(BsonType.String));
            });

            BsonClassMap.TryRegisterClassMap<AggregateRootBase<string>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetSerializer(new StringSerializer(BsonType.String));
                cm.UnmapProperty(it => it.DomainEvents);
            });

            BsonClassMap.TryRegisterClassMap<AggregateRootBase<Guid>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetSerializer(new StringSerializer(BsonType.String));
                cm.UnmapProperty(it => it.DomainEvents);
            });
        }

        private static void RegisterMongoDbServices(IServiceCollection service, MongoDatabaseConfiguration mongoOptions)
        {
            //DI db configs
            service.AddDatabase(mongoOptions);

            //Single factory that keep dictionary of active database connections for different tenants
            service.AddSingleton<MongoDatabaseFactory>();

            //DI mongoclient
            service.AddSingleton<IMongoClient>(s =>
            {
                var databaseConfiguration = s.GetRequiredService<MongoDatabaseConfiguration>();
                var settings = MongoClientSettings.FromConnectionString(databaseConfiguration.ConnectionString);
                settings.LoggingSettings = new(s.GetRequiredService<ILoggerFactory>());
                return new MongoClient(settings);
            });

            //DI database based on tenant
            service.AddScoped<IMongoDatabase>(provider =>
            {
                var mongoDatabaseFactory = provider.GetRequiredService<MongoDatabaseFactory>();
                var tenantIdProvider = provider.GetRequiredService<ITenantIdProvider>();
                var databaseConfiguration = provider.GetRequiredService<DatabaseConfiguration>();
                var mongoDatabse = mongoDatabaseFactory.GetDatabaseForTenant(tenantIdProvider.GetTenantId(), databaseConfiguration);

                return mongoDatabse;
            });

            //DI mongo unitOfWork implementation
            service.AddScoped<MongoUnitOfWork>();
            service.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MongoUnitOfWork>());
        }

        private static void AddHealthChecks(IServiceCollection service, MongoDatabaseConfiguration configuration)
            =>
                service
                    .AddHealthChecks()
                    //.AddMongoDb(configuration.ConnectionString, "health_MongoDB", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy, MongoHealthChecks.Tags)
                    ;
         
    }
}
