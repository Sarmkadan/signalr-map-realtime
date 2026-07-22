#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Formatters;

using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Serializes location data to GeoJSON format.
/// GeoJSON is a standard format for geographic data used by mapping libraries like Leaflet and Mapbox.
/// Enables integration with various mapping and GIS tools.
/// Uses Utf8JsonWriter for efficient serialization of large feature collections.
/// </summary>
public class GeoJsonSerializer
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a single location to GeoJSON Point feature.
    /// </summary>
    public static string SerializeLocation(Location location)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            WriteFeature(writer, location);
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Serializes multiple locations to GeoJSON FeatureCollection.
    /// Ideal for displaying multiple points on a map.
    /// </summary>
    public static string SerializeLocations(IEnumerable<Location> locations)
    {
        if (locations == null)
            throw new ArgumentNullException(nameof(locations));

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "FeatureCollection");
            writer.WriteStartArray("features");

            foreach (var location in locations)
            {
                WriteFeature(writer, location);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Serializes a route (sequence of waypoints) to GeoJSON LineString feature.
    /// </summary>
    public static string SerializeRoute(Route route, IEnumerable<(double Latitude, double Longitude)> waypoints)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));
        if (waypoints == null)
            throw new ArgumentNullException(nameof(waypoints));

        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "Feature");

            // Write geometry
            writer.WriteStartObject("geometry");
            writer.WriteString("type", "LineString");
            writer.WriteStartArray("coordinates");

            foreach (var wp in waypoints)
            {
                writer.WriteStartArray();
                writer.WriteNumberValue(wp.Longitude);
                writer.WriteNumberValue(wp.Latitude);
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
            writer.WriteEndObject(); // geometry

            // Write properties
            writer.WriteStartObject("properties");
            writer.WriteNumber("id", route.Id);
            if (route.Name != null)
                writer.WriteString("name", route.Name);
            if (route.Description != null)
                writer.WriteString("description", route.Description);
            writer.WriteString("status", route.IsActive ? "active" : (route.IsCompleted ? "completed" : "inactive"));
            writer.WriteString("createdAt", route.CreatedAt);
            writer.WriteNumber("waypointCount", waypoints.Count());
            writer.WriteEndObject(); // properties

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Serializes a geofence circle to GeoJSON representation.
    /// </summary>
    public static string SerializeGeofence(Guid geofenceId, double latitude, double longitude, double radiusMeters)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "Feature");

            // Write geometry
            writer.WriteStartObject("geometry");
            writer.WriteString("type", "Point");
            writer.WriteStartArray("coordinates");
            writer.WriteNumberValue(longitude);
            writer.WriteNumberValue(latitude);
            writer.WriteEndArray();
            writer.WriteEndObject(); // geometry

            // Write properties
            writer.WriteStartObject("properties");
            writer.WriteString("id", geofenceId.ToString());
            writer.WriteString("type", "Geofence");
            writer.WriteNumber("radiusMeters", radiusMeters);
            writer.WriteStartObject("center");
            writer.WriteNumber("latitude", latitude);
            writer.WriteNumber("longitude", longitude);
            writer.WriteEndObject();
            writer.WriteEndObject(); // properties

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Writes a GeoJSON Feature for a location using Utf8JsonWriter.
    /// </summary>
    private static void WriteFeature(Utf8JsonWriter writer, Location location)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "Feature");

        // Write geometry
        writer.WriteStartObject("geometry");
        writer.WriteString("type", "Point");
        writer.WriteStartArray("coordinates");
        writer.WriteNumberValue(location.Longitude);
        writer.WriteNumberValue(location.Latitude);
        writer.WriteEndArray();
        writer.WriteEndObject(); // geometry

        // Write properties
        writer.WriteStartObject("properties");
        writer.WriteNumber("id", location.Id);
        writer.WriteNumber("vehicleId", location.VehicleId);
        if (location.Accuracy.HasValue)
            writer.WriteNumber("accuracy", location.Accuracy.Value);
        if (location.Altitude.HasValue)
            writer.WriteNumber("altitude", location.Altitude.Value);
        writer.WriteString("timestamp", location.CreatedAt);
        writer.WriteString("locationType", location.LocationType.ToString());
        writer.WriteEndObject(); // properties

        writer.WriteEndObject();
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
        public double[] Coordinates { get; set; } = new double[0];
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
}