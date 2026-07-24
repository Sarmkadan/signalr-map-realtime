#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering the vehicle stale detection service with dependency injection.
/// </summary>
public static class VehicleStaleDetectionServiceExtensions
{
    /// <summary>
    /// Adds the vehicle stale detection service to the service collection.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddVehicleStaleDetectionService(this IServiceCollection services)
    {
        services.AddHostedService<VehicleStaleDetectionService>();
        services.AddScoped<VehicleStaleDetectionService>(provider =>
            provider.GetRequiredService<IHostedService>() as VehicleStaleDetectionService ??
            throw new InvalidOperationException("VehicleStaleDetectionService not found"));

        return services;
    }
}