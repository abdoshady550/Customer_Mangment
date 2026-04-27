using Customer_Mangment.CQRS.Customers.Addresses.DTOS;
using Customer_Mangment.CQRS.Customers.DTOS;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Customer_Mangment.O_Data
{
    public static class EdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<CustomerDto>("Customers");
            builder.EntitySet<AddressDto>("Addresses");
            return builder.GetEdmModel();
        }
    }
}
