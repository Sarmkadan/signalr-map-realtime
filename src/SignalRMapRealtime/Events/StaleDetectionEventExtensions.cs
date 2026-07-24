#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Events;

using Microsoft.Extensions.DependencyInjection;
using SignalRMapRealtime.Services;

/// <summary>
/// Extension methods for registering stale detection event handlers with the dependency injection container.
/// </summary>
public static class StaleDetectionEventExtensions
{
    /// <summary>
    /// Registers the stale detection event handlers with the dependency injection container.
    /// This sets up the event bus subscribers for VehicleStaleEvent and VehicleActiveEvent.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddStaleDetectionEventHandlers(this IServiceCollection services)
    {
        // Register the SignalR notification service
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

        // Register the event handlers
        services.AddScoped<VehicleStaleEventHandler>();
        services.AddScoped<VehicleActiveEventHandler>();

        // Register the stale detection service
        services.AddVehicleStaleDetectionService();

        return services;
    }

    /// <summary>
    /// Subscribes the stale detection event handlers to the event bus.
    /// This should be called during application startup to wire up the event handlers.
    /// </summary>
    /// <param name="eventBus">The event bus to subscribe to.</param>
    /// <param name="serviceProvider">The service provider for resolving handler instances.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SubscribeStaleDetectionEventHandlersAsync(this IEventBus eventBus, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        // Create handler instances using the service provider
        using var scope = serviceProvider.CreateScope();
        var staleHandler = scope.ServiceProvider.GetRequiredService<VehicleStaleEventHandler>();
        var activeHandler = scope.ServiceProvider.GetRequiredService<VehicleActiveEventHandler>();

        // Subscribe to events
        eventBus.Subscribe<VehicleStaleEvent>(staleHandler.HandleAsync);
        eventBus.Subscribe<VehicleActiveEvent>(activeHandler.HandleAsync);
    }
}