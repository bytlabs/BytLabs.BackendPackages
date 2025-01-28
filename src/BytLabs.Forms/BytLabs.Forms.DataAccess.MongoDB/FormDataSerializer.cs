using BytLabs.Forms.Domain;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace BytLabs.Forms.DataAccess.MongoDB
{
    public class FormDataSerializer<T> : SerializerBase<FormData<T>>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, FormData<T> value)
        {
            context.Writer.WriteStartDocument();
            foreach (var kvp in value)
            {
                context.Writer.WriteName(kvp.Key);
                BsonSerializer.Serialize(context.Writer, kvp.Value?.GetType() ?? typeof(object), kvp.Value);
            }
            context.Writer.WriteEndDocument();
        }

        public override FormData<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var dictionary = new FormData<T>();
            var reader = context.Reader;

            reader.ReadStartDocument();
            while (reader.State != BsonReaderState.EndOfDocument)
            {
                var key = reader.ReadName();
                var value = BsonSerializer.Deserialize<T>(reader);
                dictionary.Add(key, value);
            }
            reader.ReadEndDocument();

            return dictionary;
        }
    }

}
