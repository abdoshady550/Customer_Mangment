using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.GraphQL.Schema.Inputs
{
    public sealed record CreateAddressInput(AdressType Type, string Value);
}
