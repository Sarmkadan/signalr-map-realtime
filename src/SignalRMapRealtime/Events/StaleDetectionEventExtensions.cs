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
    /// This sets up the domain event dispatcher and SignalR notification service.
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

        // Register the domain event dispatcher
        services.AddDomainEventDispatcher();

        // Register the stale detection service
        services.AddVehicleStaleDetectionService();

        return services;
    }

    /// <summary>
    /// Configures the domain event dispatcher to automatically handle stale detection events.
    /// This should be called during application startup to wire up the event dispatcher.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection ConfigureStaleDetectionEventDispatching(this IServiceCollection services)
    {
        // This method is intentionally left for future extensibility
        // The actual event dispatching is handled automatically by the DomainEventDispatcher
        return services;
    }

    /// <summary>
    /// Creates a service scope and dispatches domain events using the domain event dispatcher.
    /// This is a convenience method for application startup configuration.
    /// </summary>
    /// <param name="eventDispatcher">The domain event dispatcher.</param>
    /// <param name="event">The domain event to dispatch.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DispatchEventAsync(
        this IDomainEventDispatcher eventDispatcher,
        DomainEvent @event,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(eventDispatcher);
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        await eventDispatcher.DispatchAsync(@event, serviceProvider).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a service scope and dispatches domain events using the domain event dispatcher.
    /// This is a convenience method for application startup configuration.
    /// </summary>
    /// <param name="eventDispatcher">The domain event dispatcher.</param>
    /// <param name="event">The domain event to dispatch.</param>
    /// <param name="scope">The service scope containing the service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DispatchEventAsync(
        this IDomainEventDispatcher eventDispatcher,
        DomainEvent @event,
        IServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(eventDispatcher);
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(scope);

        await eventDispatcher.DispatchAsync(@event, scope).ConfigureAwait(false);
    }
}