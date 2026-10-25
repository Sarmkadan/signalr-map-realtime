# ClusteringService

Source: `src/SignalRMapRealtime/Services/ClusteringService.cs`

Computes location clusters and heatmap tiles by bucketing coordinates into a fixed-size geographic grid. The file contains three types: the `IClusteringService` interface, the `ClusteringService` implementation, and the `ClusteringExtensions` class, which registers the service in DI.

Register it as scoped. It depends on `ILocationService`, and the file's own comment says the scoped lifetime matches that dependency.

## Public API

```csharp
public interface IClusteringService
public sealed class ClusteringService : IClusteringService
public static class ClusteringExtensions
```

### IClusteringService

| Member | Purpose |
|--------|---------|
| `GetClustersAsync(ClusterQueryRequest, CancellationToken)` | Returns a `ClusterResponseDto` with one cluster per occupied grid cell. |
| `GetHeatmapAsync(ClusterQueryRequest, CancellationToken)` | Returns a `HeatmapResponseDto` with one tile per occupied grid cell, intensity-scaled. |

### ClusteringService

| Member | Purpose |
|--------|---------|
| Constructor `(ILocationService, ILogger<ClusteringService>)` | Stores both dependencies. Throws `ArgumentNullException` if either is null. |
| `GetClustersAsync` | See [Cluster computation](#cluster-computation). |
| `GetHeatmapAsync` | See [Heatmap computation](#heatmap-computation). |

Both public methods are `async` and accept a `ClusterQueryRequest`. Both throw `ArgumentNullException` if `request` is null.

### ClusteringExtensions

| Member | Purpose |
|--------|---------|
| `AddClustering(this IServiceCollection)` | Calls `AddScoped<IClusteringService, ClusteringService>()` and returns the same collection. Throws `ArgumentNullException` if `services` is null. |

## Query inputs

The file reads these members of `ClusterQueryRequest`. It does not define the type.

| Member | Used for |
|--------|----------|
| `From`, `To` | Time window. Both are nullable. Missing values default to the last 24 hours (`UtcNow - 24h` to `UtcNow`). |
| `VehicleId` | Nullable. When set, the vehicle's history is loaded. When not set, the service samples recent locations across all vehicles. |
| `GridCellKm` | Requested grid cell size in kilometres. Clamped to a minimum of 0.1 km. |
| `MinLatitude`, `MaxLatitude`, `MinLongitude`, `MaxLongitude` | Optional bounding box. Each bound applies only when it has a value. Bounds are inclusive. |

## Cluster computation

`GetClustersAsync` runs these steps:

1. Logs a Debug message with the time window and requested grid size. Starts a `Stopwatch`.
2. Calls `LoadPointsAsync` to get the `(Lat, Lon)` list.
3. Sets `cellKm = Math.Max(0.1, request.GridCellKm)`.
4. Calls `BucketIntoGrid(points, cellKm)`.
5. For each occupied cell, builds a `LocationClusterDto`:
   - `CenterLatitude` and `CenterLongitude` are the arithmetic mean of the points in the cell.
   - `Count` is the number of points in the cell.
   - `MinLatitude`, `MaxLatitude`, `MinLongitude`, and `MaxLongitude` are the extremes of the points in the cell.
6. Logs an Information message with input count, cluster count, and elapsed milliseconds.
7. Returns `ClusterResponseDto` with `Clusters`, `TotalPoints` (the input count after filtering), and `GridCellKm` (the clamped value actually used).

Each occupied cell becomes exactly one cluster. The method does not merge neighbouring cells or split large ones. A cell that holds a single point still produces a cluster with `Count = 1`. Empty input returns an empty `Clusters` list with `TotalPoints = 0`.

## Heatmap computation

`GetHeatmapAsync` follows the same load and bucket steps, with a different output:

1. Calls `LoadPointsAsync` and `BucketIntoGrid` with the same clamped `cellKm`.
2. Computes `maxCount`, the largest cell count. It is `0` when there are no cells.
3. For each occupied cell, builds a `HeatmapPointDto` (positional constructor: latitude, longitude, intensity, count):
   - Latitude and longitude are the mean of the cell's points.
   - `intensity = count / maxCount`, so the busiest tile is `1.0`. It is `0.0` if `maxCount` is `0`.
   - `count` is the raw cell count.
4. Sorts tiles by `Intensity` descending.
5. Logs an Information message and returns `HeatmapResponseDto` with `Points`, `TotalSamples` (the input count after filtering), and `MaxCount`.

Unlike `GetClustersAsync`, the heatmap method does not set `GridCellKm` on its response. The grid size is still clamped the same way.

## Point loading

`LoadPointsAsync` is private. It produces the input for both public methods.

1. Resolves the window: `From ?? UtcNow - 24h`, `To ?? UtcNow`.
2. Chooses the source:
   - **`VehicleId` has a value:** calls `ILocationService.GetLocationHistoryAsync(vehicleId, from, to, ct)`. The time filter is applied by that call.
   - **`VehicleId` is null:** calls `ILocationService.GetLocationsAsync(1, 10000, ct)` once, then keeps items where `RecordedAt >= from && RecordedAt <= to`. The time filter is applied in memory, after paging.
3. Keeps only points that pass `IsWithinBounds`.
4. Returns `(Latitude, Longitude)` tuples as a `List`.

The no-vehicle path reads only the first page, up to 10,000 locations (`MaxSampledLocations`). Older points that fall outside that page are never considered, even when they lie in the requested window. The file does not show the sort order that `GetLocationsAsync` uses.

## Bounding box

`IsWithinBounds` is private and static. It returns `false` as soon as one of the following holds, and `true` otherwise:

- `MinLatitude` is set and `lat < MinLatitude`
- `MaxLatitude` is set and `lat > MaxLatitude`
- `MinLongitude` is set and `lon < MinLongitude`
- `MaxLongitude` is set and `lon > MaxLongitude`

The check is inclusive at each bound. It does not handle longitude ranges that cross the antimeridian (where `MinLongitude` is greater than `MaxLongitude`). With such inputs, every point fails the check.

## Grid algorithm

The grid is computed by `BucketIntoGrid`, which is private and static. The grid is fixed in kilometres, not in degrees.

Constants used:

| Constant | Value | Meaning |
|----------|-------|---------|
| `MinGridCellKm` | `0.1` | Smallest allowed cell size in km. |
| `KmPerDegreeLatitude` | `110.574` | Approximate km per degree of latitude. |
| `KmPerDegreeLongitudeAtEquator` | `111.32` | Approximate km per degree of longitude at the equator. Scaled by `cos(lat)` per point. |
| `MinLatitudeCosine` | `0.001` | Floor on `cos(lat)`, to avoid division blow-up near the poles. |

For each point `(lat, lon)`:

1. `lonDegPerKm = 1 / (111.32 × max(0.001, cos(lat)))`
2. `row = floor(lat / (cellKm × (1 / 110.574)))`
3. `col = floor(lon / (cellKm × lonDegPerKm))`
4. Appends the point to the dictionary entry keyed by `(row, col)`.

In short, a cell spans `cellKm` in latitude and `cellKm` in longitude, measured at the point's own latitude. The dictionary key is the `(row, col)` pair, and no further grouping happens.

Limitations that follow from the code:

- Column width is computed from each point's latitude. Two points in the same row but at slightly different latitudes can therefore fall into different column boundaries. The grid is an approximation, not a single uniform lattice.
- Cell boundaries are fixed to the origin at 0°, 0°. The grid does not adapt to the data.
- Near the poles, `cos(lat)` is floored at `0.001`, so cell width in longitude becomes very large.

## Thread safety and state

The class is stateless apart from its two injected fields, `_locationService` and `_logger`. Each call builds its own point list and dictionaries, so concurrent calls on the same instance do not share mutable state. Concurrency still depends on the thread safety of `ILocationService`.

Cancellation is passed through to `ILocationService` only. The grouping and aggregation steps are synchronous and do not check the token.

## Logging

| Level | Event |
|-------|-------|
| Debug | Start of `GetClustersAsync` or `GetHeatmapAsync`, with the time window and requested grid size. |
| Information | End of each method, with input point count, output count (clusters or tiles), and elapsed milliseconds. |

The file does not log the bounding box, the vehicle ID, or the clamped grid size.
