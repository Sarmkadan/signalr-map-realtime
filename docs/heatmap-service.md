# HeatmapService

> **Note:** there is no class named `HeatmapService` in the codebase. Heatmap generation is implemented by
> `ClusteringService` through the `IClusteringService.GetHeatmapAsync` method. This document describes that method,
> its inputs and outputs, and the related REST endpoint.

## Location

| Item | Path |
|------|------|
| Interface and implementation | `src/SignalRMapRealtime/Services/ClusteringService.cs` |
| Request model | `ClusterQueryRequest` in `src/SignalRMapRealtime/DTOs/ClusterDto.cs` |
| Response models | `HeatmapResponseDto`, `HeatmapPointDto` in `src/SignalRMapRealtime/DTOs/ClusterDto.cs` |
| REST endpoint | `ClusteringController.GetHeatmap` in `src/SignalRMapRealtime/Controllers/ClusteringController.cs` |
| DI registration | `ClusteringExtensions.AddClustering()` (registered as scoped) |

## Purpose

The heatmap answers the question "where did vehicles and assets spend time or report locations?" It takes the
location samples in a time window, groups them into a fixed-size geographic grid, and returns one tile per occupied
cell. Each tile carries a normalised intensity between 0.0 and 1.0, relative to the busiest tile in the result.

## Interface

```csharp
public interface IClusteringService
{
    Task<ClusterResponseDto> GetClustersAsync(ClusterQueryRequest request, CancellationToken cancellationToken = default);
    Task<HeatmapResponseDto> GetHeatmapAsync(ClusterQueryRequest request, CancellationToken cancellationToken = default);
}
```

`GetHeatmapAsync` throws `ArgumentNullException` when `request` is `null`.

## Input: ClusterQueryRequest

| Property | Type | Default | Meaning |
|----------|------|---------|---------|
| `VehicleId` | `int?` | `null` | If set, only this vehicle's location history is used. If `null`, samples from all locations are used. |
| `From` | `DateTime?` | now minus 24 h (UTC) | Start of the time window, inclusive. |
| `To` | `DateTime?` | now (UTC) | End of the time window, inclusive. |
| `GridCellKm` | `double` | `0.5` | Cell edge length in km. The value is clamped to a minimum of `0.1` inside the service. |
| `MinLatitude` / `MaxLatitude` | `double?` | `null` | Optional inclusive bounding box, latitude. |
| `MinLongitude` / `MaxLongitude` | `double?` | `null` | Optional inclusive bounding box, longitude. |

## Output: HeatmapResponseDto

| Property | Type | Meaning |
|----------|------|---------|
| `Points` | `IReadOnlyList<HeatmapPointDto>` | One entry per occupied cell, sorted by `Intensity` descending. |
| `TotalSamples` | `int` | Number of location samples that passed the filters. |
| `MaxCount` | `int` | Highest raw sample count of any tile. Used as the normalisation divisor. |

Each `HeatmapPointDto` (a record) contains:

| Property | Type | Meaning |
|----------|------|---------|
| `Latitude` | `double` | Mean latitude of the samples in the cell (not the geometric centre of the cell). |
| `Longitude` | `double` | Mean longitude of the samples in the cell. |
| `Intensity` | `double` | `Count / MaxCount`, in the range 0.0 to 1.0. |
| `Count` | `int` | Raw number of samples in the cell. |

If there are no samples, `Points` is empty, `TotalSamples` is `0`, and `MaxCount` is `0`. In that case `Intensity` is never computed.

## Algorithm

1. **Load samples** (`LoadPointsAsync`):
   - With `VehicleId`: calls `ILocationService.GetLocationHistoryAsync(vehicleId, from, to)`.
   - Without `VehicleId`: calls `ILocationService.GetLocationsAsync(1, 10000)` and filters by `RecordedAt` in `[from, to]`.
     Only the first 10,000 locations are read. Older or additional samples are ignored in this mode.
   - Bounding-box filter is applied to each sample.
2. **Bucket into grid** (`BucketIntoGrid`):
   - Latitude degrees per km is `1 / 110.574`.
   - Longitude degrees per km is `1 / (111.32 * cos(lat))`, with `cos(lat)` floored at `0.001` to avoid division by zero near the poles.
   - Cell row is `floor(lat / (cellKm * latDegPerKm))`, and cell column is `floor(lon / (cellKm * lonDegPerKm))`.
   - Note that the longitude width is computed from each sample's own latitude. Cells at different latitudes can therefore have slightly different real widths.
3. **Compute tiles**: for each occupied cell, `Count` is the number of samples and `Latitude`/`Longitude` are their means.
4. **Normalise**: `maxCount` is the largest cell count. `Intensity = Count / maxCount`, or `0.0` if `maxCount` is `0`.
5. **Sort** tiles by `Intensity` descending and return the result.

The method logs a debug message at the start and an information message at the end, with sample count, tile count and elapsed milliseconds.

## REST endpoint

```
GET /api/clustering/heatmap
```

Query parameters map directly to `ClusterQueryRequest`: `vehicleId`, `from`, `to`, `gridCellKm` (default `0.5`),
`minLatitude`, `maxLatitude`, `minLongitude`, `maxLongitude`.

The controller clamps `gridCellKm` to a minimum of `0.1`. The response is wrapped in `ApiResponse<HeatmapResponseDto>`
with the message "Heatmap computed successfully" and status 200. Errors are handled in the controller's `catch` block.

Example:

```
GET /api/clustering/heatmap?from=2026-10-08T00:00:00Z&to=2026-10-09T00:00:00Z&gridCellKm=1.0
```

## Registration

`DependencyInjection.cs` calls `services.AddClustering()`, which registers `IClusteringService` as
`ClusteringService` with **scoped** lifetime. The lifetime matches the `ILocationService` dependency.

## Known limitations

- **Sample cap without a vehicle filter.** The heatmap reads at most 10,000 locations when `VehicleId` is not set. On a busy
  system the result covers only part of the time window and can silently miss recent or older samples.
- **Filtering after loading.** The bounding box is always applied in memory. Without `VehicleId`, the time window is also
  applied in memory, after the 10,000-sample read. With `VehicleId`, the time window is passed to `GetLocationHistoryAsync`.
- **Mean position, not cell centre.** `Latitude` and `Longitude` are sample means. Tiles with few samples can sit
  near the edge of their cell.
- **Relative intensity.** `Intensity` is relative to the busiest tile in the current result. Values from two queries
  with different windows or bounding boxes are not directly comparable.
- **No caching.** Each call recomputes the grid from scratch.

## Related

- `IClusteringService.GetClustersAsync` uses the same loading and bucketing code and returns `LocationClusterDto` items instead of intensities.
- `ILocationService`: source of location samples.
