using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Model.Events
{
    public sealed record CustomerCreatedEvent(Customer Customer);
    public sealed record CustomerUpdatedEvent(Customer Customer);
    public sealed record CustomerDeletedEvent(Customer Customer);

    public sealed record AddressCreatedEvent(Address Address);
    public sealed record AddressUpdatedEvent(Address Address);
    public sealed record AddressDeletedEvent(Address Address);
}
