using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities.History
{
    public sealed class CustomerSnapshot
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid CustomerId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        public string Operation { get; set; } = string.Empty;
    }
}