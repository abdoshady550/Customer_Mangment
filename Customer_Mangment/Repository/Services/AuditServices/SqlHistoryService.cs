using Customer_Mangment.CQRS.Customers.DTOS;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.Audit;
using Microsoft.EntityFrameworkCore;

namespace Customer_Mangment.Repository.Services.AuditServices
{
    public sealed class SqlHistoryService(
        IGenericRepo<Customer> customerRepo,
        IGenericRepo<Address> addressRepo,
        ICustomerMapper mapper) : IHistoryService
    {
        public async Task<List<CustomerHistoryDto>> GetCustomerHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var rows = await customerRepo
                .TemporalAll()
                .Where(c => c.Id == customerId)
                .SelectAsync(c => new
                {
                    Customer = c,
                    ValidFrom = EF.Property<DateTime>(c, "ValidFrom"),
                    ValidTo = EF.Property<DateTime>(c, "ValidTo")
                }, ct);

            return rows
                .Select(r => mapper.ToCustomerHistoryDto(r.Customer) with
                {
                    ValidFrom = r.ValidFrom,
                    ValidTo = r.ValidTo,
                    Operation = null
                })
                .ToList();
        }

        public async Task<List<AddressHistoryDto>> GetAddressHistoryAsync(Guid customerId, CancellationToken ct = default)
        {
            var rows = await addressRepo
                .TemporalAll()
                .Where(a => a.CustomerId == customerId)
                .SelectAsync(a => new
                {
                    Address = a,
                    ValidFrom = EF.Property<DateTime>(a, "ValidFrom"),
                    ValidTo = EF.Property<DateTime>(a, "ValidTo")
                }, ct);

            return rows
                .Select(r => mapper.ToAddressHistoryDto(r.Address) with
                {
                    ValidFrom = r.ValidFrom,
                    ValidTo = r.ValidTo,
                    Operation = null
                })
                .ToList();
        }
    }
}
