# AssetDto

Data transfer object representing an asset in the signalr-map-realtime system. Used for serializing and deserializing asset information between client and server, particularly for real-time tracking and status updates.

## API

### `public int Id`
Unique identifier for the asset. Required field; must be a positive integer. Used as the primary key in database operations and for referencing the asset in API calls.

### `public string Name`
Human-readable name of the asset. Non-nullable string; must be provided during creation. Used for display purposes in the UI and for identification.

### `public string SerialNumber`
Unique serial number of the asset. Non-nullable string; must be provided during creation. Used for tracking and inventory purposes.

### `public AssetType AssetType`
Type classification of the asset. Non-nullable enum value indicating the category of the asset (e.g., Vehicle, Equipment). Used for filtering and grouping in the application logic.

### `public decimal? Value`
Monetary value of the asset, in the system's base currency. Optional field; may be null if value is unknown or not applicable. Stored as a nullable decimal with precision for financial calculations.

### `public string? Description`
Detailed description or notes about the asset. Optional field; may be null. Used for additional context in the UI or for documentation purposes.

### `public int? VehicleId`
Identifier of the vehicle associated with this asset, if applicable. Optional field; may be null if the asset is not vehicle-related. Used for linking assets to specific vehicles in the fleet.

### `public string? Condition`
Current physical or operational condition of the asset. Optional field; may be null if condition is not tracked. Used for maintenance and status reporting.

### `public bool RequiresSpecialHandling`
Flag indicating whether the asset requires special handling procedures (e.g., hazardous materials, fragile items). Defaults to `false`. Used to trigger safety protocols and UI warnings.

### `public DateTime? LastTrackedAt`
Timestamp of the last known location or status update for the asset. Optional field; may be null if never tracked. Used for real-time monitoring and recency checks.

### `public DateTime CreatedAt`
Timestamp indicating when the asset record was created in the system. Non-nullable; set automatically on creation. Used for auditing and lifecycle tracking.

### `public DateTime UpdatedAt`
Timestamp indicating when the asset record was last modified. Non-nullable; updated automatically on changes. Used for change tracking and synchronization.

### `public AssetType Type`
Alias for `AssetType`; duplicate property for backward compatibility or alternative naming. Non-nullable enum value indicating the category of the asset. Behavior and usage identical to `AssetType`.

### `public string? Status`
Current operational status of the asset (e.g., Active, In Maintenance, Retired). Optional field; may be null if not applicable. Used for filtering and dashboard displays.

## Usage
