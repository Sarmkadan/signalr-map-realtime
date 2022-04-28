# IEventBus

A lightweight, in-process event bus for decoupled publish/subscribe messaging within a single application domain. It enables components to communicate through events without direct dependencies, supporting both transient handler registration via DI and dynamic subscription at runtime.

## API

### `EventHandler<in T>`
```csharp
public delegate Task EventHandler<in T>(T @event);
```
Delegate representing an asynchronous event handler. The contravariant type parameter allows a handler for a base event type to handle derived event types.

**Parameters**
- `@event` — The event instance to process.

**Returns**
- A `Task` representing the asynchronous handling operation.

**Throws**
- Any exception thrown by the handler implementation propagates to the caller of `PublishAsync`.

---

### `InMemoryEventBus`
```csharp
public class InMemoryEventBus : IEventBus
```
Default implementation of `IEventBus` that stores subscriptions in memory. Registered as a singleton in the DI container via `AddEventBus`. Not distributed; all subscribers execute in the same process.

---

### `PublishAsync<T>`
```csharp
public async Task PublishAsync<T>(T @event);
```
Publishes an event to all currently subscribed handlers for type `T`. Handlers are invoked sequentially in registration order. The method completes when all handlers have finished.

**Type Parameters**
- `T` — The event type.

**Parameters**
- `@event` — The event instance to publish. Must not be null.

**Returns**
- A `Task` that completes when all handlers have executed.

**Throws**
- `ArgumentNullException` — If `@event` is null.
- `InvalidOperationException` — If the event bus has been disposed.
- Any exception thrown by a handler; subsequent handlers are not invoked if a handler throws.

---

### `Subscribe<T>`
```csharp
public void Subscribe<T>(EventHandler<T> handler);
```
Registers a handler for events of type `T`. The handler will receive all future events published for `T` until unsubscribed or the bus is disposed.

**Type Parameters**
- `T` — The event type.

**Parameters**
- `handler` — The async handler to invoke. Must not be null.

**Throws**
- `ArgumentNullException` — If `handler` is null.
- `InvalidOperationException` — If the event bus has been disposed.

---

### `Unsubscribe<T>`
```csharp
public void Unsubscribe<T>(EventHandler<T> handler);
```
Removes a previously registered handler for events of type `T`. The handler will no longer receive events.

**Type Parameters**
- `T` — The event type.

**Parameters**
- `handler` — The exact handler instance to remove. Must not be null.

**Throws**
- `ArgumentNullException` — If `handler` is null.
- `InvalidOperationException` — If the event bus has been disposed.

---

### `AddEventBus`
```csharp
public static IServiceCollection AddEventBus(this IServiceCollection services);
```
Registers `InMemoryEventBus` as a singleton implementation of `IEventBus` in the DI container. Call during application startup.

**Parameters**
- `services` — The service collection to configure.

**Returns**
- The same `IServiceCollection` for chaining.

---

### `AddEventHandler<TEvent, THandler>`
```csharp
public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
    where THandler : class, EventHandler<TEvent>;
```
Registers a handler type `THandler` for event type `TEvent`. The handler is resolved from DI as a scoped service and automatically subscribed when the scope is created. Use this for handlers that require scoped dependencies (e.g., `DbContext`).

**Type Parameters**
- `TEvent` — The event type to handle.
- `THandler` — The handler implementation type.

**Parameters**
- `services` — The service collection to configure.

**Returns**
- The same `IServiceCollection` for chaining.

**Throws**
- `ArgumentException` — If `THandler` does not implement `EventHandler<TEvent>`.

## Usage

### Basic publish/subscribe with DI-registered handlers
```csharp
// Program.cs
builder.Services.AddEventBus();
builder.Services.AddEventHandler<UserRegisteredEvent, UserRegisteredHandler>();

// UserRegisteredHandler.cs
public class UserRegisteredHandler : EventHandler<UserRegisteredEvent>
{
    private readonly IEmailService _email;
    private readonly ILogger<UserRegisteredHandler> _log;

    public UserRegisteredHandler(IEmailService email, ILogger<UserRegisteredHandler> log)
    {
        _email = email;
        _log = log;
    }

    public async Task Handle(UserRegisteredEvent @event)
    {
        _log.LogInformation("User {UserId} registered", @event.UserId);
        await _email.SendWelcomeAsync(@event.Email);
    }
}

// Publishing from a controller or service
public class AccountController : ControllerBase
{
    private readonly IEventBus _bus;

    public AccountController(IEventBus bus) => _bus = bus;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = await _userService.CreateAsync(req);
        await _bus.PublishAsync(new UserRegisteredEvent(user.Id, user.Email));
        return Ok();
    }
}
```

### Dynamic runtime subscription for transient handlers
```csharp
public class RealTimeMapHub : Hub
{
    private readonly IEventBus _bus;
    private EventHandler<LocationUpdatedEvent>? _handler;

    public RealTimeMapHub(IEventBus bus) => _bus = bus;

    public async Task SubscribeToVehicle(string vehicleId)
    {
        _handler = async @event =>
        {
            if (@event.VehicleId == vehicleId)
                await Clients.Caller.SendAsync("LocationUpdated", @event);
        };
        _bus.Subscribe(_handler);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_handler is not null)
            _bus.Unsubscribe(_handler);
        await base.OnDisconnectedAsync(exception);
    }
}
```

## Notes

- **Thread safety**: `InMemoryEventBus` uses a concurrent dictionary for subscription storage. `Subscribe`, `Unsubscribe`, and `PublishAsync` are safe for concurrent calls. However, the list of handlers for a given event type is captured at the start of `PublishAsync`; handlers added or removed during publishing do not affect the current publish operation.
- **Handler execution order**: Handlers are invoked in the order they were subscribed. DI-registered handlers (via `AddEventHandler`) are subscribed when their scope is created, typically before any runtime subscriptions.
- **Exception behavior**: If a handler throws, `PublishAsync` stops invoking remaining handlers for that event and propagates the exception. Consider wrapping handler logic in try/catch or using a middleware pattern if you need guaranteed delivery to all handlers.
- **Memory leaks**: Always call `Unsubscribe` for dynamic subscriptions (e.g., in `IDisposable.Dispose` or SignalR `OnDisconnectedAsync`). Forgotten subscriptions keep the handler delegate alive, preventing garbage collection of the subscriber and its captured dependencies.
- **Scoped handlers**: Handlers registered via `AddEventHandler` are resolved per scope. In ASP.NET Core, a scope is created per request. In background services, create a scope manually (`IServiceScopeFactory.CreateScope`) before publishing events that should trigger scoped handlers.
- **Null events**: `PublishAsync` throws `ArgumentNullException` for null events. Use a non-nullable event type or validate before publishing.
- **Disposal**: `InMemoryEventBus` implements `IDisposable` (via `IEventBus` if extended). Disposing clears all subscriptions. The DI container disposes the singleton at application shutdown.
