using AutoMapper;
using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Entities.History;

namespace Customer_Mangment.CQRS.Customers.Mappers
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {

            CreateMap<Customer, CustomerDto>();

            CreateMap<CustomerHistory, CustomerHistoryDto>();



            CreateMap<Address, AddressDto>();
            CreateMap<AddressHistory, AddressHistoryDto>();


            CreateMap<Address, AddressHistoryDto>();


        }
    }
}
