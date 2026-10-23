using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GLB.TechnicalTest.CBTW.BookSearch.Application.Setup
{
    public static class DependencyInjectionSetup
    {
        public static IServiceCollection AddApplicationSetup(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjectionSetup).Assembly;

            services.AddScoped<ISender, Sender>();
            services.AddRequestHandlersFromAssembly(assembly);

            return services;
        }

        private static IServiceCollection AddRequestHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var handlerInterface = typeof(IRequestHandler<,>);

            var registrations =
                from type in assembly.GetTypes()
                where type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                from @interface in type.GetInterfaces()
                where @interface.IsGenericType && @interface.GetGenericTypeDefinition() == handlerInterface
                select (Service: @interface, Implementation: type);

            foreach (var (service, implementation) in registrations)
            {
                services.AddScoped(service, implementation);
            }

            return services;
        }
    }
}
