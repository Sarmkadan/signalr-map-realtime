# RouteRepositoryValidation

`RouteRepositoryValidation` is a static class in the `SignalRMapRealtime.Data.Repositories` namespace. It provides extension methods that validate a `RouteRepository` instance and parameters used by route-query operations.

All validation methods return an `IReadOnlyList<string>`:

- An empty list means the value or parameter set is valid.
- A non-empty list contains human-readable validation problems.

## Repository validation

### `Validate(RouteRepository? value)`

Validates a repository instance. A `null` value causes `ArgumentNullException.ThrowIfNull` to throw an `ArgumentNullException`. Every non-null instance returns an empty list because the class delegates repository integrity to the constructor and database context.

### `IsValid(RouteRepository? value)`

Calls `Validate` and returns `true` when the resulting list is empty. It therefore returns `true` for every non-null repository and throws `ArgumentNullException` for `null`.

## Parameter validation

| Method | Valid input | Invalid result |
| --- | --- | --- |
| `ValidateParametersForGetActiveRoutesByVehicleAsync(int vehicleId)` | `vehicleId > 0` | `Vehicle ID must be positive, but was {vehicleId}.` |
| `ValidateParametersForGetRoutesByUserAsync(int userId)` | `userId > 0` | `User ID must be positive, but was {userId}.` |
| `ValidateParametersForGetRoutesByCompletionAsync(bool isCompleted)` | Both `true` and `false` | Never returns a validation problem. |
| `ValidateParametersForGetRouteWithDetailsAsync(int routeId)` | `routeId > 0` | `Route ID must be positive, but was {routeId}.` |
| `ValidateParametersForGetRoutesByDateRangeAsync(DateTime startDate, DateTime endDate)` | `startDate <= endDate` | `Start date must be before or equal to end date.` |
| `ValidateParametersForGetLongestRoutesAsync(int topCount)` | `1 <= topCount <= 1000` | For zero or a negative value: `Top count must be positive, but was {topCount}.` For a value above 1000: `Top count {topCount} is too large; maximum recommended is 1000.` |
| `ValidateParametersForGetAverageCompletionTimeAsync(int vehicleId)` | `vehicleId > 0` | `Vehicle ID must be positive, but was {vehicleId}.` |
| `ValidateParametersForGetPendingRoutesAsync()` | No parameters to validate | Never returns a validation problem. |

The date-range boundary is inclusive, so equal start and end dates are valid. The longest-routes validator also treats both ends of its range as valid: `1` and `1000` return an empty list.

Although several XML comments declare `ArgumentOutOfRangeException`, the parameter-validation implementations do not throw for invalid values. They return the messages shown above.

## Usage

```csharp
IReadOnlyList<string> vehicleProblems = vehicleId
    .ValidateParametersForGetActiveRoutesByVehicleAsync();

IReadOnlyList<string> dateProblems = startDate
    .ValidateParametersForGetRoutesByDateRangeAsync(endDate);

if (vehicleProblems.Count == 0 && dateProblems.Count == 0)
{
    // The supplied parameters satisfy these validation rules.
}
```

Because these methods only report problems, callers decide how to present or otherwise handle the returned messages.
