using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Model.Events
{
    public sealed record CustomerSnapshotMessage(Guid CustomerId,
                                                 string Name,
                                                 string Mobile,
                                                 string CreatedBy,
                                                 string UpdatedBy,
                                                 bool IsDeleted,
                                                 string Operation) : ISnapshotMessage;

    public sealed record AddressSnapshotMessage(Guid AddressId,
                                                Guid CustomerId,
                                                AdressType Type,
                                                string Value,
                                                string Operation) : ISnapshotMessage;
}
