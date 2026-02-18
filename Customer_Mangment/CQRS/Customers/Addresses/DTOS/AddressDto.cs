using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.CQRS.Customers.Addresses.DTOS
{
    public sealed record AddressDto(Guid Id, Guid CustomerId, AdressType Type, string Value);
}
