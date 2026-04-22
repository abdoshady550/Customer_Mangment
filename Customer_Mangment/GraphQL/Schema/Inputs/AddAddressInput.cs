using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.GraphQL.Schema.Inputs
{
    public sealed record AddAddressInput(Guid CustomerId, AdressType Type, string Value);

}
