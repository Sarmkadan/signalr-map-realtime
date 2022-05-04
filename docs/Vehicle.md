# Vehicle

Represents a vehicle entity within the `signalr-map-realtime` system, encapsulating properties related to identification, operational status, physical attributes, and tracking data. This type serves as a central model for real-time vehicle monitoring, route management, and driver assignment.

## API

### `public int Id`
Unique identifier for the vehicle. Assigned by the database upon creation and immutable thereafter.

### `public string Name`
Human-readable label for the vehicle. Intended for display purposes and may be updated as needed.

### `public string RegistrationNumber`
Official registration identifier issued by the relevant authority. Expected to be unique within the system and immutable after initial assignment.

### `public VehicleStatus Status`
Current operational state of the vehicle. Valid values are defined by the `VehicleStatus` enum, including states such as `Available`, `InUse`, `Maintenance`, or `Offline`.

### `public AssetType AssetType`
Categorization of the vehicle, indicating its type (e.g., `Car`, `Truck`, `Motorcycle`). Defined by the `AssetType` enum.

### `public int? DriverId`
Nullable foreign key referencing the `Id` of the `User` currently assigned as the vehicle's driver. `null` if no driver is assigned.

### `public User? Driver`
Navigation property to the `User` entity representing the vehicle's driver. `null` if no driver is assigned or if the driver is not loaded.

### `public string? Manufacturer`
Name of the vehicle's manufacturer (e.g., `Toyota`, `Ford`). Optional and may be `null`.

### `public int? ModelYear`
Year the vehicle model was manufactured. Optional and may be `null`.

### `public string? VIN`
Vehicle Identification Number, a unique 17-character code assigned by the manufacturer. Optional and may be `null`.

### `public double? MaxSpeed`
Maximum speed (in km/h) the vehicle is capable of achieving. Optional and may be `null`.

### `public double? FuelLevel`
Current fuel level as a percentage (0.0 to 100.0). Optional and may be `null`.

### `public bool IsOnline`
Indicates whether the vehicle is actively transmitting telemetry data. Updated in real-time via SignalR connections.

### `public Location? LastLocation`
Most recent geographic coordinates (`Latitude`, `Longitude`, `Timestamp`) reported by the vehicle. `null` if no location data is available.

### `public ICollection<Location> Locations`
Collection of all location records associated with the vehicle. Populated via tracking sessions and real-time updates.

### `public ICollection<TrackingSession> TrackingSessions`
Collection of tracking sessions during which the vehicle's movements were recorded. Includes start/end times, distance traveled, and related metrics.

### `public ICollection<Route> Routes`
Collection of routes assigned to or completed by the vehicle. Includes waypoints, expected duration, and status.

### `public string? Make`
Alternative to `Manufacturer`, representing the brand name (e.g., `Toyota`). Optional and may be `null`.

### `public string? Model`
Model name of the vehicle (e.g., `Camry`, `F-150`). Optional and may be `null`.

### `public int? Year`
Alternative to `ModelYear`, representing the manufacturing year. Optional and may be `null`.

## Usage

### Example 1: Assigning a Driver and Updating Vehicle Status
