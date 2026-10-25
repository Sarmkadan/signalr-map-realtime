# DomainEventDispatcher

Source: `src/SignalRMapRealtime/Events/DomainEventDispatcher.cs`

The default implementation of `IDomainEventDispatcher`. It finds the handlers for a domain event in the dependency injection container, then calls each one's `HandleAsync` method. The handlers are discovered by type at dispatch time, so no subscription step is needed.

The file contains one class, `DomainEventDispatcher`. It does not register itself with the container.

## Class

```csharp
public class DomainEventDispatcher : IDomainEventDispatcher
```

### Constructor

```csharp
public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger, IServiceProvider serviceProvider)
```

**Parameters**
- `logger`: receives the dispatcher's log messages.
- `serviceProvider`: the provider used by the single-argument `DispatchAsync` overload to resolve handlers.

**Exceptions**
- `ArgumentNullException`: thrown when either parameter is null.

## Methods

All three overloads are public and return `Task`. Each one throws `ArgumentNullException` when a required argument is null. The arguments are checked before any work starts.

### `DispatchAsync(DomainEvent)`

```csharp
public async Task DispatchAsync(DomainEvent @event)
```

Dispatches `@event` using the `IServiceProvider` passed to the constructor. It checks `@event` and then calls the two-argument overload with the stored provider.

**Parameters**
- `@event`: the event to dispatch.

### `DispatchAsync(DomainEvent, IServiceProvider)`

```csharp
public async Task DispatchAsync(DomainEvent @event, IServiceProvider serviceProvider)
```

This is the overload that does the work. The other two overloads delegate to it.

**Parameters**
- `@event`: the event to dispatch.
- `serviceProvider`: the provider to resolve handlers from.

**Behavior**

1. Logs the start of the dispatch at Information level.
2. Builds the service type `IEventHandler<T>`, where `T` is the runtime type of `@event` (`@event.GetType()`). This is the key used for the lookup.
3. Calls `GetServices` on `serviceProvider` for that type. The result is materialized into a list with `OfType<object>().ToList()`.
4. If the list is empty, logs at Debug level and returns. No error is raised.
5. Otherwise, logs the handler count at Information level, then starts every handler (see [Handler invocation](#handler-invocation)).
6. Waits for all handler tasks with `Task.WhenAll`.
7. If every handler succeeded, logs a success message at Information level.

**Returns**
- A `Task` that completes after all handlers have finished, or faults if one of them failed.

### `DispatchAsync(DomainEvent, IServiceScope)`

```csharp
public async Task DispatchAsync(DomainEvent @event, IServiceScope scope)
```

Dispatches `@event` using `scope.ServiceProvider`. It checks both arguments and then calls the `IServiceProvider` overload.

**Parameters**
- `@event`: the event to dispatch.
- `scope`: the scope whose provider resolves the handlers. The method does not dispose the scope. The caller owns it.

Use this overload when handlers depend on scoped services. Each scope then gets its own handler instances.

## Handler lookup

Handlers are found by the event's runtime type. Given an event of type `OrderPlacedEvent`, the lookup key is `IEventHandler<OrderPlacedEvent>`. The lookup does not walk base types. A handler registered for `IEventHandler<DomainEvent>` is not found for a derived event.

Every registration of the service type is returned, in the order the container returns them. The same handler class registered twice runs twice.

## Handler invocation

Each handler runs inside its own `try`/`catch`:

1. The dispatcher looks up a public method named `HandleAsync` on the handler's runtime type with `GetMethod("HandleAsync")`. The lookup is by name only. It does not check the `IEventHandler<T>` interface or the parameter types.
2. If no such method is found, the handler is skipped silently. No call is made and nothing is logged.
3. If found, the method is called through reflection with the event as its only argument.
4. The result is cast to `Task`. A `null` result throws `InvalidOperationException`. Any other non-`Task` return type fails the cast.
5. The task is awaited.

All handlers are started in the order they were resolved. Each one starts synchronously until its first `await`, and then the dispatcher moves to the next. After that, `Task.WhenAll` waits for all of them.

**Failure behavior**

- A failure in one handler is logged at Error level. The log includes the handler type name, event name, and event ID.
- The exception is rethrown from the handler's `catch` block. This means that `Task.WhenAll` faults.
- The other handlers keep running. Their results are not cancelled.
- Awaiting `DispatchAsync` throws one exception, the first one that `Task.WhenAll` reports. Any other failures are logged but not thrown to the caller.
- The success message is not logged when a handler fails, because the `await` throws first.

A caller of `DispatchAsync` must handle exceptions, because a failed handler fails the whole call.

**Exception shapes**

- A synchronous exception from `HandleAsync` arrives wrapped in `TargetInvocationException`, because it is thrown through reflection. The log message and the rethrown exception are both the wrapper. Inspect `InnerException` to find the original.
- An exception that faults the returned task is unwrapped by `await`, so it arrives unchanged.
- `AmbiguousMatchException` is raised if the handler type has more than one public `HandleAsync` method. This happens inside the `try`, so it is logged and rethrown the same way.
- `ArgumentException` is raised if the found method's parameters do not accept the event. This is also logged and rethrown.

## Logging

All messages go through the injected `ILogger<DomainEventDispatcher>`.

| Level | When | Message |
|-------|------|---------|
| Information | Dispatch starts | `Dispatching event {EventName} (ID: {EventId}) to handlers` |
| Debug | No handlers found for the event's runtime type | `No handlers registered for event type {EventType}` |
| Information | Handlers found | `Found {HandlerCount} handler(s) for event type {EventType}` |
| Error | A handler threw or faulted | `Error invoking handler {HandlerType} for event {EventName} (ID: {EventId})` |
| Information | All handlers finished without error | `Event {EventName} (ID: {EventId}) dispatched successfully to {HandlerCount} handler(s)` |

`{EventType}` is the runtime type name without the namespace. `{HandlerType}` is the handler's runtime type name.

## Thread safety

The class has two readonly fields, set once in the constructor. It keeps no mutable state between calls, so one instance can be used from several threads. Each call builds its own list of handlers.

Whether that is safe depends on the handlers and the provider. The dispatcher does not synchronize handler execution, and it does not limit how many handlers run at once.

## Limitations

- **No timeout or cancellation.** There is no `CancellationToken` parameter. A slow handler delays the caller until it finishes.
- **Reflection on every call.** `GetMethod` runs for each handler on each dispatch. The file does not cache the `MethodInfo`.
- **Root provider with the one-argument overload.** `DispatchAsync(DomainEvent)` uses the provider from the constructor. If the handlers need scoped services, call the `IServiceScope` overload instead.
- **No base-type dispatch.** Dispatching a derived event does not reach handlers registered for its base type.

## Usage

The example assumes a handler class and event type defined elsewhere. The handler is registered in the container under `IEventHandler<ExampleEvent>`, and the dispatcher is available as `IDomainEventDispatcher`.

```csharp
public class ExampleService
{
    private readonly IDomainEventDispatcher _dispatcher;

    public ExampleService(IDomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    // Dispatch using the root provider. Use the scope overload when handlers are scoped.
    public Task RaiseAsync()
    {
        return _dispatcher.DispatchAsync(new ExampleEvent());
    }

    // Dispatch within a scope. The caller disposes the scope.
    public async Task RaiseInScopeAsync(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        await _dispatcher.DispatchAsync(new ExampleEvent(), scope);
    }
}
```

`IDomainEventDispatcher` is the interface type the rest of the code depends on. Its declaration is in `IDomainEventDispatcher.cs`, not in this file.
