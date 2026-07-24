#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Extension methods for registering the event bus in dependency injection.
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Adds the in-memory event bus to the service collection.
    /// </summary>
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }

    /// <summary>
    /// Adds the domain event dispatcher to the service collection.
    /// </summary>
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }

    /// <summary>
    /// Subscribes a handler to an event when using dependency injection.
    /// </summary>
    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services) where TEvent : DomainEvent where THandler : class
    {
        services.AddScoped<THandler>();
        return services;
    }
}