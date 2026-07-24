#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Events;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of <see cref="IDomainEventDispatcher"/> that resolves and invokes
/// domain event handlers from the dependency injection container.
/// This provides automatic handler discovery and invocation without manual subscription management.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventDispatcher"/> class.
    /// </summary>
    /// <param name="logger">Logger for tracking dispatcher operations.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or serviceProvider is null.</exception>
    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Publishes a domain event and automatically dispatches it to all registered handlers.
    /// Handlers are resolved from the dependency injection container using the current scope.
    /// </summary>
    /// <param name="event">The domain event to dispatch.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when event is null.</exception>
    public async Task DispatchAsync(DomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        await DispatchAsync(@event, _serviceProvider).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a domain event and automatically dispatches it to all registered handlers.
    /// This overload accepts a service provider for scenarios where the dispatcher needs explicit access.
    /// </summary>
    /// <param name="event">The domain event to dispatch.</param>
    /// <param name="serviceProvider">The service provider for resolving handlers.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when event or serviceProvider is null.</exception>
    public async Task DispatchAsync(DomainEvent @event, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _logger.LogInformation(
            "Dispatching event {EventName} (ID: {EventId}) to handlers",
            @event.EventName,
            @event.EventId);

        // Find all handlers that implement IEventHandler<T> for this event type
        var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
        var handlerServices = serviceProvider.GetServices(handlerType);

        if (!handlerServices.Any())
        {
            _logger.LogDebug(
                "No handlers registered for event type {EventType}",
                @event.GetType().Name);
            return;
        }

        _logger.LogInformation(
            "Found {HandlerCount} handler(s) for event type {EventType}",
            handlerServices.Count(),
            @event.GetType().Name);

        // Invoke each handler asynchronously
        var tasks = handlerServices.Select(async handlerService =>
        {
            try
            {
                var handleMethod = handlerService.GetType().GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = (Task)handleMethod.Invoke(handlerService, new object[] { @event });
                    await task.ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error invoking handler {HandlerType} for event {EventName} (ID: {EventId})",
                    handlerService.GetType().Name,
                    @event.EventName,
                    @event.EventId);
                throw;
            }
        }).ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _logger.LogInformation(
            "Event {EventName} (ID: {EventId}) dispatched successfully to {HandlerCount} handler(s)",
            @event.EventName,
            @event.EventId,
            handlerServices.Count());
    }

    /// <summary>
    /// Publishes a domain event and automatically dispatches it to all registered handlers.
    /// This overload accepts a service scope for scenarios where scoped services are needed.
    /// </summary>
    /// <param name="event">The domain event to dispatch.</param>
    /// <param name="scope">The service scope containing the service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when event or scope is null.</exception>
    public async Task DispatchAsync(DomainEvent @event, IServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(scope);

        await DispatchAsync(@event, scope.ServiceProvider).ConfigureAwait(false);
    }
}