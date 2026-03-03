using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.CQRS.Customers.DTOS
{
    public sealed record CustomerHistoryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Mobile { get; init; } = string.Empty;
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
        public bool IsDeleted { get; init; }
        public string? Operation { get; init; }
        public DateTime ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }

    public sealed record AddressHistoryDto
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public AdressType Type { get; init; }
        public string Value { get; init; } = string.Empty;
        public string? Operation { get; init; }
        public DateTime ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }
    public class CustomerAddressHistoryDto
    {
        public List<CustomerHistoryDto> CustomerHistoryDtos { get; set; } = new();
        public List<AddressHistoryDto> AddressHistoryDtos { get; set; } = new();

    }


}
