using Customer_Mangment.IdentityServer.CQRS;
using FluentValidation;
using Wolverine;
using Wolverine.FluentValidation;
namespace Customer_Mangment.IdentityServer.Extensions
{
    public static class WolverineExtensions
    {

        public static IHostBuilder AddIdentityServerWolverine(this IHostBuilder host)
        {
            host.UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(Program).Assembly);

                opts.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
            });

            return host;
        }


        public static IServiceCollection AddIdentityServerWolverineServices(
            this IServiceCollection services)
        {
            services.AddScoped<IIdentityDispatcher, IdentityDispatcher>();

            services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            return services;
        }
    }
}