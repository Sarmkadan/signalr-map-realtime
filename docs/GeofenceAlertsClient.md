# GeofenceAlertsClient

The `GeofenceAlertsClient` is a SignalR-based client for managing geofence alerts in real-time vehicle tracking systems. It facilitates the creation, monitoring, and management of geofenced areas around specified coordinates, enabling alerts when tracked vehicles enter or exit these zones.

## API

### `public string Id`
A unique identifier for the geofence instance. Used to distinguish between multiple geofence clients in the system.

### `public string Name`
A human-readable name for the geofence. Descriptive labels help identify the purpose or location of the geofence in the system.

### `public double Latitude`
The latitude coordinate of the geofence center in decimal degrees. Used to define the geographic center of the circular geofence.

### `public double Longitude`
The longitude coordinate of the geofence center in decimal degrees. Paired with `Latitude`, it defines the geographic center of the circular geofence.

### `public double RadiusKm`
The radius of the geofence in kilometers. Defines the circular boundary around the center point (`Latitude`, `Longitude`) within which alerts are triggered.

### `public GeofenceAlertsClient`
Constructor for the `GeofenceAlertsClient` class. Initializes a new instance with default or unspecified values for `Id`, `Name`, `Latitude`, `Longitude`, and `RadiusKm`.

### `public void AddGeofence`
Registers the geofence with the SignalR hub. The geofence is defined by the `Latitude`, `Longitude`, and `RadiusKm` properties. No parameters or return value.

### `public async Task Connect`
Establishes a connection to the SignalR hub. Required before adding geofences or receiving alerts. Returns a `Task` that completes when the connection is established.

### `public async Task Disconnect`
Terminates the connection to the SignalR hub. Should be called when the client is no longer needed to free resources. Returns a `Task` that completes when the disconnection is finalized.

### `public async Task RunExample`
Executes a predefined example scenario demonstrating geofence alert functionality. Useful for testing or demonstration purposes. Returns a `Task` that completes when the example finishes.

### `public string Type`
The type of geofence alert or event. Descriptive label indicating the nature of the alert (e.g., "Entered", "Exited").

### `public string VehicleId`
The unique identifier of the vehicle associated with a geofence alert or event.

### `public string Message`
A descriptive message associated with a geofence alert or event. Provides context about the alert (e.g., "Vehicle entered geofence zone").

### `public string VehicleId`
The unique identifier of the vehicle associated with the geofence client instance.

### `public double Latitude`
The latitude coordinate of the vehicle's current position. Used to track vehicle movement relative to the geofence.

### `public double Longitude`
The longitude coordinate of the vehicle's current position. Used to track vehicle movement relative to the geofence.

### `public static async Task Main`
Entry point for console applications using the `GeofenceAlertsClient`. Demonstrates basic usage of the client, including connection, geofence setup, and disconnection. Returns a `Task` that completes when the program exits.

## Usage

### Example 1: Basic Geofence Setup and Monitoring
