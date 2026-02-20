using Customer_Mangment.Model.Results;

namespace Customer_Mangment.Model.Entities
{
    public sealed class Customer
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Mobile { get; private set; }
        public bool IsDeleted { get; private set; } = false;

        private readonly List<Address> _addresses = new();
        public IReadOnlyCollection<Address> Addresses => _addresses;
        private readonly List<CustomerHistory> _customerHistories = new();
        public IReadOnlyCollection<CustomerHistory> CustomerHistory => _customerHistories;

        private Customer() { }
        private Customer(string name, string mobile)
        {
            Id = Guid.NewGuid();
            Name = name;
            Mobile = mobile;
        }

        private Customer(string name, string mobile, List<Address> addresses)
        {
            Id = Guid.NewGuid();
            Name = name;
            Mobile = mobile;
            _addresses.AddRange(addresses);
        }
        public static Result<Customer> CreateCustomer(string name, string mobile, IEnumerable<(AdressType type, string value)> addresses)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Error.Failure("Invalide_Name", "Name cannot be null or empty");

            if (string.IsNullOrWhiteSpace(mobile))
                return Error.Failure("Invalide_Mobile", "Mobile number cannot be null or empty");

            var customer = new Customer(name, mobile);

            foreach (var a in addresses)
            {
                var result = customer.AddAddress(a.type, a.value);
                if (result.IsError)
                    return result.Errors;
            }

            return customer;
        }
        public Result<Updated> UpdateCustomer(string? name, string? mobile)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            if (!string.IsNullOrWhiteSpace(mobile))
                Mobile = mobile;
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
