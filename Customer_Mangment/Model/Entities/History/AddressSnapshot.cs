using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities.History
{
    public sealed class AddressSnapshot
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid AddressId { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid CustomerId { get; set; }

        public AdressType Type { get; set; }
        public string Value { get; set; } = string.Empty;

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public string Operation { get; set; } = string.Empty;
    }
}