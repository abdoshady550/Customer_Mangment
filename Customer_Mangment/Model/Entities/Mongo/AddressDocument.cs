using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities.Mongo
{
    public sealed class AddressDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid CustomerId { get; set; }

        public int Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
