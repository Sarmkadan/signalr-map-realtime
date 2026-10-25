# LocationOptions

`LocationOptions` is a configuration class in the `SignalRMapRealtime.Configuration` namespace. It holds location-related thresholds and the history retention period. Values are bound from configuration under the `LocationOptions` section (`LocationOptions.SectionName`). Any setting that is not configured keeps the default shown below.

Each property is annotated with a data-annotation `[Range]` attribute, so invalid values are rejected when options are validated at startup.

## Configuration section

```json
{
  "LocationOptions": {
    "MinLocationChangeKm": 0.05,
    "MinAccuracyMeters": 50.0,
    "MaxAllowedSpeed": 200.0,
    "DefaultGeofenceRadiusKm": 0.5,
    "LocationRetentionDays": 90,
    "MinPointsForAnalysis": 5
  }
}
```

## API

### `SectionName`
The configuration section name, `"LocationOptions"`.

- **Type:** `const string`

### `MinLocationChangeKm`
Minimum distance difference, in kilometers, for a location to be considered as moved.

- **Type:** `double`
- **Default:** `0.05` (50 meters)
- **Validation:** `[Range(0, double.MaxValue)]`

### `MinAccuracyMeters`
Minimum accuracy threshold, in meters, for accepting a location.

- **Type:** `double`
- **Default:** `50.0`
- **Validation:** `[Range(0, double.MaxValue)]`

### `MaxAllowedSpeed`
Maximum allowed speed on a route, in km/h.

- **Type:** `double`
- **Default:** `200.0`
- **Validation:** `[Range(0, double.MaxValue)]`

### `DefaultGeofenceRadiusKm`
Default geofence radius, in kilometers.

- **Type:** `double`
- **Default:** `0.5` (500 meters)
- **Validation:** `[Range(0, double.MaxValue)]`

### `LocationRetentionDays`
Number of days to retain location history.

- **Type:** `int`
- **Default:** `90`
- **Validation:** `[Range(1, int.MaxValue)]`

### `MinPointsForAnalysis`
Minimum number of points required for accurate route analysis.

- **Type:** `int`
- **Default:** `5`
- **Validation:** `[Range(1, int.MaxValue)]`
