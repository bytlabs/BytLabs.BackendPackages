using MongoDB.Bson.Serialization;
using System.Text.Json;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using System.Globalization;
using MongoDB.Bson.IO;

namespace BytLabs.DataAccess.MongoDB.DynamicData
{
    public class JsonElementSerializer : SerializerBase<JsonElement>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined) return;

            var jsonString = value.ToString();
            var bsonDocument = BsonDocument.Parse(jsonString);
            bsonDocument.ConvertIsoStringsToBsonDates();
            BsonSerializer.Serialize(context.Writer, bsonDocument);
        }

        public override JsonElement Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.State == BsonReaderState.EndOfDocument) return default;

            var data = BsonSerializer.Deserialize<BsonDocument>(context.Reader);
            if (data == null) return default;

            data.ConvertBsonDatesToIsoStrings();
            var jsonString = data.ToString();
            return JsonDocument.Parse(jsonString).RootElement;
        }


    }
}
