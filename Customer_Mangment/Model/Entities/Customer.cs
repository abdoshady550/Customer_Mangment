using Customer_Mangment.Model.Results;

namespace Customer_Mangment.Model.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Mobile { get; private set; }

        private readonly List<Address> _addresses = new();
        public IReadOnlyCollection<Address> Addresses => _addresses;
        private Customer() { }

        Customer(string name, string mobile, List<Address> addresses)
        {
            Id = Guid.NewGuid();
            Name = name;
            Mobile = mobile;
            _addresses.AddRange(addresses);
        }
        public static Result<Customer> CreateCustomer(string name, string mobile, List<Address> addresses)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Error.Failure("Invalide_Name", "Name cannot be null or empty");

            if (string.IsNullOrWhiteSpace(mobile))
                return Error.Failure("Invalide_Mobile", "Mobile number cannot be null or empty");

            return new Customer(name, mobile, addresses);
        }
        public Result<Updated> UpdateCustomer(string? name, string? mobile)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            if (!string.IsNullOrWhiteSpace(mobile))
                Mobile = mobile;
            return Result.Updated;
        }
        public Result<Updated> AddAddress(AdressType type, string value)
        {
            var addressResult = Address.CreateAddress(type, value);
            if (addressResult.IsSuccess)
            {
                _addresses.Add(addressResult.Value);
                return Result.Updated;
            }
            return addressResult.Errors;
        }
    }
}
