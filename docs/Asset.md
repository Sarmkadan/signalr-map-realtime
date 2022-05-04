# Asset

The `Asset` class represents a trackable object within the `signalr-map-realtime` system, serving as the central entity for maintaining the identification, physical status, location, and vehicle association data for items being monitored. It integrates with the persistence layer to track historical telemetry and logistical details, providing methods to manage its assignment lifecycle relative to fleet vehicles.

## API

*   **`int Id`**: The unique primary key identifier for the asset.
*   **`string Name`**: The descriptive name used for identifying the asset within the application.
*   **`string SerialNumber`**: The unique manufacturer or internal serial number assigned to the asset.
*   **`AssetType AssetType`**: The primary categorization of the asset.
*   **`decimal? Value`**: An optional field representing the monetary value of the asset.
*   **`string? Description`**: An optional field containing additional descriptive information about the asset.
*   **`int? VehicleId`**: The foreign key identifier of the `Vehicle` to which the asset is currently assigned; `null` if unassigned.
*   **`Vehicle? Vehicle`**: The navigation property pointing to the associated `Vehicle` instance.
*   **`string? Condition`**: A textual description of the current physical state of the asset.
*   **`Location? CurrentLocation`**: The latest recorded geographical position of the asset.
*   **`ICollection<Location> LocationHistory`**: A collection containing the logged historical locations of the asset.
*   **`bool RequiresSpecialHandling`**: A flag indicating if the asset requires specific handling or transport protocols.
*   **`string? SpecialHandlingInstructions`**: Textual instructions for special handling, relevant if `RequiresSpecialHandling` is `true`.
*   **`DateTime CreatedAt`**: The timestamp when the asset record was initially created.
*   **`DateTime UpdatedAt`**: The timestamp of the most recent modification to the asset record.
*   **`DateTime? LastTrackedAt`**: The timestamp indicating when the asset was last updated by telemetry systems.
*   **`AssetType Type`**: An additional categorization field mirroring the type of the asset.
*   **`string? Status`**: The current operational status of the asset.
*   **`void AssignToVehicle()`**: Establishes an association between the asset and a vehicle, updating the `VehicleId` and navigation property.
*   **`void UnassignFromVehicle()`**: Removes the current association with a vehicle, setting the `VehicleId` to `null` and clearing the `Vehicle` navigation property.

## Usage

### Creating and Configuring an Asset
```csharp
var newAsset = new Asset
{
    Name = "Generator-Alpha",
    SerialNumber = "SN-99887766",
    AssetType = AssetType.Equipment,
    RequiresSpecialHandling = true,
    SpecialHandlingInstructions = "Keep upright at all times; do not stack.",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};
// Add to database context and save changes...
```

### Assigning an Asset to a Vehicle
```csharp
// Assuming an existing asset and target vehicle ID
var asset = context.Assets.Find(assetId);
if (asset != null)
{
    // Update the record and persist
    asset.AssignToVehicle();
    asset.VehicleId = targetVehicleId;
    asset.UpdatedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();
}
```

## Notes

*   **Thread Safety**: The `Asset` class is not inherently thread-safe. Instances manipulated within Entity Framework Core contexts must be accessed and modified within the bounds of a single thread or task, adhering to the standard database context lifecycle management.
*   **Data Integrity**: When utilizing `AssignToVehicle` or `UnassignFromVehicle`, ensure that the `UpdatedAt` property is manually refreshed if necessary to reflect the time of the business logic change, as the model may rely on external synchronization to populate audit timestamps.
*   **Nullability**: Optional properties marked with nullable types (`?`) should be validated before access if they are expected to be populated during application workflow execution, particularly when dealing with `Vehicle`, `CurrentLocation`, or `Value`.
