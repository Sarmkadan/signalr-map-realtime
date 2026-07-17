#nullable enable
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

    // Registered as a singleton, so subscription state is shared across requests and
    // hub invocations; use a concurrent map with immutable handler lists so Subscribe/
    // Unsubscribe can race safely with PublishAsync.
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();

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

        // Wrap each handler so that both synchronous throws and faulted returned tasks
        // are caught and logged; otherwise Task.WhenAll would rethrow out of PublishAsync
        // and one failing subscriber would break the publisher and skip error isolation.
        var tasks = handlers.Select(async handler =>
        {
            try
            {
                if (handler is EventHandler<T> typedHandler)
                {
                    await typedHandler(@event).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error handling event {EventName} (ID: {EventId}) in handler {HandlerType}",
                    @event.EventName,
                    @event.EventId,
                    handler.GetType().Name);
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

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

        // Copy-on-write: publishers iterate the list without locking, so never mutate
        // a list that may be concurrently enumerated.
        _subscribers.AddOrUpdate(
            eventType,
            _ => new List<Delegate> { handler },
            (_, existing) =>
            {
                var copy = new List<Delegate>(existing) { handler };
                return copy;
            });

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
            // Copy-on-write removal for the same reason as Subscribe.
            var copy = new List<Delegate>(handlers);
            if (copy.Remove(handler))
            {
                _subscribers[eventType] = copy;
                _logger.LogInformation("Handler unsubscribed from event type {EventType}", eventType.Name);
            }
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
