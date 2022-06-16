# VehicleExtensions

`VehicleExtensions` provides a set of static convenience methods for querying and formatting information about vehicle entities within the signalr-map-realtime domain. These extension methods operate on vehicle objects to surface derived state—such as driver presence, descriptive summaries, tracking metadata, and age calculations—without requiring direct manipulation of the underlying vehicle internals.

## API

### `HasDriver`

```csharp
public static bool HasDriver(this Vehicle vehicle)
```

**Purpose:** Determines whether the specified vehicle currently has an assigned driver.

**Parameters:**
- `vehicle` — The `Vehicle` instance to inspect. Must not be `null`.

**Return Value:** `true` if a driver is assigned and present; otherwise `false`.

**Throws:** `ArgumentNullException` when `vehicle` is `null`.

---

### `GetFullDescription`

```csharp
public static string GetFullDescription(this Vehicle vehicle)
```

**Purpose:** Builds and returns a human-readable, full description string for the vehicle, typically combining properties such as make, model, registration, and status into a single formatted value.

**Parameters:**
- `vehicle` — The `Vehicle` instance to describe. Must not be `null`.

**Return Value:** A non-null, non-empty string containing the vehicle’s full description.

**Throws:** `ArgumentNullException` when `vehicle` is `null`.

---

### `GetTrackingSessionCount`

```csharp
public static int GetTrackingSessionCount(this Vehicle vehicle)
```

**Purpose:** Retrieves the total number of tracking sessions recorded for the vehicle. This count reflects historical or active telemetry sessions associated with the vehicle’s movement data.

**Parameters:**
- `vehicle` — The `Vehicle` instance to query. Must not be `null`.

**Return Value:** A non-negative integer representing the count of tracking sessions.

**Throws:** `ArgumentNullException` when `vehicle` is `null`.

---

### `GetVehicleAge`

```csharp
public static int? GetVehicleAge(this Vehicle vehicle)
```

**Purpose:** Calculates the age of the vehicle in whole years based on its known manufacture or registration date. Returns `null` when the date required for the calculation is unavailable.

**Parameters:**
- `vehicle` — The `Vehicle` instance to evaluate. Must not be `null`.

**Return Value:** An integer representing the vehicle age in years, or `null` if the age cannot be determined.

**Throws:** `ArgumentNullException` when `vehicle` is `null`.

## Usage

### Example 1: Dashboard summary for a single vehicle

```csharp
Vehicle vehicle = vehicleRepository.GetById(vehicleId);

bool hasDriver = vehicle.HasDriver();
string description = vehicle.GetFullDescription();
int? age = vehicle.GetVehicleAge();

Console.WriteLine($"Vehicle: {description}");
Console.WriteLine($"Driver present: {hasDriver}");
Console.WriteLine($"Age: {(age.HasValue ? $"{age.Value} years" : "Unknown")}");
```

### Example 2: Fleet tracking report

```csharp
IEnumerable<Vehicle> fleet = vehicleRepository.GetAllActive();

foreach (var vehicle in fleet)
{
    int sessionCount = vehicle.GetTrackingSessionCount();
    string label = vehicle.GetFullDescription();

    logger.Info($"{label} — {sessionCount} tracking session(s)");

    if (sessionCount == 0)
    {
        alertService.RaiseWarning($"No tracking data for {label}");
    }
}
```

## Notes

- All methods throw `ArgumentNullException` when the `vehicle` argument is `null`. Callers should perform null checks before invocation when the vehicle source is not guaranteed to return a valid instance.
- `GetVehicleAge` returns `null` when the underlying date field (e.g., manufacture date) is absent. Consumers must handle the nullable result explicitly to avoid null-reference issues downstream.
- `GetTrackingSessionCount` returns zero when no sessions exist; it does not return `null`. A zero value is a valid, meaningful result and should not be interpreted as an error state.
- These methods are static extension methods and are thread-safe provided the `Vehicle` instance is not concurrently mutated during the call. The methods themselves do not modify vehicle state and hold no internal mutable shared data.
