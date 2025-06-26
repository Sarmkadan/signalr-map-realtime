// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Formatters;

using System.Text.Json;
using System.Text.Json.Serialization;
using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Serializes location data to GeoJSON format.
/// GeoJSON is a standard format for geographic data used by mapping libraries like Leaflet and Mapbox.
/// Enables integration with various mapping and GIS tools.
/// </summary>
public class GeoJsonSerializer
{
    /// <summary>
    /// Serializes a single location to GeoJSON Point feature.
    /// </summary>
    public static string SerializeLocation(Location location)
    {
        var feature = new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new GeoJsonPoint
            {
                Type = "Point",
                Coordinates = new[] { location.Longitude, location.Latitude }
            },
            Properties = new Dictionary<string, object>
            {
                { "id", location.Id },
                { "vehicleId", location.VehicleId },
                { "accuracy", location.Accuracy },
                { "altitude", location.Altitude },
                { "timestamp", location.CreatedAt },
                { "locationType", location.LocationType }
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return JsonSerializer.Serialize(feature, options);
    }

    /// <summary>
    /// Serializes multiple locations to GeoJSON FeatureCollection.
    /// Ideal for displaying multiple points on a map.
    /// </summary>
    public static string SerializeLocations(IEnumerable<Location> locations)
    {
        var features = locations.Select(loc => new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new GeoJsonPoint
            {
                Type = "Point",
                Coordinates = new[] { loc.Longitude, loc.Latitude }
            },
            Properties = new Dictionary<string, object>
            {
                { "id", loc.Id },
                { "vehicleId", loc.VehicleId },
                { "accuracy", loc.Accuracy },
                { "altitude", loc.Altitude },
                { "timestamp", loc.CreatedAt },
                { "locationType", loc.LocationType }
            }
        }).ToList();

        var featureCollection = new GeoJsonFeatureCollection
        {
            Type = "FeatureCollection",
            Features = features
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return JsonSerializer.Serialize(featureCollection, options);
    }

    /// <summary>
    /// Serializes a route (sequence of waypoints) to GeoJSON LineString feature.
    /// </summary>
    public static string SerializeRoute(Route route, IEnumerable<(double Latitude, double Longitude)> waypoints)
    {
        var coordinates = waypoints.Select(wp => new[] { wp.Longitude, wp.Latitude }).ToArray();

        var feature = new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new GeoJsonLineString
            {
                Type = "LineString",
                Coordinates = coordinates
            },
            Properties = new Dictionary<string, object>
            {
                { "id", route.Id },
                { "name", route.Name },
                { "description", route.Description },
                { "status", route.Status },
                { "createdAt", route.CreatedAt },
                { "waypointCount", coordinates.Length }
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return JsonSerializer.Serialize(feature, options);
    }

    /// <summary>
    /// Serializes a geofence circle to GeoJSON representation.
    /// </summary>
    public static string SerializeGeofence(Guid geofenceId, double latitude, double longitude, double radiusMeters)
    {
        var feature = new GeoJsonFeature
        {
            Type = "Feature",
            Geometry = new GeoJsonPoint
            {
                Type = "Point",
                Coordinates = new[] { longitude, latitude }
            },
            Properties = new Dictionary<string, object>
            {
                { "id", geofenceId },
                { "type", "Geofence" },
                { "radiusMeters", radiusMeters },
                { "center", new { latitude, longitude } }
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return JsonSerializer.Serialize(feature, options);
    }
}

/// <summary>
/// GeoJSON Feature structure.
/// </summary>
public class GeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Feature";

    [JsonPropertyName("geometry")]
    public object? Geometry { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// GeoJSON FeatureCollection structure.
/// </summary>
public class GeoJsonFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "FeatureCollection";

    [JsonPropertyName("features")]
    public List<GeoJsonFeature> Features { get; set; } = new();
}

/// <summary>
/// GeoJSON Point geometry.
/// </summary>
public class GeoJsonPoint
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Point";

    [JsonPropertyName("coordinates")]
    public double[] Coordinates { get; set; } = Array.Empty<double>();
}

/// <summary>
/// GeoJSON LineString geometry.
/// </summary>
public class GeoJsonLineString
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "LineString";

    [JsonPropertyName("coordinates")]
    public double[][] Coordinates { get; set; } = Array.Empty<double[]>();
}
