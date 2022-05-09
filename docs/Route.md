# Route
The `Route` type represents a planned or actual journey between two points, including details about the vehicle, assigned user, and waypoints along the route. It provides a comprehensive set of properties to track the status and progress of the route, making it a fundamental component in the signalr-map-realtime project.

## API
The `Route` type exposes the following public members:
* `Id`: A unique identifier for the route.
* `Name`: The name of the route.
* `Description`: An optional description of the route.
* `VehicleId`: The identifier of the vehicle assigned to the route.
* `Vehicle`: The vehicle object assigned to the route, or null if not assigned.
* `AssignedUserId`: The identifier of the user assigned to the route, or null if not assigned.
* `AssignedUser`: The user object assigned to the route, or null if not assigned.
* `PlannedDepartureTime`: The planned departure time of the route.
* `EstimatedArrivalTime`: The estimated arrival time of the route.
* `ActualDepartureTime`: The actual departure time of the route, or null if not departed.
* `ActualArrivalTime`: The actual arrival time of the route, or null if not arrived.
* `Origin`: The origin of the route, or null if not specified.
* `Destination`: The destination of the route, or null if not specified.
* `TotalDistance`: The total distance of the route, or null if not calculated.
* `ActualDistance`: The actual distance traveled along the route, or null if not tracked.
* `IsActive`: A boolean indicating whether the route is active.
* `IsCompleted`: A boolean indicating whether the route is completed.
* `Waypoints`: A collection of waypoints along the route.
* `TrackingSessionId`: The identifier of the tracking session associated with the route, or null if not associated.
* `TrackingSession`: The tracking session object associated with the route, or null if not associated.

## Usage
Here are two examples of using the `Route` type in C#:
```csharp
// Create a new route
var route = new Route
{
    Name = "Route 1",
    Description = "Test route",
    VehicleId = 1,
    PlannedDepartureTime = DateTime.Now,
    EstimatedArrivalTime = DateTime.Now.AddHours(2),
    Origin = "Point A",
    Destination = "Point B",
    Waypoints = new List<Waypoint>
    {
        new Waypoint { Latitude = 37.7749, Longitude = -122.4194 },
        new Waypoint { Latitude = 37.7859, Longitude = -122.4364 }
    }
};

// Update the route status
route.ActualDepartureTime = DateTime.Now;
route.IsActive = true;
```

## Notes
When working with the `Route` type, consider the following edge cases and thread-safety remarks:
* The `Vehicle` and `AssignedUser` properties can be null if not assigned, so null checks should be performed before accessing their properties.
* The `ActualDepartureTime` and `ActualArrivalTime` properties can be null if not departed or arrived, so null checks should be performed before accessing their values.
* The `TotalDistance` and `ActualDistance` properties can be null if not calculated or tracked, so null checks should be performed before accessing their values.
* The `Waypoints` collection can be empty if no waypoints are defined, so null checks should be performed before accessing its elements.
* The `TrackingSessionId` and `TrackingSession` properties can be null if not associated, so null checks should be performed before accessing their values.
* When updating the route status, ensure that the `IsActive` and `IsCompleted` properties are updated accordingly to reflect the current state of the route.
* When accessing or updating the route properties, consider using thread-safe approaches to avoid concurrency issues, especially in multi-threaded environments.
