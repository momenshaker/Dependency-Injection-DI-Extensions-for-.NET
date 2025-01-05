using DependencyInjection.Extensions.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace DependencyInjection.Extensions
{
    /// <summary>
    /// Provides advanced dependency injection (DI) extension methods for customizing service lifetimes.
    /// </summary>
    public static class ServiceLifetimeExtensions
    {
        // Thread-safe storage for per-session services
        private static readonly ConcurrentDictionary<string, object> _userSessionServices = new ConcurrentDictionary<string, object>();


        /// <summary>
        /// Registers the specified service type with a singleton lifetime, meaning the instance is shared across all user sessions.
        /// </summary>
        /// <typeparam name="TService">The service type to be registered.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        public static void AddPerUser<TService>(this IServiceCollection services)
            where TService : class
        {
            services.AddSingleton<TService>(serviceProvider =>
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var sessionId = httpContextAccessor.HttpContext?.Session?.Id;

                // If session ID is not found, throw an exception as session is required
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new InvalidOperationException("User session is missing or has expired.");
                }

                // Retrieve the service instance for the current session or create a new one if it doesn't exist
                if (!_userSessionServices.TryGetValue(sessionId, out var serviceInstance))
                {
                    serviceInstance = ActivatorUtilities.CreateInstance<TService>(serviceProvider);
                    _userSessionServices[sessionId] = serviceInstance;
                }
                if (serviceInstance is IDisposable disposableService)
                {
                    // Get the SessionServiceCleaner to register the service for cleanup
                    var sessionServiceCleaner = serviceProvider.GetRequiredService<ISessionServiceCleaner>();
                    sessionServiceCleaner.RegisterServiceForCleanup(sessionId, disposableService);
                }
                else
                {
                    throw new InvalidOperationException($"Service {typeof(TService).FullName} does not implement IDisposable.");
                }
               
                // Return the cached service instance for the current session
                return (TService)serviceInstance;
            });
        }

        /// <summary>
        /// Registers all services in the provided assembly that have the <see cref="RegisterServiceAttribute"/> applied to them.
        /// This allows automatic self-registration of services without needing explicit calls to AddWithCustomAttribute for each service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="assembly">The assembly that contains the service classes to be registered.</param>
        public static void AddServicesWithCustomAttributesFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<RegisterServiceAttribute>() != null)
                .ToList();

            foreach (var serviceType in serviceTypes)
            {
                // Get the RegisterServiceAttribute applied to the service class
                var customAttribute = serviceType.GetCustomAttribute<RegisterServiceAttribute>();

                // If the attribute is found, register the service with the specified lifetime
                if (customAttribute != null)
                {
                    RegisterServiceByLifetime(services, serviceType, customAttribute.Lifetime);
                }
                else
                {
                    // If no attribute is found, register the service with Transient by default
                    services.AddTransient(serviceType);
                }
            }
        }

        /// <summary>
        /// Registers services with custom lifetimes, considering additional options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the services in.</param>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="lifetime">The lifetime for the service.</param>
        private static void RegisterServiceByLifetime(IServiceCollection services, Type serviceType, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    services.AddTransient(serviceType);
                    break;

                case ServiceLifetime.Scoped:
                    services.AddScoped(serviceType);
                    break;

                case ServiceLifetime.Singleton:
                    services.AddSingleton(serviceType);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported service lifetime: {lifetime}");
            }
        }

        /// <summary>
        /// Registers a service conditionally based on a provided predicate (e.g., environment, configuration).
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        /// <param name="predicate">The condition to check before registering the service.</param>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        public static void AddServiceIf<TService>(this IServiceCollection services, Func<bool> predicate, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
        {
            if (predicate())
            {
                RegisterServiceByLifetime(services, typeof(TService), lifetime);
            }
        }

        /// <summary>
        /// Registers multiple services of the same type with different configurations.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        /// <param name="configurations">An array of configurations to apply to the service instances.</param>
        public static void AddMultipleServices<TService>(this IServiceCollection services, params Action<IServiceCollection>[] configurations)
            where TService : class
        {
            foreach (var config in configurations)
            {
                config(services);
            }
        }

        /// <summary>
        /// Attempts to register a service only if it is not already registered.
        /// This avoids redundant registrations that could lead to issues with conflicting services.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        public static void AddServiceOnce<TService>(this IServiceCollection services)
            where TService : class
        {
            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TService));

            if (serviceDescriptor == null)
            {
                services.AddTransient<TService>();
            }
            else
            {
                // Optionally log a message or throw an exception for redundant registration
                Console.WriteLine($"{typeof(TService).Name} is already registered.");
            }
        }
    }
}
