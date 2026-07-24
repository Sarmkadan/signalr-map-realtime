#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Dispatches domain events to their registered handlers using dependency injection.
/// Provides automatic resolution of event handlers from the DI container,
/// eliminating the need for manual subscription management.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Publishes a domain event and automatically dispatches it to all registered handlers.
    /// Handlers are resolved from the dependency injection container and invoked asynchronously.
    /// </summary>
    /// <param name="event">The domain event to dispatch.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null.</exception>
    Task DispatchAsync(DomainEvent @event);

    /// <summary>
    /// Publishes a domain event and automatically dispatches it to all registered handlers.
    /// This overload accepts a service provider for scenarios where the dispatcher needs explicit access.
    /// </summary>
    /// <param name="event">The domain event to dispatch.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when event or serviceProvider is null.</exception>
    Task DispatchAsync(DomainEvent @event, IServiceProvider serviceProvider);

    /// <summary>
    /// Publishes a domain event and automatically dispatches it to all registered handlers.
    /// This overload accepts a service scope for scenarios where scoped services are needed.
    /// </summary>
    /// <param name="event">The domain event to dispatch.</param>
    /// <param name="scope">The service scope containing the service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when event or scope is null.</exception>
    Task DispatchAsync(DomainEvent @event, IServiceScope scope);
}