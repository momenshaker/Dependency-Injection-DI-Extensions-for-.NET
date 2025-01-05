using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DependencyInjection.Extensions
{
    /// <summary>
    /// Provides extension methods for automatically registering services and applying decorators in the DI container.
    /// </summary>
    public static class ServiceRegistrationExtensions
    {
        /// <summary>
        /// Automatically registers services from the provided assembly based on interface/implementation matching.
        /// Each class in the assembly that implements an interface will be registered with that interface.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which services will be registered.</param>
        /// <param name="assembly">The assembly containing the services to be registered.</param>
        public static void AddServicesAutomatically(this IServiceCollection services, Assembly assembly)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            // Get all non-abstract classes in the assembly
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();

            foreach (var serviceType in serviceTypes)
            {
                // For each service type, get the interfaces it implements
                var interfaces = serviceType.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    // Register service type to its implemented interfaces
                    services.AddTransient(@interface, serviceType);
                }
            }
        }

        /// <summary>
        /// Registers multiple decorators for a single service type. 
        /// The decorators are applied in the order they are provided.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface the service implements.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the decorators will be registered.</param>
        /// <param name="decoratorTypes">An array of decorator types to be applied in order.</param>
        public static void AddDecorator<TInterface>(this IServiceCollection services, params Type[] decoratorTypes)
            where TInterface : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (decoratorTypes == null || decoratorTypes.Length == 0)
                throw new ArgumentException("At least one decorator type must be provided.", nameof(decoratorTypes));

            // Register a service with decorators applied in reverse order (innermost decorator first)
            services.AddTransient<TInterface>(serviceProvider =>
            {
                var originalService = serviceProvider.GetRequiredService<TInterface>();

                foreach (var decoratorType in decoratorTypes.Reverse())
                {
                    // Ensure the decorator type implements the interface
                    if (!typeof(TInterface).IsAssignableFrom(decoratorType))
                    {
                        throw new InvalidOperationException($"Decorator type {decoratorType.Name} must implement {typeof(TInterface).Name}.");
                    }

                    // Create a new decorator instance that wraps the original service
                    originalService = (TInterface)Activator.CreateInstance(decoratorType, originalService);
                }

                return originalService;
            });
        }
    }
}
