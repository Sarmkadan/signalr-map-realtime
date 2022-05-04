# Waypoint

The `Waypoint` class represents a specific stop or location within a defined route in the `signalr-map-realtime` system. It encapsulates location data, scheduling information, contact details, and the completion status required for managing and tracking the progress of entities moving through a sequence of points.

## API

### Properties

*   `Id` (`int`): The unique identifier for the waypoint.
*   `Sequence` (`int`): The order of this waypoint within its associated route.
*   `Name` (`string`): The descriptive name or label of the waypoint.
*   `Latitude` (`double`): The geographic latitude of the waypoint.
*   `Longitude` (`double`): The geographic longitude of the waypoint.
*   `Address` (`string?`): The physical address associated with the waypoint, if available.
*   `ArrivalTimeStart` (`string?`): The start of the scheduled arrival window, formatted as a string.
*   `ArrivalTimeEnd` (`string?`): The end of the scheduled arrival window, formatted as a string.
*   `EstimatedDurationMinutes` (`int?`): The expected duration of the stop in minutes, if specified.
*   `Instructions` (`string?`): Additional operational instructions for this waypoint.
*   `ContactName` (`string?`): The name of the primary contact at the waypoint.
*   `ContactPhone` (`string?`): The phone number for the contact at the waypoint.
*   `IsCompleted` (`bool`): Indicates whether the waypoint has been marked as visited or processed.
*   `ActualArrivalTime` (`DateTime?`): The timestamp when the waypoint was actually reached.
*   `ActualDepartureTime` (`DateTime?`): The timestamp when the entity departed from the waypoint.
*   `RouteId` (`int`): The foreign key referencing the associated route.
*   `Route` (`Route?`): The navigation property for the associated route.

### Methods

*   `void CompleteWaypoint()`: Updates the state of the waypoint to completed and records the current time as the `ActualArrivalTime` if not already set.
*   `void Reset()`: Resets the waypoint to an uncompleted state, clearing `IsCompleted`, `ActualArrivalTime`, and `ActualDepartureTime`.
*   `bool HasValidCoordinates()`: Returns `true` if the latitude and longitude represent a valid geographic location (e.g., within standard bounds); otherwise returns `false`.

## Usage

### Marking a Waypoint as Complete
```csharp
var waypoint = route.Waypoints.FirstOrDefault(w => w.Id == targetId);
if (waypoint != null && !waypoint.IsCompleted)
{
    waypoint.CompleteWaypoint();
    // Proceed to persist changes to the database
}
```

### Validating Waypoint Location
```csharp
public bool CanNavigateToWaypoint(Waypoint waypoint)
{
    if (!waypoint.HasValidCoordinates())
    {
        Log.Warning($"Waypoint {waypoint.Name} has invalid coordinates.");
        return false;
    }
    return true;
}
```

## Notes

*   **Thread Safety**: The `Waypoint` class is not inherently thread-safe. If instances are shared across multiple threads during updates or state transitions, external synchronization mechanisms must be implemented to prevent race conditions.
*   **Coordinate Validation**: The `HasValidCoordinates` method performs basic validation. Ensure that the numeric values for `Latitude` and `Longitude` adhere to standard geographic ranges before relying on them for mapping services.
*   **Nullable Types**: Several fields are nullable (`?`). Consuming code should perform appropriate null checks or use null-coalescing operators when accessing `Address`, `ArrivalTimeStart`, `ActualArrivalTime`, and other nullable members to avoid `NullReferenceException`.
