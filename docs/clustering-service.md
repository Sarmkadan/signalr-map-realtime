# ClusteringService

`ClusteringService` computes two read-only views over vehicle location data: grouped **clusters** (`GetClustersAsync`) and a normalised **heatmap** (`GetHeatmapAsync`). Both use the same pipeline: load points, filter by bounding box, bucket into a fixed geographic grid, then aggregate per occupied cell.

- Interface: `IClusteringService` (`src/SignalRMapRealtime/Services/ClusteringService.cs`)
- Implementation: `ClusteringService` (sealed, same file)
- DI helper: `ClusteringExtensions.AddClustering(IServiceCollection)`, which registers the service as **scoped**. It is called from `src/SignalRMapRealtime/Configuration/DependencyInjection.cs`.
- Consumer: `ClusteringController` (REST endpoints for clusters and heatmap).
- DTOs: `src/SignalRMapRealtime/DTOs/ClusterDto.cs`

## Dependencies

| Dependency | Purpose |
|---|---|
| `ILocationService` | Source of location samples (`GetLocationHistoryAsync`, `GetLocationsAsync`). |
| `ILogger<ClusteringService>` | Debug log at start, Information log with timing on completion. |

Both constructor arguments are null-checked with `ArgumentNullException.ThrowIfNull`.

The service is scoped because it depends on `ILocationService`, which is also scoped. Do not register it as singleton.

## Methods

### `GetClustersAsync(ClusterQueryRequest, CancellationToken)`

Returns a `ClusterResponseDto`:

- `Clusters`: one `LocationClusterDto` per occupied grid cell.
- `TotalPoints`: number of points that passed the filters.
- `GridCellKm`: the effective cell size (after clamping, see below).

Each cluster carries:

- `CenterLatitude` / `CenterLongitude`: arithmetic mean of the points in the cell (not the geometric centre of the cell).
- `Count`: number of points in the cell.
- `MinLatitude`, `MaxLatitude`, `MinLongitude`, `MaxLongitude`: bounding box of the points in the cell.

A cell with a single point is returned as a single-point cluster with `Count = 1`. An empty input returns an empty `Clusters` list and `TotalPoints = 0`.

### `GetHeatmapAsync(ClusterQueryRequest, CancellationToken)`

Returns a `HeatmapResponseDto`:

- `Points`: one `HeatmapPointDto` per occupied cell, ordered by `Intensity` descending.
- `TotalSamples`: number of points that passed the filters.
- `MaxCount`: highest per-cell count. `0` when there are no points.
- `ComputedAt`: UTC timestamp set by the DTO initialiser.

Each point has:

- `Latitude` / `Longitude`: mean position of the points in the cell.
- `Intensity = Count / MaxCount`, in the range (0, 1]. The densest cell is always `1.0`. With no points, `Points` is empty and `MaxCount = 0`.
- `Count`: raw sample count.

## Request: `ClusterQueryRequest`

| Property | Default | Behaviour |
|---|---|---|
| `VehicleId` | `null` | When set, history is loaded for that vehicle only via `GetLocationHistoryAsync`. When `null`, `GetLocationsAsync(1, 10000)` is used and filtered by time window. |
| `From` | `UtcNow - 24h` | Inclusive lower bound on `RecordedAt`. |
| `To` | `UtcNow` | Inclusive upper bound on `RecordedAt`. |
| `GridCellKm` | `0.5` | Cell edge length in km. Clamped to a minimum of `0.1` (`Math.Max(0.1, value)`). |
| `MinLatitude`, `MaxLatitude`, `MinLongitude`, `MaxLongitude` | `null` | Optional inclusive bounding box. Applied after loading, to both code paths. |

## Grid bucketing

Points are bucketed by `BucketIntoGrid`:

- Row: `floor(lat / (cellKm / 110.574))`.
- Column: `floor(lon / (cellKm / (111.32 * cos(lat))))`. The longitude degree width is computed from the latitude of **each point**, with `cos` floored at `0.001` to avoid division by zero near the poles.

This is an approximation. Cells are roughly `cellKm` on a side at the point's own latitude. Because the column width is recalculated per point, cells at different latitudes are not exactly aligned, so two nearby points on either side of a latitude band can land in different columns even when they are close. The effect is small for typical map areas and short distances; it is worth knowing when comparing counts across large latitude ranges.

## Known limits

- **Only the first 10,000 locations are read** when `VehicleId` is not supplied. `GetLocationsAsync(1, 10_000)` returns page 1 only. Time and box filters are applied after that page is loaded, so older or additional samples beyond the first page are never seen. For full-coverage results, use `VehicleId` or extend the loader to page through results.
- **Filtering happens in memory.** Time and bounding-box filters are not pushed down to the database for the global path.
- **Aggregation is synchronous** after loading, so large point sets take CPU time on the request thread.
- **Cell centres are means of points**, not grid-cell midpoints. Two maps rendered with different point sets will shift cluster centres even at the same grid size.

## Logging

| Level | Event |
|---|---|
| Debug | Start of computation: time window and `GridCellKm`. |
| Information | Completion: input point count, cluster or tile count, elapsed milliseconds. |

Logs contain no vehicle IDs or coordinates.

## Usage

```csharp
// Registration (already done in DependencyInjection.cs)
services.AddClustering();

// Injection (scoped consumers)
public class MyController(IClusteringService clustering)
{
    public Task<ClusterResponseDto> Get() => clustering.GetClustersAsync(new ClusterQueryRequest
    {
        VehicleId = 42,
        From = DateTime.UtcNow.AddHours(-6),
        To = DateTime.UtcNow,
        GridCellKm = 0.25,
    });
}
```

```csharp
var heat = await clustering.GetHeatmapAsync(new ClusterQueryRequest
{
    MinLatitude = 53.4, MaxLatitude = 53.7,
    MinLongitude = 9.8, MaxLongitude = 10.2,
});

foreach (var tile in heat.Points.Take(10))
{
    Console.WriteLine($"({tile.Latitude:F5}, {tile.Longitude:F5}) count={tile.Count} intensity={tile.Intensity:F2}");
}
```

## Tests

Unit tests are in `tests/signalr-map-realtime.Tests/ClusteringServiceTests.cs` and `tests/signalr-map-realtime.Tests/Services/ClusteringServiceZoomTests.cs`. Run them with:

```bash
dotnet test tests/signalr-map-realtime.Tests --filter "FullyQualifiedName~ClusteringService"
```
