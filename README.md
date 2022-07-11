// ... existing content ...

## DomainEventExtensions

The `DomainEventExtensions` class provides utility methods for working with domain events in the application. These extensions enable easy cloning, serialization, and type checking of domain events.

### Usage Example

```csharp
var originalEvent = new VehicleStatusChangedEvent
{
    VehicleId = Guid.NewGuid(),
    Status = VehicleStatus.InTransit,
    Timestamp = DateTime.UtcNow
};

var clonedEvent = DomainEventExtensions.Clone(originalEvent);

Console.WriteLine(clonedEvent.ToJson());
// Output: {"VehicleId":"<guid>","Status":"InTransit","Timestamp":"<timestamp>"}

Console.WriteLine(DomainEventExtensions.IsLocationUpdate(originalEvent));
// Output: False

Console.WriteLine(DomainEventExtensions.IsStatusChange(originalEvent));
// Output: True

Console.WriteLine(DomainEventExtensions.GetVehicleId(originalEvent));
// Output: <guid>

Console.WriteLine(DomainEventExtensions.GetDescription(originalEvent));
// Output: A human-readable description of the event
```
// ... existing content ...
