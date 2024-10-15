using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace BytLabs.DataAccess.MongDB.Conventions
{
    internal static class MongoDbConventions
    {
        internal static void RegisterMongoConventions()
        {

            var conventionPack = new ConventionPack
                                 {
                                     new CamelCaseElementNameConvention(),
                                     new EnumRepresentationConvention(BsonType.String),
                                     new IgnoreExtraElementsConvention(true), new IgnoreIfNullConvention(true)
                                 };
            ConventionRegistry.Register("all", conventionPack, _ => true);
        }
    }
}