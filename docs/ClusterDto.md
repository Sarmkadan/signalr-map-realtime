# ClusterDto

Source: `src/SignalRMapRealtime/DTOs/ClusterDto.cs`

The file lives in the `SignalRMapRealtime.DTOs` namespace and defines five public types that describe clustering and heatmap data:

| Type | Kind | Purpose |
|------|------|---------|
| `LocationClusterDto` | `record` | One geographic cluster of location points |
| `HeatmapPointDto` | `record` | One heatmap tile with normalised intensity |
| `HeatmapResponseDto` | `class` | Heatmap result: tiles plus aggregate metadata |
| `ClusterResponseDto` | `class` | Clustering result: clusters plus aggregate metadata |
| `ClusterQueryRequest` | `class` | Query parameters for a clustering or heatmap request |

The file has no methods other than the `ToString` override on `ClusterResponseDto`. It contains no logic for computing clusters or heatmaps.

## LocationClusterDto

Immutable positional record that describes one cluster. All values are passed to the primary constructor.

```csharp
public record LocationClusterDto(
    double CenterLatitude,
    double CenterLongitude,
    int Count,
    double MinLatitude,
    double MaxLatitude,
    double MinLongitude,
    double MaxLongitude);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `CenterLatitude` | `double` | Centroid latitude of the cluster, in decimal degrees. |
| `CenterLongitude` | `double` | Centroid longitude of the cluster, in decimal degrees. |
| `Count` | `int` | Number of location points in the cluster. |
| `MinLatitude` | `double` | Southernmost latitude in the cluster. |
| `MaxLatitude` | `double` | Northernmost latitude in the cluster. |
| `MinLongitude` | `double` | Westernmost longitude in the cluster. |
| `MaxLongitude` | `double` | Easternmost longitude in the cluster. |

The four `Min`/`Max` members give the cluster's bounding box. The record has no nullable members.

## HeatmapPointDto

Immutable positional record for one heatmap tile.

```csharp
public record HeatmapPointDto(
    double Latitude,
    double Longitude,
    double Intensity,
    int Count);
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `Latitude` | `double` | Latitude of the tile centre, in decimal degrees. |
| `Longitude` | `double` | Longitude of the tile centre, in decimal degrees. |
| `Intensity` | `double` | Normalised value from `0.0` (no activity) to `1.0` (peak). The XML docs say it is relative to the busiest tile in the result set. |
| `Count` | `int` | Raw number of location samples in the tile. |

## HeatmapResponseDto

Class that wraps a heatmap result. All properties are `init`-only.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Points` | `IReadOnlyList<HeatmapPointDto>` | `Array.Empty<HeatmapPointDto>()` | Tiles, documented as ordered by descending `Intensity`. |
| `TotalSamples` | `int` | `0` | Total number of location samples used to build the heatmap. |
| `MaxCount` | `int` | `0` | Highest raw `Count` across all tiles. Used for normalisation. |
| `ComputedAt` | `DateTime` | `DateTime.UtcNow` at construction | UTC timestamp when the heatmap data was computed. |

## ClusterResponseDto

Class that wraps a clustering result. All properties are `init`-only.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Clusters` | `IReadOnlyList<LocationClusterDto>` | `Array.Empty<LocationClusterDto>()` | Clusters derived from the input locations. |
| `TotalPoints` | `int` | `0` | Total number of location points processed. |
| `GridCellKm` | `double` | `0` | Grid cell size in kilometres used during clustering. |
| `ComputedAt` | `DateTime` | `DateTime.UtcNow` at construction | UTC timestamp when clustering was computed. |

`ToString()` is overridden to return a summary in this form:

```
ClusterResponseDto(Clusters=<count>, TotalPoints=<n>, GridCellKm=<km>, ComputedAt=<ISO 8601 round-trip>)
```

`ComputedAt` is formatted with the `"O"` specifier.

## ClusterQueryRequest

Class that holds the query parameters. All properties are `init`-only, so the class is set with an object initializer.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `VehicleId` | `int?` | `null` | Restricts the query to one vehicle's location history. |
| `From` | `DateTime?` | `null` | Start of the time window, in UTC. The XML docs say it defaults to 24 hours before now when not supplied. |
| `To` | `DateTime?` | `null` | End of the time window, in UTC. The XML docs say it defaults to the current UTC time when not supplied. |
| `GridCellKm` | `double` | `0.5` | Edge length of a grid cell in kilometres. Smaller values give finer clusters and tiles. |
| `MinLatitude` | `double?` | `null` | Bounding-box minimum latitude (inclusive). |
| `MaxLatitude` | `double?` | `null` | Bounding-box maximum latitude (inclusive). |
| `MinLongitude` | `double?` | `null` | Bounding-box minimum longitude (inclusive). |
| `MaxLongitude` | `double?` | `null` | Bounding-box maximum longitude (inclusive). |

The `From`/`To` defaults are stated only in the XML docs. This file does not apply them. The code that applies them is outside this file.

## Usage

### Building a cluster record

```csharp
using SignalRMapRealtime.DTOs;

var cluster = new LocationClusterDto(
    CenterLatitude: 40.7129,
    CenterLongitude: -74.0059,
    Count: 19,
    MinLatitude: 40.7120,
    MaxLatitude: 40.7138,
    MinLongitude: -74.0070,
    MaxLongitude: -74.0048);
```

### Building a heatmap response

```csharp
var response = new HeatmapResponseDto
{
    Points = new List<HeatmapPointDto>
    {
        new(40.7129, -74.0059, Intensity: 1.0, Count: 12),
        new(40.7131, -74.0061, Intensity: 0.58, Count: 7)
    },
    TotalSamples = 19,
    MaxCount = 12
};
// ComputedAt is set to DateTime.UtcNow when the object is created.
```

### Building a query request

```csharp
var request = new ClusterQueryRequest
{
    VehicleId = 42,
    From = DateTime.UtcNow.AddHours(-6),
    To = DateTime.UtcNow,
    GridCellKm = 0.25,
    MinLatitude = 40.0,
    MaxLatitude = 41.0,
    MinLongitude = -75.0,
    MaxLongitude = -73.0
};
```

Leave `VehicleId`, `From`, `To`, and the bounding-box fields unset to query without those filters. `GridCellKm` keeps its default of `0.5` unless it is set.

## Notes

- **Units:** Latitude and longitude are in decimal degrees. `GridCellKm` is in kilometres.
- **Time zone:** Every `DateTime` in this file is documented as UTC. Pass UTC values in `ClusterQueryRequest`.
- **Nullability:** The file is compiled with `#nullable enable`. Only the `ClusterQueryRequest` filter fields and `VehicleId` are nullable. Collections are never `null`; they default to empty arrays.
- **Immutability:** The two records cannot be changed after construction. The three classes accept values only through `init` setters or the constructor.
- **Collection mutability:** The collection properties are typed `IReadOnlyList<T>`, but a caller that passes a mutable `List<T>` keeps the ability to change it. Pass arrays or other read-only collections if the data must not change after it is shared.
- **Bounding boxes:** The `Min`/`Max` values in `LocationClusterDto` are the cluster's extremes. The `Min`/`Max` fields in `ClusterQueryRequest` are documented as inclusive filters.
