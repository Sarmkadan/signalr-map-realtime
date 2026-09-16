# `WaypointExtensions`

`WaypointExtensions` is a static class in the `SignalRMapRealtime.Domain.Models` namespace. It provides convenience methods for calculating the distance between two `Waypoint` instances, selecting a display name, and determining whether a waypoint is currently in progress.

## `CalculateDistanceTo`

```csharp
double distanceKm = source.CalculateDistanceTo(destination);
```

Calculates the great-circle distance between `source` and `destination` with the Haversine formula. Latitude and longitude values are converted from degrees to radians, and the result is returned in kilometers using an Earth radius of `6371.0` kilometers.

The method throws `ArgumentNullException` when either `source` or `destination` is `null`.

## `GetDisplayName`

```csharp
string displayName = waypoint.GetDisplayName();
```

Returns the first available display value in this order:

1. `Name`, when it is not null or empty.
2. `Address`, when it is not null or empty.
3. Coordinates formatted as `"{Latitude:F4}, {Longitude:F4}"`.

Whitespace-only names and addresses count as available because the method checks with `string.IsNullOrEmpty`, not `string.IsNullOrWhiteSpace`.

The method throws `ArgumentNullException` when `waypoint` is `null`.

## `IsInProgress`

```csharp
bool inProgress = waypoint.IsInProgress();
```

Returns `true` only when all of the following are true:

- `IsCompleted` is `false`.
- `ActualArrivalTime` has a value.
- `ActualDepartureTime` does not have a value.

If `IsCompleted` is `true`, the method returns `false` regardless of the arrival and departure values.

The method throws `ArgumentNullException` when `waypoint` is `null`.
