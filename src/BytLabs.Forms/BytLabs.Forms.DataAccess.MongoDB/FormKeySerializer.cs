using BytLabs.Forms.Domain;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace BytLabs.Forms.DataAccess.MongoDB
{
    public class FormKeySerializer : SerializerBase<FormKey>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, FormKey value)
        {
            // Serialize the FormKey as a string (its Value property)
            context.Writer.WriteString(value.Value);
        }

        public override FormKey Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Deserialize the FormKey from the string value
            var value = context.Reader.ReadString();
            return new FormKey(value);  // Create FormKey from the string
        }
    }

}