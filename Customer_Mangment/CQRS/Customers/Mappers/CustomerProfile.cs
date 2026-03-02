using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Riok.Mapperly.Abstractions;

namespace Customer_Mangment.CQRS.Customers.Mappers
{
    [Mapper(
     PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive
)]
    public partial class CustomerMapper : ICustomerMapper
    {
        [MapperIgnoreSource(nameof(Customer.IsDeleted))]
        public partial CustomerDto ToCustomerDto(Customer customer);
        public partial List<CustomerDto> ToCustomerDtoList(List<Customer> customers);

        public partial Customer ToCustomerSanpDto(CustomerSnapshot snapshot);
        public partial List<Customer> ToCustomerSanpDtolist(List<CustomerSnapshot> Snapshots);

        [MapperIgnoreSource(nameof(Address.Customer))]
        public partial AddressDto ToAddressDto(Address address);
        public partial List<AddressDto> ToAddressDtoList(List<Address> addresses);
        public partial Address ToAddressSnapDto(AddressSnapshot snapshot);
        public partial List<Address> ToAddressSnapDtoList(List<AddressSnapshot> Snapshots);

    }

}
