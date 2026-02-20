using AutoMapper;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.CQRS.Customers.Mappers
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDto>();

            CreateMap<Address, AddressDto>();

            CreateMap<CustomerHistory, CustomerHistoryDto>();
        }
    }
}
