using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.GraphQL.Schema.Types
{
    public sealed class CustomerType : ObjectType<Customer>
    {
        protected override void Configure(IObjectTypeDescriptor<Customer> descriptor)
        {
            descriptor.Description("A customer record.");

            descriptor.Field(c => c.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(c => c.Name).Type<NonNullType<StringType>>();
            descriptor.Field(c => c.Mobile).Type<NonNullType<StringType>>();
            descriptor.Field(c => c.CreatedBy).Type<NonNullType<StringType>>();
            descriptor.Field(c => c.UpdatedBy).Type<NonNullType<StringType>>();
            descriptor.Field(c => c.Addresses).Type<NonNullType<ListType<NonNullType<AddressType>>>>();

            descriptor.Ignore(c => c.IsDeleted);
            descriptor.Ignore(c => c.TenantId);
        }
    }
}
