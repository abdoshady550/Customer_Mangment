using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;

namespace Customer_Mangment.CQRS.Customers.Mappers
{
    public interface ICustomerMapper
    {
        CustomerDto ToCustomerDto(Customer customer);
        CustomerHistoryDto ToCustomerHistoryDto(CustomerHistory history);
        AddressDto ToAddressDto(Address address);
        AddressHistoryDto ToAddressHistoryDto(AddressHistory addressHistory);
        List<CustomerHistoryDto> ToCustomerHistoryDtoList(List<CustomerHistory> history);
        List<AddressHistoryDto> ToAddressHistoryDtoList(List<AddressHistory> history);
        List<AddressDto> ToAddressDtoList(List<Address> addresses);
        List<CustomerDto> ToCustomerDtoList(List<Customer> customers);
    }


}
