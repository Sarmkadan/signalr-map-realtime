# EventBus

Source: `src/SignalRMapRealtime/Events/EventBus.cs`

An in-process publish-subscribe bus for domain events. Publishers call `PublishAsync` with an event; every handler subscribed to that event's type is invoked and the publisher is awaited until they finish. Handler failures are caught and logged, so they never propagate to the publisher.

The file contains:

- `EventHandler<in T>`: the handler delegate.
- `IEventBus`: the public contract.
- `InMemoryEventBus`: the single in-memory implementation.

The bus is not distributed. Subscribers run in the same process, and subscriptions live only as long as the instance. The implementation's own comment says it is registered as a singleton, so subscriptions are shared across requests and hub invocations. This file does not register the service with the DI container.

## Delegate

### `EventHandler<in T>`

```csharp
public delegate Task EventHandler<in T>(T @event) where T : DomainEvent;
```

The signature of a domain event handler. `T` is constrained to `DomainEvent`, and the `in` modifier makes the type parameter contravariant.

**Parameters**
- `@event`: the event instance being handled.

**Returns**
- A `Task` that completes when the handler has finished its work.

Note that the name shadows `System.EventHandler<TEventArgs>` inside the `SignalRMapRealtime.Events` namespace. Code in that namespace refers to this delegate.

## Interface

### `IEventBus`

```csharp
public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : DomainEvent;
    void Subscribe<T>(EventHandler<T> handler) where T : DomainEvent;
    void Unsubscribe<T>(EventHandler<T> handler) where T : DomainEvent;
}
```

Depend on `IEventBus` in consumers and publishers. Use `InMemoryEventBus` as the concrete type.

### `PublishAsync<T>`

```csharp
Task PublishAsync<T>(T @event) where T : DomainEvent;
```

Delivers `@event` to every handler currently subscribed to `T`.

