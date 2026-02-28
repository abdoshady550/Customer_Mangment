using Customer_Mangment.CQRS.Customers.Addresses.DTOS;

namespace Customer_Mangment.CQRS.Customers.DTOS
{
    public class CustomerHistoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool IsDeleted { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }

    }

    public class AddressHistoryDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public int Type { get; set; }
        public string Value { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
    public class CustomerAddressHistoryDto
    {
        //public List<CustomerHistoryDto> CustomerHistoryDtos { get; set; } = new();
        //public List<AddressHistoryDto> AddressHistoryDtos { get; set; } = new();
        public List<CustomerDto> CustomerHistoryDtos { get; set; } = new();
        public List<AddressDto> AddressHistoryDtos { get; set; } = new();

    }


}
