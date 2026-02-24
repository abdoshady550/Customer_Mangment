using Customer_Mangment.CQRS.Customers.Addresses.DTOS;

namespace Customer_Mangment.CQRS.Customers.DTOS
{
    public sealed record CustomerDto(Guid Id, string Name, string Mobile, string CreatedBy, string UpdatedBy, List<AddressDto> Addresses);
}
