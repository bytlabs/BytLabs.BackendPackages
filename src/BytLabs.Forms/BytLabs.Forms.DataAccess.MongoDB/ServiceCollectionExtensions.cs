using BytLabs.Forms.Domain;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace BytLabs.Forms.DataAccess.MongoDB
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFormData<T>(this IServiceCollection services)
        {
            BsonSerializer.TryRegisterSerializer(typeof(FormData<T>), new FormDataSerializer<T>());

            return services;
        }
    }
}
