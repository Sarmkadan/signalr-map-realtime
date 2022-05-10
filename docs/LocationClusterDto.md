# LocationClusterDto

`LocationClusterDto` is a data transfer object used to represent a hierarchical cluster of geographic locations, optionally accompanied by a heatmap representation of the points within the cluster. It is designed for efficient serialization in real‑time SignalR communications between server and client components of the `signalr-map-realtime` application.

## API

### Nested type: `HeatmapPointDto`
- **Purpose**: Represents a single point used for heatmap visualization. The exact fields are internal to the DTO but typically include latitude, longitude, and a weight or intensity value.
- **Type**: `public record HeatmapPointDto`
- **Remarks**: Being a record, it is immutable and provides value‑based equality semantics.

### Properties

| Property | Type | Purpose | Remarks |
|----------|------|---------|---------|
| `Points` | `IReadOnlyList<HeatmapPointDto>` | Collection of heatmap points that belong to this cluster. | The list is read‑only; callers cannot modify the collection through this property. |
| `TotalSamples` | `int` | Total number of raw samples that contributed to the cluster’s statistics. | Non‑negative value. |
| `MaxCount` | `int` | Maximum number of samples found in any sub‑cell or child cluster within this cluster. | Useful for determining the intensity peak. |
| `ComputedAt` | `DateTime` | Timestamp indicating when the cluster data was generated. | Stored as UTC; consumers may convert to local time as needed. |
| `Clusters` | `IReadOnlyList<LocationClusterDto>` | Child clusters that further subdivide this cluster’s geographic area. | Enables hierarchical clustering; may be empty if no subdivision exists. |
| `TotalPoints` | `int` | Total number of distinct location points represented by this cluster (including all descendants). | Should equal the sum of points in `Points` plus points in all child clusters. |
| `GridCellKm` | `double` | Size of the grid cell (in kilometers) used to generate this cluster’s spatial partitioning. | Positive value; influences the granularity of clustering. |
| `VehicleId` | `int?` | Identifier of the vehicle associated with the data, if the cluster is scoped to a specific vehicle; otherwise `null`. | Nullable to allow global (all‑vehicles) clusters. |
| `From` | `DateTime?` | Start of the time interval covered by the cluster’s data; `null` if unbounded. | When specified, represents the inclusive lower bound. |
| `To` | `DateTime?` | End of the time interval covered by the cluster’s data; `null` if unbounded. | When specified, represents the inclusive upper bound. |
| `MinLatitude` | `double?` | Minimum latitude of the geographic bounding box for this cluster; `null` if not applicable. | Together with `MaxLatitude`, `MinLongitude`, `MaxLongitude` defines the spatial extent. |
| `MaxLatitude` | `double?` | Maximum latitude of the geographic bounding box for this cluster; `null` if not applicable. |
| `MinLongitude` | `double?` | Minimum longitude of the geographic bounding box for this cluster; `null` if not applicable. |
| `MaxLongitude` | `double?` | Maximum longitude of the geographic bounding box for this cluster; `null` if not applicable. |

All properties are get‑only auto‑implemented members of a record type; they do not throw exceptions under normal use. Attempting to set a property outside of the constructor results in a compile‑time error.

## Usage

### Example 1: Creating a simple cluster with heatmap points

```csharp
using System;
using System.Collections.Generic;

// Assuming the DTOs are generated or referenced from the signalr-map-realtime project.
var heatmapPoints = new List<HeatmapPointDto>
{
    new HeatmapPointDto(40.7128, -74.0060, 12),
    new HeatmapPointDto(40.7130, -74.0058, 7)
};

var cluster = new LocationClusterDto(
    Points: heatmapPoints,
    TotalSamples: heatmapPoints.Count,
    MaxCount: 12,
    ComputedAt: DateTime.UtcNow,
    Clusters: Array.Empty<LocationClusterDto>(),
    TotalPoints: heatmapPoints.Count,
    GridCellKm: 0.5,
    VehicleId: 42,
    From: DateTime.UtcNow.AddHours(-1),
    To: DateTime.UtcNow,
    MinLatitude: 40.7128,
    MaxLatitude: 40.7130,
    MinLongitude: -74.0060,
    MaxLongitude: -74.0058
);

// The cluster can now be serialized and sent over a SignalR hub.
```

### Example 2: Reading hierarchical cluster data

```csharp
public void ProcessCluster(LocationClusterDto root)
{
    Console.WriteLine($"Cluster computed at {root.ComputedAt:O} with {root.TotalPoints} points.");

    foreach (var point in root.Points)
    {
        // point.Latitude, point.Longitude, point.Weight are accessible via the HeatmapPointDto record.
        Console.WriteLine($"Heatmap point: ({point.Latitude}, {point.Longitude}) weight={point.Weight}");
    }

    foreach (var child in root.Clusters)
    {
        ProcessCluster(child); // Recursive traversal of the cluster tree.
    }
}
```

## Notes

- **Nullability**: The nullable properties (`VehicleId`, `From`, `To`, `MinLatitude`, `MaxLatitude`, `MinLongitude`, `MaxLongitude`) may be `null` to indicate that the corresponding value is not applicable or unknown. Consumers should check for `null` before using these values.
- **Empty collections**: `Points` and `Clusters` may be empty lists (`Count == 0`). The DTO never returns `null` for these properties.
- **Immutability**: As a `record`, `LocationClusterDto` is immutable; all its properties are initialized via the primary constructor and cannot be altered after creation. This makes instances inherently thread‑safe for concurrent read operations.
- **Thread‑safety of underlying lists**: Although the DTO exposes `IReadOnlyList<T>`, the actual list instances supplied at construction time could be mutable. To guarantee thread‑safety, callers should pass immutable or read‑only collections (e.g., `Array.Empty<T>()`, `ImmutableArray<T>.Empty`, or a `ReadOnlyCollection<T>`) when constructing the DTO.
- **Timestamp handling**: `ComputedAt`, `From`, and `To` are expressed as `DateTime` values. The application expects UTC times; mixing local and UTC times may lead to incorrect temporal filtering.
- **Geographic bounds**: When `MinLatitude`, `MaxLatitude`, `MinLongitude`, and `MaxLongitude` are all non‑null, they define an axis‑aligned bounding box that fully encloses the cluster’s points. If any of these values are `null`, the bounds are considered undefined for that dimension.
