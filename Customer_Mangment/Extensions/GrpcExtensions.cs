using Customer_Mangment.Contracts.Grpc;

namespace Customer_Mangment.Extensions;

public static class GrpcExtensions
{
    public static IServiceCollection AddGrpcClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var identityAddress = configuration["Auth:Authority"]
            ?? throw new InvalidOperationException("Auth:Authority required.");

        services.AddGrpcClient<IdentityGrpcService.IdentityGrpcServiceClient>(o =>
        {
            o.Address = new Uri(identityAddress);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        return services;
    }
}