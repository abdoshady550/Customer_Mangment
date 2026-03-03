using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;

namespace Customer_Mangment.CQRS.Customers.Mappers
{
    public interface ICustomerMapper
    {
        // ── Customer ──────────────────────────────────────────────────────────
        CustomerDto ToCustomerDto(Customer customer);
        List<CustomerDto> ToCustomerDtoList(List<Customer> customers);

        // ── Address ───────────────────────────────────────────────────────────
        AddressDto ToAddressDto(Address address);
        List<AddressDto> ToAddressDtoList(List<Address> addresses);

        // ── Snapshot → Entity (existing) ──────────────────────────────────────
        Customer ToCustomerSanpDto(CustomerSnapshot snapshot);
        List<Customer> ToCustomerSanpDtolist(List<CustomerSnapshot> snapshots);
        Address ToAddressSnapDto(AddressSnapshot snapshot);
        List<Address> ToAddressSnapDtoList(List<AddressSnapshot> snapshots);

        // ── Single-entity history mapping (used by SqlHistoryService with `with`) ──
        CustomerHistoryDto ToCustomerHistoryDto(Customer customer);
        AddressHistoryDto ToAddressHistoryDto(Address address);

        // ── MongoDB snapshot history ───────────────────────────────────────────
        CustomerHistoryDto ToCustomerHistoryDto(CustomerSnapshot snapshot);
        List<CustomerHistoryDto> ToCustomerHistoryDtoList(List<CustomerSnapshot> snapshots);
        AddressHistoryDto ToAddressHistoryDto(AddressSnapshot snapshot);
        List<AddressHistoryDto> ToAddressHistoryDtoList(List<AddressSnapshot> snapshots);
    }
}
