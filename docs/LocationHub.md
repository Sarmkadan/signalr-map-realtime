# LocationHub

`LocationHub` is a SignalR hub responsible for real-time communication between a server and clients tracking vehicle locations, status updates, and fleet-related events. It facilitates broadcasting location changes, handling subscriptions to specific vehicles, and managing alerts or route progress notifications.

## API

### `public LocationHub`
The constructor initializes the hub instance. No custom parameters are exposed; dependency injection should be configured externally.

### `public override async Task OnConnectedAsync()`
Called when a client connects to the hub. Performs any necessary setup, such as logging or initializing client-specific state.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `HubException` – If connection setup fails.

---

### `public override async Task OnDisconnectedAsync(Exception exception)`
Called when a client disconnects from the hub. Handles cleanup, such as unsubscribing the client from vehicle updates or logging the disconnection.

**Parameters:**
- `exception` (`Exception`) – The exception that caused the disconnection, if any. `null` if the disconnection was clean.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `HubException` – If cleanup fails.

---

### `public async Task SendLocationUpdate(string vehicleId, double latitude, double longitude, double? speed = null, double? heading = null)`
Broadcasts a location update for a specific vehicle to all subscribed clients.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the vehicle.
- `latitude` (`double`) – The vehicle's latitude coordinate.
- `longitude` (`double`) – The vehicle's longitude coordinate.
- `speed` (`double?`, optional) – The vehicle's current speed in km/h. Defaults to `null`.
- `heading` (`double?`, optional) – The vehicle's current heading in degrees. Defaults to `null`.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId` is `null` or empty.
- `HubException` – If broadcasting fails.

---

### `public async Task NotifyAssetRemoved(string vehicleId)`
Notifies subscribed clients that a vehicle has been removed from the system.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the removed vehicle.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId` is `null` or empty.
- `HubException` – If notification fails.

---

### `public async Task RequestAllVehicleLocations()`
Requests the server to send the current locations of all vehicles to the calling client. Typically used by clients to synchronize state after reconnecting.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `HubException` – If retrieval or broadcasting fails.

---

### `public async Task BroadcastVehicleStatusChange(string vehicleId, string status)`
Broadcasts a status change for a specific vehicle to all subscribed clients.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the vehicle.
- `status` (`string`) – The new status of the vehicle (e.g., "online", "offline", "maintenance").

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId` or `status` is `null` or empty.
- `HubException` – If broadcasting fails.

---

### `public async Task SubscribeToVehicle(string vehicleId)`
Subscribes the calling client to receive updates for a specific vehicle.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the vehicle to subscribe to.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId` is `null` or empty.
- `HubException` – If subscription fails.

---

### `public async Task UnsubscribeFromVehicle(string vehicleId)`
Unsubscribes the calling client from receiving updates for a specific vehicle.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the vehicle to unsubscribe from.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId` is `null` or empty.
- `HubException` – If unsubscription fails.

---

### `public async Task RequestVehicleLocation(string vehicleId)`
Requests the server to send the current location of a specific vehicle to the calling client.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the vehicle.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId` is `null` or empty.
- `HubException` – If retrieval or broadcasting fails.

---

### `public async Task BroadcastRouteProgress(string vehicleId, string routeId, double progress)`
Broadcasts the progress of a vehicle along a predefined route to all subscribed clients.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the vehicle.
- `routeId` (`string`) – The unique identifier of the route.
- `progress` (`double`) – The progress percentage (0–100) along the route.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId` or `routeId` is `null` or empty.
- `ArgumentOutOfRangeException` – If `progress` is outside the 0–100 range.
- `HubException` – If broadcasting fails.

---

### `public async Task SendAlert(string vehicleId, string alertType, string message)`
Sends an alert for a specific vehicle to all subscribed clients.

**Parameters:**
- `vehicleId` (`string`) – The unique identifier of the vehicle.
- `alertType` (`string`) – The type of alert (e.g., "speeding", "geofence_violation").
- `message` (`string`) – A descriptive message accompanying the alert.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `vehicleId`, `alertType`, or `message` is `null` or empty.
- `HubException` – If broadcasting fails.

---

### `public async Task RequestOnlineVehicleCount()`
Requests the server to send the current count of online vehicles to the calling client.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `HubException` – If retrieval or broadcasting fails.

---

### `public async Task BroadcastFleetStatus(string status)`
Broadcasts a fleet-wide status update to all connected clients.

**Parameters:**
- `status` (`string`) – The new fleet status (e.g., "active", "degraded", "offline").

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `ArgumentNullException` – If `status` is `null` or empty.
- `HubException` – If broadcasting fails.

---

### `public async Task Ping()`
A lightweight method to verify the hub connection is alive. Clients may call this periodically to check connectivity.

**Returns:**
`Task` – A task representing the asynchronous operation.

**Throws:**
- `HubException` – If the ping fails.

## Usage

### Example 1: Subscribing to Vehicle Updates
