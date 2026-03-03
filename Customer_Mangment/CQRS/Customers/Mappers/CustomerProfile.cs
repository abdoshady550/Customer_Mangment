using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;
using Riok.Mapperly.Abstractions;

namespace Customer_Mangment.CQRS.Customers.Mappers
{
    [Mapper(PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive)]
    public partial class CustomerMapper : ICustomerMapper
    {
        // ── Customer

        [MapperIgnoreSource(nameof(Customer.IsDeleted))]
        public partial CustomerDto ToCustomerDto(Customer customer);

        public partial List<CustomerDto> ToCustomerDtoList(List<Customer> customers);

        // ── Address

        [MapperIgnoreSource(nameof(Address.Customer))]
        public partial AddressDto ToAddressDto(Address address);

        public partial List<AddressDto> ToAddressDtoList(List<Address> addresses);

        // ── Snapshot 

        public partial Customer ToCustomerSanpDto(CustomerSnapshot snapshot);
        public partial List<Customer> ToCustomerSanpDtolist(List<CustomerSnapshot> snapshots);

        public partial Address ToAddressSnapDto(AddressSnapshot snapshot);
        public partial List<Address> ToAddressSnapDtoList(List<AddressSnapshot> snapshots);


        [MapperIgnoreTarget(nameof(CustomerHistoryDto.ValidFrom))]
        [MapperIgnoreTarget(nameof(CustomerHistoryDto.ValidTo))]
        [MapperIgnoreTarget(nameof(CustomerHistoryDto.Operation))]
        public partial CustomerHistoryDto ToCustomerHistoryDto(Customer customer);


        [MapperIgnoreTarget(nameof(AddressHistoryDto.ValidFrom))]
        [MapperIgnoreTarget(nameof(AddressHistoryDto.ValidTo))]
        [MapperIgnoreTarget(nameof(AddressHistoryDto.Operation))]
        [MapperIgnoreSource(nameof(Address.Customer))]
        public partial AddressHistoryDto ToAddressHistoryDto(Address address);

        // ── MongoDB:Snapshot 

        [MapProperty(nameof(CustomerSnapshot.CustomerId), nameof(CustomerHistoryDto.Id))]
        [MapperIgnoreTarget(nameof(CustomerHistoryDto.ValidTo))]
        public partial CustomerHistoryDto ToCustomerHistoryDto(CustomerSnapshot snapshot);

        public partial List<CustomerHistoryDto> ToCustomerHistoryDtoList(List<CustomerSnapshot> snapshots);



        [MapProperty(nameof(AddressSnapshot.AddressId), nameof(AddressHistoryDto.Id))]
        [MapperIgnoreTarget(nameof(AddressHistoryDto.ValidTo))]
        public partial AddressHistoryDto ToAddressHistoryDto(AddressSnapshot snapshot);

        public partial List<AddressHistoryDto> ToAddressHistoryDtoList(List<AddressSnapshot> snapshots);
    }
}
