namespace Customer_Mangment.GraphQL.Schema.Inputs
{
    public sealed record CreateCustomerInput(
        string Name,
        string Mobile,
        List<CreateAddressInput>? Addresses);

}
