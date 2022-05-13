# VehicleService

The `VehicleService` class provides a comprehensive interface for managing vehicle-related operations in the `signalr-map-realtime` project. It encapsulates business logic for creating, retrieving, updating, and deleting vehicle records, as well as querying vehicles based on various criteria such as status, driver association, fuel levels, and speeding conditions. This service acts as an intermediary between data access layers and application logic, ensuring consistent handling of vehicle data.

## API

### `Task<VehicleDto> CreateVehicleAsync`
Creates a new vehicle record in the system.

- **Purpose**: Adds a new vehicle to the database.
- **Parameters**: None (assumes vehicle data is provided via the method's context or injected dependencies).
- **Returns**: A `VehicleDto` representing the newly created vehicle.
- **Throws**:
  - `ArgumentNullException` if required vehicle data is missing.
  - `InvalidOperationException` if the vehicle cannot be created due to business rules (e.g., duplicate identifiers).

---

### `Task<VehicleDto?> GetVehicleAsync`
Retrieves a single vehicle by its unique identifier.

- **Purpose**: Fetches a vehicle record from the database.
- **Parameters**: None (assumes the vehicle identifier is provided via the method's context).
- **Returns**: A `VehicleDto` if the vehicle exists; otherwise, `null`.
- **Throws**: None.

---

### `Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync`
Retrieves all vehicles in the system.

- **Purpose**: Returns a complete list of vehicles for administrative or analytical purposes.
- **Parameters**: None.
- **Returns**: An `IEnumerable<VehicleDto>` containing all vehicles.
- **Throws**: None.

---

### `Task<IEnumerable<VehicleDto>> GetVehiclesByStatusAsync`
Retrieves vehicles filtered by their operational status (e.g., "Online", "Offline", "Maintenance").

- **Purpose**: Queries vehicles based on their current status.
- **Parameters**: None (assumes the status filter is provided via the method's context).
- **Returns**: An `IEnumerable<VehicleDto>` of vehicles matching the specified status.
- **Throws**: None.

---

### `Task<IEnumerable<VehicleDto>> GetOnlineVehiclesAsync`
Retrieves all vehicles currently marked as online.

- **Purpose**: Convenience method for fetching vehicles that are actively reporting telemetry.
- **Parameters**: None.
- **Returns**: An `IEnumerable<VehicleDto>` of online vehicles.
- **Throws**: None.

---

### `Task<IEnumerable<VehicleDto>> GetVehiclesByDriverAsync`
Retrieves vehicles associated with a specific driver.

- **Purpose**: Queries vehicles assigned to a driver for tracking or management.
- **Parameters**: None (assumes the driver identifier is provided via the method's context).
- **Returns**: An `IEnumerable<VehicleDto>` of vehicles linked to the driver.
- **Throws**: None.

---

### `Task<VehicleDto> UpdateVehicleAsync`
Updates an existing vehicle record.

- **Purpose**: Modifies vehicle attributes (e.g., location, status, metadata).
- **Parameters**: None (assumes updated vehicle data is provided via the method's context).
- **Returns**: A `VehicleDto` reflecting the updated vehicle.
- **Throws**:
  - `ArgumentNullException` if required data is missing.
  - `KeyNotFoundException` if the vehicle does not exist.
  - `InvalidOperationException` if the update violates business rules.

---

### `Task<bool> SetVehicleOnlineStatusAsync`
Toggles a vehicle's online/offline status.

- **Purpose**: Marks a vehicle as online or offline based on telemetry or user input.
- **Parameters**: None (assumes the vehicle identifier and desired status are provided via the method's context).
- **Returns**: `true` if the status was updated successfully; otherwise, `false`.
- **Throws**: None.

---

### `Task<bool> UpdateVehicleStatusAsync`
Updates a vehicle's operational status (e.g., "In Transit", "Idle", "Maintenance").

- **Purpose**: Modifies the vehicle's status to reflect its current operational state.
- **Parameters**: None (assumes the vehicle identifier and new status are provided via the method's context).
- **Returns**: `true` if the status was updated successfully; otherwise, `false`.
- **Throws**: None.

---

### `Task<IEnumerable<VehicleDto>> GetLowFuelVehiclesAsync`
Retrieves vehicles with critically low fuel levels.

- **Purpose**: Identifies vehicles requiring refueling for proactive maintenance.
- **Parameters**: None (assumes the fuel threshold is defined via configuration or method context).
- **Returns**: An `IEnumerable<VehicleDto>` of vehicles below the fuel threshold.
- **Throws**: None.

---

### `Task<IEnumerable<VehicleDto>> GetSpeedingVehiclesAsync`
Retrieves vehicles exceeding a predefined speed limit.

- **Purpose**: Identifies vehicles violating speed policies for enforcement or alerts.
- **Parameters**: None (assumes the speed threshold is defined via configuration or method context).
- **Returns**: An `IEnumerable<VehicleDto>` of vehicles exceeding the speed limit.
- **Throws**: None.

---

### `Task<int> GetOnlineVehicleCountAsync`
Returns the number of vehicles currently online.

- **Purpose**: Provides a count of active vehicles for monitoring or dashboard displays.
- **Parameters**: None.
- **Returns**: The number of online vehicles as an `int`.
- **Throws**: None.

---

### `Task<bool> VehicleExistsAsync`
Checks if a vehicle exists in the system.

- **Purpose**: Validates the presence of a vehicle before performing operations.
- **Parameters**: None (assumes the vehicle identifier is provided via the method's context).
- **Returns**: `true` if the vehicle exists; otherwise, `false`.
- **Throws**: None.

---

### `Task<bool> DeleteVehicleAsync`
Removes a vehicle record from the system.

- **Purpose**: Deletes a vehicle and its associated data.
- **Parameters**: None (assumes the vehicle identifier is provided via the method's context).
- **Returns**: `true` if the vehicle was deleted successfully; otherwise, `false`.
- **Throws**:
  - `KeyNotFoundException` if the vehicle does not exist.
  - `InvalidOperationException` if the vehicle cannot be deleted due to business rules (e.g., active assignments).

---

### `Task<IEnumerable<VehicleDto>> GetVehiclesByAssetTypeAsync`
Retrieves vehicles filtered by their asset type (e.g., "Truck", "Car", "Motorcycle").

- **Purpose**: Queries vehicles based on their classification for reporting or management.
- **Parameters**: None (assumes the asset type filter is provided via the method's context).
- **Returns**: An `IEnumerable<VehicleDto>` of vehicles matching the specified asset type.
- **Throws**: None.

## Usage

### Example 1: Creating and Updating a Vehicle
