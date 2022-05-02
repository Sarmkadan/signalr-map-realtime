# LocationHubExtensions

`LocationHubExtensions` is a static utility class in the `signalr-map-realtime` project that provides extension methods for SignalR hub contexts to facilitate real-time location-based messaging. It enables targeted communication with vehicle groups and fleet-wide broadcasts, along with utilities for monitoring connected client counts. These methods are designed to simplify server-side logic when managing real-time vehicle tracking and fleet coordination scenarios.

## API

### BroadcastToVehicleGroupAsync

**Purpose**  
Sends a real-time message to all connected clients subscribed to a specific vehicle group.

**Parameters**  
- `hubContext` (`IHubContext<LocationHub>`): The SignalR hub context used to send messages.
- `vehicleId` (`string`): The unique identifier of the vehicle group to target.
- `message` (`object`): The payload to broadcast to the group.

**Return Value**  
`Task`: A task representing the asynchronous operation.

**Exceptions**  
- `ArgumentNullException`: Thrown when `hubContext` or `vehicleId` is `null`.
- `InvalidOperationException`: Thrown if the hub context is not properly initialized or the group does not exist.

---

### GetConnectionId

**Purpose**  
Retrieves the connection ID associated with the current hub context.

**Parameters**  
- `hubContext` (`IHubContext<LocationHub>`): The SignalR hub context from which to extract the connection ID.

**Return Value**  
`string`: The connection ID of the current client.

**Exceptions**  
- `ArgumentNullException`: Thrown when `hubContext` is `null`.
- `InvalidOperationException`: Thrown if the connection ID cannot be resolved from the context.

---

### BroadcastToFleetAsync

**Purpose**  
Broadcasts a real-time message to all connected clients subscribed to a specific fleet group.

**Parameters**  
- `hubContext` (`IHubContext<LocationHub>`): The SignalR hub context used to send messages.
- `fleetId` (`string`): The unique identifier of the fleet group to target.
- `message` (`object`): The payload to broadcast to the fleet.

**Return Value**  
`Task`: A task representing the asynchronous operation.

**Exceptions**  
- `ArgumentNullException`: Thrown when `hubContext` or `fleetId` is `null`.
- `InvalidOperationException`: Thrown if the hub context is not properly initialized or the group does not exist.

---

### GetConnectedClientCountAsync

**Purpose**  
Retrieves the number of currently connected clients to the hub.

**Parameters**  
- `hubContext` (`IHubContext<LocationHub>`): The SignalR hub context to query for client count.

**Return Value**  
`Task<int>`: A task that resolves to the count of connected clients.

**Exceptions**  
- `ArgumentNullException`: Thrown when `hubContext` is `null`.
- `InvalidOperationException`: Thrown if the hub context is not properly initialized.

---

## Usage

### Example 1: Broadcasting Location Updates to a Vehicle Group

```csharp
public class VehicleLocationService
{
    private readonly IHubContext<LocationHub> _hubContext;

    public VehicleLocationService(IHubContext<LocationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task UpdateVehicleLocation(string vehicleId, LocationUpdate update)
    {
        await _hubContext.BroadcastToVehicleGroupAsync(vehicleId, update);
    }
}
```

### Example 2: Monitoring Connected Clients in a Fleet Hub

```csharp
public class FleetMonitor
{
    private readonly IHubContext<LocationHub> _hubContext;

    public FleetMonitor(IHubContext<LocationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<int> GetActiveClientCount()
    {
        return await _hubContext.GetConnectedClientCountAsync();
    }
}
```

---

## Notes

- All methods require a valid `IHubContext<LocationHub>` instance. Passing `null` will result in `ArgumentNullException`.
- `BroadcastToVehicleGroupAsync` and `BroadcastToFleetAsync` rely on SignalR's group management. Clients must be explicitly added to groups using `AddToGroupAsync` on the hub context for messages to be received.
- Thread safety: These methods are safe for concurrent use in multi-threaded environments, as they delegate to SignalR's internal synchronization mechanisms. However, the caller must ensure that the `IHubContext` instance remains valid for the duration of the operation.
- `GetConnectedClientCountAsync` returns the count at the time of invocation. The value may become stale immediately in high-traffic scenarios.
- Exception handling for network or infrastructure failures during message broadcasting is not explicitly documented and should be handled by the caller if required.
