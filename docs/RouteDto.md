# RouteDto
The `RouteDto` type is a data transfer object designed to represent a route in the context of the signalr-map-realtime project. It encapsulates various details about a route, including its identifier, name, description, vehicle assignment, user assignment, planned and estimated times, distances, and waypoints. This object is intended to facilitate the exchange of route information between different components or layers of the application.

## API
The `RouteDto` type exposes the following public members:
* `Id`: A unique identifier for the route, represented as an integer.
* `Name`: The name of the route, represented as a string.
* `Description`: An optional description of the route, represented as a nullable string.
* `VehicleId`: The identifier of the vehicle assigned to the route, represented as an integer.
* `Vehicle`: The vehicle assigned to the route, represented as a nullable `VehicleDto` object.
* `AssignedUserId`: The identifier of the user assigned to the route, represented as a nullable integer.
* `AssignedUser`: The user assigned to the route, represented as a nullable `UserDto` object.
* `PlannedDepartureTime`: The planned departure time of the route, represented as a `DateTime` object.
* `EstimatedArrivalTime`: The estimated arrival time of the route, represented as a `DateTime` object.
* `TotalDistance`: The total distance of the route, represented as a nullable double.
* `ActualDistance`: The actual distance traveled, represented as a nullable double.
* `IsActive`: A boolean indicating whether the route is active.
* `IsCompleted`: A boolean indicating whether the route is completed.
* `Waypoints`: A collection of waypoints along the route, represented as an `ICollection` of `WaypointDto` objects.
* `CreatedAt`: The time at which the route was created, represented as a `DateTime` object.
* `UpdatedAt`: The time at which the route was last updated, represented as a `DateTime` object.

## Usage
Here are two examples of using the `RouteDto` type in C# code:
```csharp
// Example 1: Creating a new RouteDto object
RouteDto newRoute = new RouteDto
{
    Id = 1,
    Name = "Route 1",
    Description = "This is the first route",
    VehicleId = 1,
    PlannedDepartureTime = DateTime.Now,
    EstimatedArrivalTime = DateTime.Now.AddHours(2),
    Waypoints = new List<WaypointDto>
    {
        new WaypointDto { Latitude = 37.7749, Longitude = -122.4194 },
        new WaypointDto { Latitude = 38.8977, Longitude = -77.0365 }
    }
};

// Example 2: Updating an existing RouteDto object
RouteDto existingRoute = GetRouteFromDatabase(1);
existingRoute.Description = "Updated route description";
existingRoute.ActualDistance = 100.5;
existingRoute.IsCompleted = true;
UpdateRouteInDatabase(existingRoute);
```

## Notes
When working with the `RouteDto` type, consider the following edge cases and thread-safety remarks:
* The `Vehicle` and `AssignedUser` properties are nullable, so be sure to check for null before attempting to access their properties.
* The `TotalDistance` and `ActualDistance` properties are nullable, so be sure to check for null before performing calculations or comparisons.
* The `Waypoints` collection is an `ICollection`, so be sure to use the appropriate methods for adding, removing, or modifying waypoints.
* When updating a `RouteDto` object, be sure to update the `UpdatedAt` property to reflect the current time.
* The `RouteDto` type does not appear to be thread-safe, so be sure to use appropriate synchronization mechanisms when accessing or modifying its properties in a multi-threaded environment.
