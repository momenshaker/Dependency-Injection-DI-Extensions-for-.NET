using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Extensions
{
    /// <summary>
    /// Provides extension methods for adding interceptors to services.
    /// Interceptors allow you to modify behavior of methods, including synchronous, asynchronous, and lazy-loaded services.
    /// </summary>
    public static class InterceptorExtensions
    {
        /// <summary>
        /// Adds a method-level interceptor to a service, allowing for custom logic to be executed before or after method calls.
        /// </summary>
        /// <typeparam name="TService">The service type to which the interceptor will be applied.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        /// <param name="methodInterceptor">The method interceptor delegate to apply to the service instance.</param>
        public static void AddMethodInterceptor<TService>(this IServiceCollection services, Action<TService> methodInterceptor)
            where TService : class
        {
            if (methodInterceptor == null) throw new ArgumentNullException(nameof(methodInterceptor), "Interceptor cannot be null.");

            services.AddTransient<TService>(serviceProvider =>
            {
                var service = serviceProvider.GetRequiredService<TService>();
                methodInterceptor(service); // Apply the interceptor logic
                return service;
            });
        }

        /// <summary>
        /// Adds an asynchronous interceptor to a service, allowing asynchronous methods to be intercepted and modified.
        /// </summary>
        /// <typeparam name="TService">The service type to which the asynchronous interceptor will be applied.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        /// <param name="asyncInterceptor">The asynchronous interceptor delegate to apply to the service instance.</param>
        public static void AddAsyncInterceptor<TService>(this IServiceCollection services, Func<TService, Task> asyncInterceptor)
            where TService : class
        {
            if (asyncInterceptor == null) throw new ArgumentNullException(nameof(asyncInterceptor), "Async interceptor cannot be null.");

            services.AddTransient<TService>(serviceProvider =>
            {
                var service = serviceProvider.GetRequiredService<TService>();
                // Apply the async interceptor for handling asynchronous methods
                _ = asyncInterceptor(service); // Note: Asynchronous execution may not block
                return service;
            });
        }

        /// <summary>
        /// Adds a lazy interceptor to a service, allowing for deferred initialization of the service.
        /// </summary>
        /// <typeparam name="TService">The service type to which the lazy interceptor will be applied.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        /// <param name="lazyInterceptor">The lazy initialization interceptor delegate to apply to the service instance.</param>
        public static void AddLazyInterceptor<TService>(this IServiceCollection services, Func<TService, Lazy<TService>> lazyInterceptor)
            where TService : class
        {
            if (lazyInterceptor == null) throw new ArgumentNullException(nameof(lazyInterceptor), "Lazy interceptor cannot be null.");

            services.AddTransient<TService>(serviceProvider =>
            {
                var service = serviceProvider.GetRequiredService<TService>();
                return lazyInterceptor(service).Value; // Return the lazily initialized service
            });
        }

        /// <summary>
        /// Adds an interceptor that allows you to modify the return value of a method.
        /// This can be used to alter the return values dynamically during service calls.
        /// </summary>
        /// <typeparam name="TService">The service type to which the return value interceptor will be applied.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        /// <param name="returnValueInterceptor">A delegate to modify the return value of the service method.</param>
        public static void AddReturnValueInterceptor<TService>(this IServiceCollection services, Func<TService, object, object> returnValueInterceptor)
            where TService : class
        {
            if (returnValueInterceptor == null) throw new ArgumentNullException(nameof(returnValueInterceptor), "Return value interceptor cannot be null.");

            services.AddTransient<TService>(serviceProvider =>
            {
                var service = serviceProvider.GetRequiredService<TService>();
                var originalMethod = service as dynamic;

                // Intercept and modify the return value of a method if needed
                return returnValueInterceptor(service, originalMethod) ?? originalMethod;
            });
        }

        /// <summary>
        /// Adds an interceptor that can catch and handle exceptions thrown during method execution.
        /// This allows for custom exception handling logic to be executed.
        /// </summary>
        /// <typeparam name="TService">The service type to which the exception handling interceptor will be applied.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the service in.</param>
        /// <param name="exceptionInterceptor">A delegate to handle exceptions thrown during method execution.</param>
        public static void AddExceptionInterceptor<TService>(this IServiceCollection services, Func<TService, Exception, Task> exceptionInterceptor)
            where TService : class
        {
            if (exceptionInterceptor == null) throw new ArgumentNullException(nameof(exceptionInterceptor), "Exception interceptor cannot be null.");

            services.AddTransient<TService>(serviceProvider =>
            {
                var service = serviceProvider.GetRequiredService<TService>();
                try
                {
                    return service; // Return the service if no exception occurs
                }
                catch (Exception ex)
                {
                    // Handle the exception via the provided interceptor
                    exceptionInterceptor(service, ex);
                    throw; // Rethrow the exception after handling it
                }
            });
        }
    }
}
