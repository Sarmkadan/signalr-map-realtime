#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Events;

/// <summary>
/// Marker interface for domain event handlers.
/// Implement this interface on classes that handle specific domain events.
/// </summary>
/// <typeparam name="TEvent">The type of domain event this handler processes.</typeparam>
public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="event">The domain event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TEvent @event);
}