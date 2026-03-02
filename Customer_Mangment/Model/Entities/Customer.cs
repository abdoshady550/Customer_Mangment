using Customer_Mangment.Model.Results;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Customer_Mangment.Model.Entities
{
    public sealed class Customer
    {
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Mobile { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public string CreatedBy { get; private set; }

        public string UpdatedBy { get; set; }


        private readonly List<Address> _addresses = new();
        public IReadOnlyCollection<Address> Addresses => _addresses;


        public Customer() { }
        private Customer(string name, string mobile, string createdBy)
        {
            Id = Guid.NewGuid();
            Name = name;
            Mobile = mobile;
            CreatedBy = createdBy;
            UpdatedBy = createdBy;

        }
        public static Result<Customer> CreateCustomer(string name, string mobile, string createdBy, IEnumerable<(AdressType type, string value)> addresses)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Error.Failure("Invalide_Name", "Name cannot be null or empty");

            if (string.IsNullOrWhiteSpace(mobile))
                return Error.Failure("Invalide_Mobile", "Mobile number cannot be null or empty");

            var customer = new Customer(name, mobile, createdBy);

            foreach (var a in addresses)
            {
                var result = customer.AddAddress(a.type, a.value);
                if (result.IsError)
                    return result.Errors;
            }

            return customer;
        }
        public Result<Updated> UpdateCustomer(string? name, string? mobile, string updatedBy)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            if (!string.IsNullOrWhiteSpace(mobile))
                Mobile = mobile;
            UpdatedBy = updatedBy;
            return Result.Updated;
        }
        public Result<Address> AddAddress(AdressType type, string value)
        {
            if (_addresses.Any(a => a.Type == type))
                return Error.Validation("DuplicateAddressType", $"Duplicate address type: {type}");

            var addressResult = Address.CreateAddress(type, value);
            if (addressResult.IsError)
                return addressResult.Errors;

            var address = addressResult.Value;

            address.SetCustomer(this);

            _addresses.Add(address);

            return address;
        }

        public void DeleteCustomer()
        {
            IsDeleted = true;
        }
    }
}
