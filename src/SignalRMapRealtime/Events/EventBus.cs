// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Handler delegate for domain events.
/// Allows subscribers to handle specific event types.
/// </summary>
public delegate Task EventHandler<in T>(T @event) where T : DomainEvent;

/// <summary>
/// Event bus service for publish-subscribe domain event handling.
/// Enables decoupling of business logic through event-driven architecture.
/// Subscribers can react to domain events asynchronously without tight coupling.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes a domain event to all registered subscribers.
    /// </summary>
    Task PublishAsync<T>(T @event) where T : DomainEvent;

    /// <summary>
    /// Subscribes to a specific type of domain event.
    /// Handler will be called whenever an event of this type is published.
    /// </summary>
    void Subscribe<T>(EventHandler<T> handler) where T : DomainEvent;

    /// <summary>
    /// Unsubscribes from a specific domain event.
    /// </summary>
    void Unsubscribe<T>(EventHandler<T> handler) where T : DomainEvent;
}

/// <summary>
/// In-memory implementation of the event bus.
/// Suitable for single-instance deployments and testing.
/// For distributed systems, consider replacing with a message queue (RabbitMQ, Azure Service Bus, etc.).
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// Handles exceptions from individual handlers to prevent cascading failures.
    /// </summary>
    public async Task PublishAsync<T>(T @event) where T : DomainEvent
    {
        var eventType = typeof(T);

        if (!_subscribers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug("No handlers registered for event type {EventType}", eventType.Name);
            return;
        }

        _logger.LogInformation(
            "Publishing event {EventName} (ID: {EventId}) with {HandlerCount} handlers",
            @event.EventName,
            @event.EventId,
            handlers.Count);

        var tasks = handlers.Select(handler =>
        {
            try
            {
                if (handler is EventHandler<T> typedHandler)
                {
                    return typedHandler(@event);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error handling event {EventName} (ID: {EventId}) in handler {HandlerType}",
                    @event.EventName,
                    @event.EventId,
                    handler.GetType().Name);
                return Task.CompletedTask;
            }
        });

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Event {EventName} (ID: {EventId}) published successfully",
            @event.EventName,
            @event.EventId);
    }

    /// <summary>
    /// Registers a handler for a specific event type.
    /// </summary>
    public void Subscribe<T>(EventHandler<T> handler) where T : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(T);

        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }

        _subscribers[eventType].Add(handler);
        _logger.LogInformation("Handler subscribed for event type {EventType}", eventType.Name);
    }

    /// <summary>
    /// Unsubscribes a handler from a specific event type.
    /// </summary>
    public void Unsubscribe<T>(EventHandler<T> handler) where T : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(T);

        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
            _logger.LogInformation("Handler unsubscribed from event type {EventType}", eventType.Name);
        }
    }
}

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
    /// Subscribes a handler to an event when using dependency injection.
    /// </summary>
    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : DomainEvent
        where THandler : class
    {
        services.AddScoped<THandler>();
        return services;
    }
}
