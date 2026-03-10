using Customer_Mangment.Repository.Interfaces.MassageBroker;
using System.Reflection;

namespace Customer_Mangment.Extensions
{
    public static class MessagingExtensions
    {

        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddScoped<IMessagePublisher, MessagePublisher>();

            RegisterConsumers(services, typeof(Program).Assembly);

            return services;
        }


        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            services.AddScoped<IMessagePublisher, MessagePublisher>();

            foreach (var assembly in assemblies)
                RegisterConsumers(services, assembly);

            return services;
        }

        private static void RegisterConsumers(
            IServiceCollection services,
            Assembly assembly)
        {
            var consumerInterface = typeof(IMessageConsumer<>);

            var consumerTypes = assembly
                .GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == consumerInterface)
                    .Select(i => (ConcreteType: t, MessageType: i.GetGenericArguments()[0])));

            foreach (var (concreteType, messageType) in consumerTypes)
            {
                var iface = consumerInterface.MakeGenericType(messageType);
                services.AddScoped(iface, concreteType);

                var adapterType = typeof(ConsumerAdapter<>).MakeGenericType(messageType);
                services.AddScoped(adapterType);
            }
        }
    }
}