**Type Parameters**
- `T`: the event type. It is taken from the compile-time type of the argument (see [Dispatch by exact type](#dispatch-by-exact-type)).

**Parameters**
- `@event`: the event to publish.

**Returns**
- A `Task` that completes after all matching handlers have finished, successfully or not.

**Behavior**
- If no handlers are registered for `T`, the method logs at Debug level and returns.
- Otherwise, all handlers are started in registration order and run concurrently. `Task.WhenAll` waits for every one of them. There is no timeout and no cancellation token, so a slow handler delays the publisher.
- Each handler is wrapped in a `try`/`catch (Exception)`. A synchronous throw or a faulted task is logged at Error level, with the event name, event ID, and handler type. The remaining handlers still run, and `PublishAsync` does not throw because of them.
- The "published successfully" message is logged even when some handlers failed.

**Known limitation**
- `@event` is not validated. A null event with at least one subscriber throws `NullReferenceException` while the method builds its log message, before any handler runs. With no subscribers, a null event is accepted silently.

### `Subscribe<T>`

```csharp
void Subscribe<T>(EventHandler<T> handler) where T : DomainEvent;
```

Adds `handler` to the list of handlers for `T`.

**Type Parameters**
- `T`: the event type to listen for.

**Parameters**
- `handler`: the delegate to invoke. Must not be null, otherwise `ArgumentNullException` is thrown.

**Behavior**
- Adding is copy-on-write: a new list is built and swapped in, so a publish that is already in progress is unaffected.
- No duplicate check is made. Subscribing the same delegate twice causes it to run twice per event.
- Logs at Information level.

### `Unsubscribe<T>`

```csharp
void Unsubscribe<T>(EventHandler<T> handler) where T : DomainEvent;
```

Removes one registration of `handler` from the handlers for `T`.

**Type Parameters**
- `T`: the event type the handler was subscribed to.

**Parameters**
- `handler`: the delegate to remove. Must not be null, otherwise `ArgumentNullException` is thrown.

**Behavior**
- Removal uses `List<T>.Remove`, which relies on `Delegate.Equals`. Two delegates are equal when they have the same target and method. A lambda creates a new delegate each time it is written, so keep a reference to the instance you subscribed. A method group is equal to itself across conversions when it refers to the same target and method.
- Only the first matching registration is removed. If the handler was subscribed twice, call `Unsubscribe` twice.
- If the handler is not registered, the call does nothing and logs nothing.
- Removing the last handler leaves an empty list behind. Later publishes for that type log an Information message reporting 0 handlers rather than the Debug message.
- Logs at Information level when a handler is actually removed.

## Dispatch by exact type

Handlers are stored under `typeof(T)`, where `T` is the type parameter used at the call site, not the runtime type of the event object. The consequences:

- A handler subscribed to a base event type does not receive events published as a derived type, and a handler subscribed to a derived type does not receive events published as the base type.
- To reach handlers for a base type from a derived instance, publish with the base type as the type argument: `PublishAsync<DomainEvent>(evt)`. Doing so reaches only handlers subscribed to `DomainEvent`, not the derived-type subscribers.

## Thread safety

- The subscriber map is a `ConcurrentDictionary<Type, List<Delegate>>`. Each list is treated as immutable once published. `PublishAsync` takes a reference to the list and iterates it without locking.
- `Subscribe` uses `AddOrUpdate`, which is atomic.
- `Unsubscribe` is a read, copy, and then a write through the indexer. It is not atomic. If `Subscribe` or another `Unsubscribe` for the same event type runs between the read and the write, one of the changes can be lost. Avoid changing subscriptions for the same event type from several threads at once. Changing subscriptions while publishes run on other threads is safe.

## Logging

All messages go through the injected `ILogger<InMemoryEventBus>`.

| Level | When | Message |
|-------|------|---------|
| Debug | Publish with no handlers for `T` | `No handlers registered for event type {EventType}` |
| Information | Publish with handlers | `Publishing event {EventName} (ID: {EventId}) with {HandlerCount} handlers` |
| Information | Publish finished | `Event {EventName} (ID: {EventId}) published successfully` |
| Information | Subscribe | `Handler subscribed for event type {EventType}` |
| Information | Unsubscribe removed a handler | `Handler unsubscribed from event type {EventType}` |
| Error | A handler threw | `Error handling event {EventName} (ID: {EventId}) in handler {HandlerType}` |

The `HandlerType` value comes from `handler.GetType().Name` on the stored `Delegate`. This is the delegate type, which is `EventHandler`1`, not the class that owns the handler method. Use the exception stack trace to identify the subscriber.

## Usage

The example assumes a concrete `DomainEvent` subclass named `ExampleEvent`. `DomainEvent` is defined elsewhere in `Events/`.

```csharp
public class ExampleService
{
    private readonly IEventBus _bus;

    public ExampleService(IEventBus bus)
    {
        _bus = bus;
    }

    // Subscribe once, for example during startup.
    public void Start()
    {
        _bus.Subscribe<ExampleEvent>(OnExample);
    }

    // Stop receiving events. Passes an equal delegate created from the same method group.
    public void Stop()
    {
        _bus.Unsubscribe<ExampleEvent>(OnExample);
    }

    public Task RaiseAsync()
    {
        return _bus.PublishAsync(new ExampleEvent());
    }

    private Task OnExample(ExampleEvent @event)
    {
        // Handle the event. Exceptions are logged by the bus and do not reach RaiseAsync.
        return Task.CompletedTask;
    }
}
```

To keep a handler you can unsubscribe later, store the delegate in a field:

```csharp
private readonly EventHandler<ExampleEvent> _onExample;

public ExampleService(IEventBus bus)
{
    _bus = bus;
    _onExample = e => Task.CompletedTask;
}

public void Start() => _bus.Subscribe(_onExample);
public void Stop() => _bus.Unsubscribe(_onExample);
```

Subscribing a lambda without keeping a reference to it makes later removal impossible.
