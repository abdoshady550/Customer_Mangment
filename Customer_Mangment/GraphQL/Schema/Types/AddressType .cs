using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.GraphQL.Schema.Types
{
    public sealed class AddressType : ObjectType<Address>
    {
        protected override void Configure(IObjectTypeDescriptor<Address> descriptor)
        {
            descriptor.Description("A customer address.");

            descriptor.Field(a => a.Id).Type<NonNullType<UuidType>>();
            descriptor.Field(a => a.CustomerId).Type<NonNullType<UuidType>>();
            descriptor.Field(a => a.Type).Type<NonNullType<EnumType<AdressType>>>();
            descriptor.Field(a => a.Value).Type<NonNullType<StringType>>();

            descriptor.Ignore(a => a.Customer);
            descriptor.Ignore(a => a.TenantId);

        }
    }
}

