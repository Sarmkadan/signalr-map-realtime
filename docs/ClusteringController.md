# ClusteringController

`ClusteringController` is the REST controller for location clustering and heatmap density visualisation. Both endpoints are `GET`-only and computed on demand from the location history stored in the database, so each request runs a fresh calculation. The results are intended for map front-ends: clusters for marker grouping at lower zoom levels, and heatmaps for density overlays.

Source: `src/SignalRMapRealtime/Controllers/ClusteringController.cs`

## Routing

| Setting | Value |
|---|---|
| Base route | `api/Clustering` (from `[Route("api/[controller]")]`) |
| Response content type | `application/json` (from `[Produces]`) |
| Attribute | `[ApiController]` |

| Method | Route | Action | Purpose |
|---|---|---|---|
| `GET` | `/api/Clustering/clusters` | `GetClusters` | Groups locations into grid cells |
| `GET` | `/api/Clustering/heatmap` | `GetHeatmap` | Returns normalised density tiles |

## Constructor

`ClusteringController(IClusteringService clusteringService, ILogger<ClusteringController> logger)`

Both dependencies are injected and checked with `ArgumentNullException.ThrowIfNull`. A `null` argument throws at construction time.

## Endpoints

### GET /api/Clustering/clusters

Returns location clusters grouped by a geographic grid cell. Intended for rendering Leaflet marker clusters at lower zoom levels.

Query parameters (all optional):

| Name | Type | Default | Description |
|---|---|---|---|
| `vehicleId` | `int?` | `null` | Restricts results to a single vehicle (asset). |
| `from` | `DateTime?` | 24 hours ago (UTC) | Start of the time window. |
| `to` | `DateTime?` | now (UTC) | End of the time window. |
| `gridCellKm` | `double` | `0.5` | Grid cell edge length in km. Values below `0.1` are raised to `0.1`. |
| `minLatitude` | `double?` | `null` | Southern bounding-box limit. |
| `maxLatitude` | `double?` | `null` | Northern bounding-box limit. |
| `minLongitude` | `double?` | `null` | Western bounding-box limit. |
| `maxLongitude` | `double?` | `null` | Eastern bounding-box limit. |

Success response: `200 OK` with `ApiResponse<ClusterResponseDto>`, message `"Clusters retrieved successfully"`.

### GET /api/Clustering/heatmap

Returns a normalised density heatmap derived from location history. Each tile carries an intensity between `0.0` (no activity) and `1.0` (peak activity), relative to the most active tile in the response.

Query parameters are identical to `/clusters`.

Success response: `200 OK` with `ApiResponse<HeatmapResponseDto>`, message `"Heatmap computed successfully"`.

## Parameter handling

- `gridCellKm` is clamped in the controller with `Math.Max(0.1, gridCellKm)`. A smaller or negative value does not produce an error; it is silently treated as `0.1`.
- The `from`, `to`, and bounding-box values are passed to the service unchanged. The controller does not check that `from` is before `to` or that `minLatitude` is less than `maxLatitude`. Those checks, if any, happen in `IClusteringService`.
- The default time window is documented on the parameters but applied by the service when the values are `null`. The controller passes `null` through.

## Error handling

Both actions wrap the service call in a `try/catch (Exception)`. Any exception is logged with the request `TraceId` and converted to a response:

| Endpoint | Status | Body |
|---|---|---|
| `/clusters` | `400 Bad Request` | `ErrorResponse.ServerError("Failed to compute clusters", traceId)` |
| `/heatmap` | `400 Bad Request` | `ErrorResponse.ServerError("Failed to compute heatmap", traceId)` |

Note that service failures return `400`, not `500`, even though the message says the server failed. Clients should not assume a `400` means the request itself was malformed; read the error body and the `traceId` for details.

Framework-level binding and validation failures (for example, a non-numeric `vehicleId`) are handled by `[ApiController]` before the action runs and produce a standard validation-problem response.

Every response includes `HttpContext.TraceIdentifier` in the envelope, which can be used to correlate a client report with the server log entry.

## Usage

Clusters for one vehicle over the last 6 hours, with 1 km cells:

```http
GET /api/Clustering/clusters?vehicleId=42&from=2026-10-25T06:00:00Z&to=2026-10-25T12:00:00Z&gridCellKm=1
```

Heatmap for a bounding box covering a city, using the default 0.5 km cells and default time window:

```http
GET /api/Clustering/heatmap?minLatitude=50.40&maxLatitude=50.50&minLongitude=30.40&maxLongitude=30.60
```

Client-side in JavaScript:

```javascript
const params = new URLSearchParams({
  minLatitude: 50.40,
  maxLatitude: 50.50,
  minLongitude: 30.40,
  maxLongitude: 30.60,
  gridCellKm: 0.5,
});

const response = await fetch(`/api/Clustering/heatmap?${params}`);
const body = await response.json();

if (response.ok) {
  // body is ApiResponse<HeatmapResponseDto>
  console.log(body.message);
} else {
  // body is ErrorResponse
  console.error(body);
}
```

## Notes

- Both endpoints do a full computation per request. Large time windows or small `gridCellKm` values increase the work done on the server. Narrow `from`/`to` and bounding boxes where possible.
- The controller holds no mutable state; its only fields are the injected `IClusteringService` and `ILogger`. Thread-safety of each request therefore depends on the service implementation.
- The envelope types (`ApiResponse<T>`, `ErrorResponse`) and the DTOs (`ClusterResponseDto`, `HeatmapResponseDto`) are defined outside this file. Their field-level structure is not described here.
