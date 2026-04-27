using Customer_Mangment.GraphQL.Schema.Mutations;
using Customer_Mangment.GraphQL.Schema.Queries;
using Customer_Mangment.GraphQL.Schema.Types;
using Customer_Mangment.Model.Results;
namespace Customer_Mangment.Extensions
{
    public static class GraphQL
    {
        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            services.AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                .AddTypeExtension<CustomerQuery>()
                .AddMutationType(d => d.Name("Mutation"))
                .AddTypeExtension<CustomerMutation>()
                .AddTypeExtension<AddressMutation>()
                .AddType<CustomerType>()
                .AddType<AddressType>()
                .BindRuntimeType<Updated, AnyType>()
                .BindRuntimeType<Deleted, AnyType>()
                .BindRuntimeType<Created, AnyType>()
                .AddAuthorization()
                .AddMaxExecutionDepthRule(5)
                .DisableIntrospection()
                .ModifyRequestOptions(o => o.IncludeExceptionDetails = true)
                .ModifyCostOptions(options =>
                {
                    options.MaxFieldCost = 1500;
                    options.MaxTypeCost = 1500;
                    options.EnforceCostLimits = true;
                    options.ApplyCostDefaults = true;
                    options.DefaultResolverCost = 10.0;
                });
            return services;
        }
    }
}
