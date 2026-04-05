using Customer_Mangment.Model.Results;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities
{
    public sealed class Address : IMustHaveTenant
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; private set; }
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid CustomerId { get; private set; }
        public AdressType Type { get; private set; }
        public string Value { get; private set; } = string.Empty;
        public Customer Customer { get; private set; }
        public string TenantId { get; set; } = string.Empty;

        private Address() { }

        public Address(AdressType type, string value)
        {
            Id = Guid.NewGuid();
            Type = type;
            Value = value;
        }
        public static Result<Address> CreateAddress(AdressType type, string value, IStringLocalizer<SharedResource> localizer)
        {

            if (string.IsNullOrWhiteSpace(value))
                return LocalizedError.Validation(localizer, "Invalide_Address", ResourceKeys.Validation.AddressValueEmpty);

            return new Address(type, value);
        }
        public Result<Updated> UpdateAddress(AdressType? type, string? value, string updatedBy)
        {
            if (!string.IsNullOrWhiteSpace(value))
                Value = value;
            if (type.HasValue)
                Type = type.Value;
            Customer.UpdatedBy = updatedBy;

            return Result.Updated;
        }
        public void SetCustomer(Customer customer)
        {
            Customer = customer;
            CustomerId = customer.Id;
        }
    }

    public enum AdressType
    {
        Home = 1,
        Work = 2,
        Other = 3
    }
}