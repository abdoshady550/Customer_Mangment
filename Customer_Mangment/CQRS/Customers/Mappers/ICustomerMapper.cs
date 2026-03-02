using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;

namespace Customer_Mangment.CQRS.Customers.Mappers
{
    public interface ICustomerMapper
    {
        CustomerDto ToCustomerDto(Customer customer);
        List<CustomerDto> ToCustomerDtoList(List<Customer> customers);

        AddressDto ToAddressDto(Address address);
        List<AddressDto> ToAddressDtoList(List<Address> addresses);
        Customer ToCustomerSanpDto(CustomerSnapshot snapshot);
        List<Customer> ToCustomerSanpDtolist(List<CustomerSnapshot> Snapshots);
        Address ToAddressSnapDto(AddressSnapshot snapshot);
        List<Address> ToAddressSnapDtoList(List<AddressSnapshot> Snapshots);
    }


}
