using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities.Mongo
{
    public sealed class CustomerDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;

        public List<AddressDocument> Addresses { get; set; } = new();
    }
}
