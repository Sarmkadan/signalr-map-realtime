#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Extension methods for registering <see cref="LocationOptions"/> with the DI container.
/// </summary>
public static class LocationOptionsExtensions
{
    /// <summary>
    /// Binds <see cref="LocationOptions"/> to the <c>LocationOptions</c> section of the
    /// application configuration and registers it for DI.
    /// </summary>
    /// <param name="services">The service collection to add the options to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The original <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddLocationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LocationOptions>()
            .Bind(configuration.GetSection(LocationOptions.SectionName))
            .ValidateDataAnnotations();

        return services;
    }
}
