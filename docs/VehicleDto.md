# VehicleDto

Data transfer object representing a vehicle in the real‑time map application. It aggregates identifying information, operational status, asset details, and timestamps used by the SignalR hub to broadcast vehicle state to clients.

## API

| Member | Type | Purpose | Parameters | Return Value | Throws |
|--------|------|---------|------------|--------------|--------|
| `Id` | `int` | Unique identifier for the vehicle record. | none | The vehicle's ID. | None |
| `Name` | `string` | Human‑readable name or nickname of the vehicle. | none | The vehicle name; may be empty string. | None |
| `RegistrationNumber` | `string` | Official registration identifier (e.g., license plate) used for regulatory purposes. | none | The registration number; may be empty string. | None |
| `Status` | `VehicleStatus` | Current operational status (e.g., Idle, Moving, Stopped). | none | Enum value indicating status. | None |
| `AssetType` | `AssetType` | Classification of the vehicle asset (e.g., Truck, Car, Van). | none | Enum value indicating asset type. | None |
| `DriverId` | `int?` | Identifier of the driver assigned to the vehicle, if any. | none | Driver ID or `null` when unassigned. | None |
| `Manufacturer` | `string?` | Name of the vehicle's manufacturer. | none | Manufacturer name or `null` if unknown. | None |
| `ModelYear` | `int?` | Model year of the vehicle. | none | Year as an integer or `null`. | None |
| `MaxSpeed` | `double?` | Maximum speed the vehicle can achieve, in km/h. | none | Speed value or `null`. | None |
| `FuelLevel` | `double?` | Current fuel level as a percentage (0‑100). | none | Fuel level or `null`. | None |
| `IsOnline` | `bool` | Indicates whether the vehicle is currently connected and reporting telemetry. | none | `true` if online, `false` otherwise. | None |
| `LastLocation` | `LocationDto?` | Most recent geographic location received from the vehicle. | none | Location data or `null` if no location has been reported. | None |
| `Make` | `string?` | Manufacturer make (synonymous with `Manufacturer` in some contexts). | none | Make string or `null`. | None |
| `Model` | `string?` | Specific model designation of the vehicle. | none | Model string or `null`. | None |
| `Year` | `int?` | Calendar year the vehicle was manufactured (may duplicate `ModelYear`). | none | Year value or `null`. | None |
| `LicensePlate` | `string?` | Official license plate number (may duplicate `RegistrationNumber`). | none | Plate string or `null`. | None |
| `CreatedAt` | `DateTime` | Timestamp when the vehicle record was first created in the system. | none | UTC creation time. | None |
| `UpdatedAt` | `DateTime` | Timestamp when the vehicle record was last modified. | none | UTC last‑update time. | None |

## Usage

```csharp
// Example  VehicleDto for a newly registered truck
var truckDto = new VehicleDto
{
    Id = 101,
    Name = "Big Hauler",
    RegistrationNumber = "XYZ‑98767Plate = "XYZ-987", // note: property name is LicensePlate
    Status = VehicleStatus.Idle,
    AssetType = AssetType.Truck,
    DriverId = 42,
    Manufacturer = "Volvo",
    ModelYear = 2022,
    MaxSpeed = 120.0,
    FuelLevel = 75.5,
    IsOnline = true,
    LastLocation = new LocationDto { Latitude = 40.7128, Longitude = -74.0060 },
    Make = "Volvo",
    Model = "FH16",
    Year = 2022,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Sending the DTO via a SignalR hub method
await hubContext.Clients.All.SendAsync("ReceiveVehicleUpdate", truckDto);
```

```csharp
// Example: updating an existing vehicle's fuel level and status
public async Task UpdateVehicleTelemetry(int vehicleId, double fuelLevel, VehicleStatus status)
{
    var dto = await _vehicleRepository.GetDtoByIdAsync(vehicleId);
    if (dto == null) throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");

    dto.FuelLevel = fuelLevel;
    dto.Status = status;
    dto.UpdatedAt = DateTime.UtcNow;

    await _vehicleRepository.SaveAsync(dto);
    await _hubContext.Clients.Group($"vehicle-{vehicleId}").SendAsync("VehicleTelemetry", dto);
}
```

## Notes

- The class is a plain data container with public gettable and settable properties; it does **not** enforce immutability or synchronization. Concurrent modifications from multiple threads can lead to race conditions; external synchronization is required if the instance is shared.
- Several properties appear to duplicate information (`Name`/`RegistrationNumber` vs `Make`/`Model`/`Year`/`LicensePlate`). Consumers should treat them as independent fields unless the domain model guarantees consistency.
- Nullable reference types (`string?`, `int?`, `double?`, `LocationDto?`) indicate that the corresponding data may be absent; callers must check for `null` before dereferencing.
- `DateTime` values are expected to be in UTC; mixing local times may cause incorrect ordering or display.
- The `IsOnline` flag does not guarantee that `LastLocation` is current; a vehicle may be online but have not yet reported a location after reconnection.
- No property throws exceptions on get or set; exceptions may arise only from surrounding logic (e.g., repository access).
