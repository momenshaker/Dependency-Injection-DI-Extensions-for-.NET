using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{ 
    /// <summary>
  /// Registers the AddSessionCleanUp service with dependency injection.
  /// </summary>
  /// <param name="services">The IServiceCollection to add services to.</param>
  /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddSessionCleanUp(this IServiceCollection services)
    {
        // Register TaskSchedulerWithRetry as a singleton or transient service based on preference
        services.AddSingleton<ISessionServiceCleaner, SessionServiceCleaner>();

        return services;
    }
}
