using Customer_Mangment.Model.Events;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Services.AppMediator;
using FluentValidation;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.RabbitMQ;

namespace Customer_Mangment.Extensions;

public static class WolverineExtensions
{
    public static IHostBuilder AddWolverineMessaging(
        this IHostBuilder host,
        IConfiguration configuration)
    {
        host.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(IAssmblyMarker).Assembly);

            opts.UseFluentValidation();
            opts.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);


            var rabbitConnectionString = configuration.GetConnectionString("rabbitmq");
            if (rabbitConnectionString != null)
            {
                var uri = new Uri(rabbitConnectionString);
                opts.UseRabbitMq(rmq =>
                {
                    rmq.HostName = uri.Host;
                    rmq.Port = uri.Port > 0 ? uri.Port : 5672;
                    rmq.UserName = uri.UserInfo.Split(':')[0];
                    rmq.Password = Uri.UnescapeDataString(uri.UserInfo.Split(':')[1]);
                })
                .AutoProvision()
                .AutoPurgeOnStartup();
            }
            else
            {
                var rabbit = configuration.GetSection("RabbitMQ");
                opts.UseRabbitMq(rmq =>
                {
                    rmq.HostName = rabbit["Host"]!;
                    rmq.Port = 5672;
                    rmq.UserName = rabbit["Username"]!;
                    rmq.Password = rabbit["Password"]!;
                }).AutoProvision()
                  .AutoPurgeOnStartup();
            }

            opts.Policies.OnException<Exception>()
                .RetryWithCooldown(
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(5));

            opts.PublishMessage<CustomerSnapshotMessage>()
                .ToRabbitQueue("customer-snapshots");

            opts.ListenToRabbitQueue("customer-snapshots")
                .CircuitBreaker(cb =>
                {
                    cb.PauseTime = TimeSpan.FromMinutes(5);
                    cb.FailurePercentageThreshold = 15;
                });

            opts.PublishMessage<AddressSnapshotMessage>()
                .ToRabbitQueue("address-snapshots");

            opts.ListenToRabbitQueue("address-snapshots")
                .CircuitBreaker(cb =>
                {
                    cb.PauseTime = TimeSpan.FromMinutes(5);
                    cb.FailurePercentageThreshold = 15;
                });
        });

        return host;
    }

    public static IServiceCollection AddWolverineServices(
        this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, AppDispatcher>();
        services.AddValidatorsFromAssembly(typeof(IAssmblyMarker).Assembly);

        return services;
    }
}