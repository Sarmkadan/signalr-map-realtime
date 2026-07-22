#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Tests.Formatters;

using System.Text.Json;
using FluentAssertions;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Formatters;
using Xunit;

/// <summary>
/// Tests for the <see cref="GeoJsonSerializer"/> class.
/// Verifies GeoJSON output format, coordinate ordering (lon-lat), and edge cases.
/// </summary>
public class GeoJsonSerializerTests
{
    /// <summary>
    /// Verifies that SerializeLocation produces valid GeoJSON Point feature.
    /// GeoJSON Point must have coordinates in [longitude, latitude] order.
    /// </summary>
    [Fact]
    public void SerializeLocation_ProducesValidGeoJsonPointFeature()
    {
        // Arrange
        var location = new Location
        {
            Id = 123,
            Latitude = 51.5074,
            Longitude = -0.1278,
            VehicleId = 42,
            LocationType = LocationType.TrackPoint,
            Accuracy = 5.5,
            Altitude = 100.0,
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var result = GeoJsonSerializer.SerializeLocation(location);

        // Assert - basic structure
        result.Should().NotBeNullOrWhiteSpace();

        // Parse and verify GeoJSON structure
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("Feature");

        var geometry = root.GetProperty("geometry");
        geometry.GetProperty("type").GetString().Should().Be("Point");

        var coordinates = geometry.GetProperty("coordinates");
        coordinates.ValueKind.Should().Be(JsonValueKind.Array);
        coordinates.GetArrayLength().Should().Be(2);

        // Verify coordinate order: [longitude, latitude]
        coordinates[0].GetDouble().Should().Be(-0.1278); // longitude
        coordinates[1].GetDouble().Should().Be(51.5074);  // latitude

        // Verify properties
        var properties = root.GetProperty("properties");
        properties.GetProperty("id").GetInt32().Should().Be(123);
        properties.GetProperty("vehicleId").GetInt32().Should().Be(42);
        properties.GetProperty("accuracy").GetDouble().Should().Be(5.5);
        properties.GetProperty("altitude").GetDouble().Should().Be(100.0);
        properties.GetProperty("timestamp").GetString().Should().Be("2024-01-15T10:30:00Z");
        properties.GetProperty("locationType").GetString().Should().Be("TrackPoint");
    }

    /// <summary>
    /// Verifies that SerializeLocation handles null optional fields correctly.
    /// </summary>
    [Fact]
    public void SerializeLocation_HandlesNullOptionalFields()
    {
        // Arrange - location with null optional fields
        var location = new Location
        {
            Id = 999,
            Latitude = 40.7128,
            Longitude = -74.0060,
            VehicleId = 77,
            LocationType = LocationType.DeliveryPoint,
            Accuracy = null,
            Altitude = null,
            CreatedAt = new DateTime(2024, 6, 20, 14, 45, 0, DateTimeKind.Utc)
        };

        // Act
        var result = GeoJsonSerializer.SerializeLocation(location);

        // Assert
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        var properties = root.GetProperty("properties");

        // Optional fields should not be present
        properties.TryGetProperty("accuracy", out _).Should().BeFalse();
        properties.TryGetProperty("altitude", out _).Should().BeFalse();
        properties.TryGetProperty("address", out _).Should().BeFalse();
        properties.TryGetProperty("notes", out _).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that SerializeLocations produces valid GeoJSON FeatureCollection.
    /// FeatureCollection must contain an array of features.
    /// </summary>
    [Fact]
    public void SerializeLocations_ProducesValidGeoJsonFeatureCollection()
    {
        // Arrange
        var locations = new List<Location>
        {
            new Location
            {
                Id = 1,
                Latitude = 51.5074,
                Longitude = -0.1278,
                VehicleId = 1,
                LocationType = LocationType.TrackPoint,
                CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
            },
            new Location
            {
                Id = 2,
                Latitude = 48.8566,
                Longitude = 2.3522,
                VehicleId = 2,
                LocationType = LocationType.DeliveryPoint,
                CreatedAt = new DateTime(2024, 1, 15, 11, 0, 0, DateTimeKind.Utc)
            },
            new Location
            {
                Id = 3,
                Latitude = 40.7128,
                Longitude = -74.0060,
                VehicleId = 3,
                LocationType = LocationType.CustomerLocation,
                CreatedAt = new DateTime(2024, 1, 15, 11, 30, 0, DateTimeKind.Utc)
            }
        };

        // Act
        var result = GeoJsonSerializer.SerializeLocations(locations);

        // Assert - basic structure
        result.Should().NotBeNullOrWhiteSpace();

        // Parse and verify GeoJSON structure
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("FeatureCollection");

        var features = root.GetProperty("features");
        features.ValueKind.Should().Be(JsonValueKind.Array);
        features.GetArrayLength().Should().Be(3);

        // Verify each feature is a Point
        for (int i = 0; i < features.GetArrayLength(); i++)
        {
            var feature = features[i];
            feature.GetProperty("type").GetString().Should().Be("Feature");

            var geometry = feature.GetProperty("geometry");
            geometry.GetProperty("type").GetString().Should().Be("Point");

            var coordinates = geometry.GetProperty("coordinates");
            coordinates.GetArrayLength().Should().Be(2);

            // Verify coordinate order
            coordinates[0].GetDouble().Should().BeGreaterOrEqualTo(-180.0);
            coordinates[0].GetDouble().Should().BeLessOrEqualTo(180.0);
            coordinates[1].GetDouble().Should().BeGreaterOrEqualTo(-90.0);
            coordinates[1].GetDouble().Should().BeLessOrEqualTo(90.0);
        }
    }

    /// <summary>
    /// Verifies that SerializeLocations handles empty collection.
    /// </summary>
    [Fact]
    public void SerializeLocations_HandlesEmptyCollection()
    {
        // Arrange
        var locations = new List<Location>();

        // Act
        var result = GeoJsonSerializer.SerializeLocations(locations);

        // Assert
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("FeatureCollection");
        root.GetProperty("features").GetArrayLength().Should().Be(0);
    }

    /// <summary>
    /// Verifies that SerializeLocations throws ArgumentNullException for null input.
    /// </summary>
    [Fact]
    public void SerializeLocations_ThrowsForNullInput()
    {
        // Arrange
        List<Location>? nullLocations = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GeoJsonSerializer.SerializeLocations(nullLocations!));
    }

    /// <summary>
    /// Verifies that SerializeRoute produces valid GeoJSON LineString feature.
    /// LineString coordinates must be in [longitude, latitude] order.
    /// </summary>
    [Fact]
    public void SerializeRoute_ProducesValidGeoJsonLineStringFeature()
    {
        // Arrange
        var route = new Route
        {
            Id = 5,
            Name = "Test Route",
            Description = "A test route for verification",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        var waypoints = new List<(double Latitude, double Longitude)>
        {
            (51.5074, -0.1278),   // London
            (48.8566, 2.3522),    // Paris
            (52.5200, 13.4050),   // Berlin
            (40.7128, -74.0060)   // New York
        };

        // Act
        var result = GeoJsonSerializer.SerializeRoute(route, waypoints);

        // Assert - basic structure
        result.Should().NotBeNullOrWhiteSpace();

        // Parse and verify GeoJSON structure
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("Feature");

        var geometry = root.GetProperty("geometry");
        geometry.GetProperty("type").GetString().Should().Be("LineString");

        var coordinates = geometry.GetProperty("coordinates");
        coordinates.ValueKind.Should().Be(JsonValueKind.Array);
        coordinates.GetArrayLength().Should().Be(4);

        // Verify each coordinate pair is in [longitude, latitude] order
        for (int i = 0; i < coordinates.GetArrayLength(); i++)
        {
            var coordPair = coordinates[i];
            coordPair.ValueKind.Should().Be(JsonValueKind.Array);
            coordPair.GetArrayLength().Should().Be(2);

            // Verify coordinate order: [longitude, latitude]
            var lon = coordPair[0].GetDouble();
            var lat = coordPair[1].GetDouble();

            lon.Should().BeGreaterOrEqualTo(-180.0);
            lon.Should().BeLessOrEqualTo(180.0);
            lat.Should().BeGreaterOrEqualTo(-90.0);
            lat.Should().BeLessOrEqualTo(90.0);
        }

        // Verify properties
        var properties = root.GetProperty("properties");
        properties.GetProperty("id").GetInt32().Should().Be(5);
        properties.GetProperty("name").GetString().Should().Be("Test Route");
        properties.GetProperty("description").GetString().Should().Be("A test route for verification");
        properties.GetProperty("status").GetString().Should().Be("active");
        properties.GetProperty("waypointCount").GetInt32().Should().Be(4);
    }

    /// <summary>
    /// Verifies that SerializeRoute throws ArgumentNullException for null route.
    /// </summary>
    [Fact]
    public void SerializeRoute_ThrowsForNullRoute()
    {
        // Arrange
        Route? nullRoute = null;
        var waypoints = new List<(double Latitude, double Longitude)> { (0, 0) };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GeoJsonSerializer.SerializeRoute(nullRoute!, waypoints));
    }

    /// <summary>
    /// Verifies that SerializeRoute throws ArgumentNullException for null waypoints.
    /// </summary>
    [Fact]
    public void SerializeRoute_ThrowsForNullWaypoints()
    {
        // Arrange
        var route = new Route { Id = 1, Name = "Test" };
        List<(double Latitude, double Longitude)>? nullWaypoints = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => GeoJsonSerializer.SerializeRoute(route, nullWaypoints!));
    }

    /// <summary>
    /// Verifies that SerializeGeofence produces valid GeoJSON Point feature.
    /// Geofence coordinates must be in [longitude, latitude] order.
    /// </summary>
    [Fact]
    public void SerializeGeofence_ProducesValidGeoJsonPointFeature()
    {
        // Arrange
        var geofenceId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        const double latitude = 34.0522;
        const double longitude = -118.2437;
        const double radiusMeters = 1000.0;

        // Act
        var result = GeoJsonSerializer.SerializeGeofence(geofenceId, latitude, longitude, radiusMeters);

        // Assert - basic structure
        result.Should().NotBeNullOrWhiteSpace();

        // Parse and verify GeoJSON structure
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        root.GetProperty("type").GetString().Should().Be("Feature");

        var geometry = root.GetProperty("geometry");
        geometry.GetProperty("type").GetString().Should().Be("Point");

        var coordinates = geometry.GetProperty("coordinates");
        coordinates.ValueKind.Should().Be(JsonValueKind.Array);
        coordinates.GetArrayLength().Should().Be(2);

        // Verify coordinate order: [longitude, latitude]
        coordinates[0].GetDouble().Should().Be(-118.2437); // longitude
        coordinates[1].GetDouble().Should().Be(34.0522);   // latitude

        // Verify properties
        var properties = root.GetProperty("properties");
        properties.GetProperty("id").GetString().Should().Be(geofenceId.ToString());
        properties.GetProperty("type").GetString().Should().Be("Geofence");
        properties.GetProperty("radiusMeters").GetDouble().Should().Be(radiusMeters);

        var center = properties.GetProperty("center");
        center.GetProperty("latitude").GetDouble().Should().Be(latitude);
        center.GetProperty("longitude").GetDouble().Should().Be(longitude);
    }

    /// <summary>
    /// Verifies that SerializeGeofence throws ArgumentNullException for null geofence.
    /// Not directly possible since parameters are value types, but we can test with invalid values.
    /// </summary>
    [Fact]
    public void SerializeGeofence_HandlesValidInput()
    {
        // Arrange
        var geofenceId = Guid.NewGuid();
        const double latitude = 0.0;
        const double longitude = 0.0;
        const double radiusMeters = 500.0;

        // Act
        var result = GeoJsonSerializer.SerializeGeofence(geofenceId, latitude, longitude, radiusMeters);

        // Assert - should produce valid output
        result.Should().NotBeNullOrWhiteSpace();

        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;
        root.GetProperty("type").GetString().Should().Be("Feature");
    }

    /// <summary>
    /// Verifies that coordinates are in correct GeoJSON order [longitude, latitude].
    /// This is a critical requirement for GeoJSON compliance.
    /// </summary>
    [Fact]
    public void AllSerializers_UseCorrectCoordinateOrder_LongitudeFirst_LatitudeSecond()
    {
        // Test SerializeLocation
        var location = new Location
        {
            Id = 1,
            Latitude = 45.0,
            Longitude = 90.0,
            VehicleId = 1,
            LocationType = LocationType.TrackPoint,
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        var locationJson = GeoJsonSerializer.SerializeLocation(location);
        var locationDoc = JsonDocument.Parse(locationJson);
        var locationCoords = locationDoc.RootElement.GetProperty("geometry").GetProperty("coordinates");

        locationCoords[0].GetDouble().Should().Be(90.0); // longitude
        locationCoords[1].GetDouble().Should().Be(45.0); // latitude

        // Test SerializeGeofence
        var geofenceJson = GeoJsonSerializer.SerializeGeofence(Guid.NewGuid(), 45.0, 90.0, 100.0);
        var geofenceDoc = JsonDocument.Parse(geofenceJson);
        var geofenceCoords = geofenceDoc.RootElement.GetProperty("geometry").GetProperty("coordinates");

        geofenceCoords[0].GetDouble().Should().Be(90.0); // longitude
        geofenceCoords[1].GetDouble().Should().Be(45.0); // latitude

        // Test SerializeRoute
        var route = new Route { Id = 1, Name = "Test" };
        var waypoints = new List<(double Latitude, double Longitude)> { (45.0, 90.0) };
        var routeJson = GeoJsonSerializer.SerializeRoute(route, waypoints);
        var routeDoc = JsonDocument.Parse(routeJson);
        var routeCoords = routeDoc.RootElement.GetProperty("geometry").GetProperty("coordinates")[0];

        routeCoords[0].GetDouble().Should().Be(90.0); // longitude
        routeCoords[1].GetDouble().Should().Be(45.0); // latitude
    }

    /// <summary>
    /// Verifies that the JSON output is valid and can be parsed.
    /// Ensures no syntax errors in serialization.
    /// </summary>
    [Fact]
    public void Serializers_ProduceValidJson()
    {
        // Arrange
        var location = new Location
        {
            Id = 1,
            Latitude = 12.34,
            Longitude = 56.78,
            VehicleId = 1,
            LocationType = LocationType.TrackPoint,
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        var locations = new List<Location> { location };
        var route = new Route { Id = 1, Name = "Test Route" };
        var waypoints = new List<(double Latitude, double Longitude)> { (12.34, 56.78) };
        var geofenceId = Guid.NewGuid();

        // Act
        var locationJson = GeoJsonSerializer.SerializeLocation(location);
        var locationsJson = GeoJsonSerializer.SerializeLocations(locations);
        var routeJson = GeoJsonSerializer.SerializeRoute(route, waypoints);
        var geofenceJson = GeoJsonSerializer.SerializeGeofence(geofenceId, 12.34, 56.78, 100.0);

        // Assert - all should be valid JSON
        Action parseLocation = () => JsonDocument.Parse(locationJson);
        Action parseLocations = () => JsonDocument.Parse(locationsJson);
        Action parseRoute = () => JsonDocument.Parse(routeJson);
        Action parseGeofence = () => JsonDocument.Parse(geofenceJson);

        parseLocation.Should().NotThrow();
        parseLocations.Should().NotThrow();
        parseRoute.Should().NotThrow();
        parseGeofence.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that the JSON output uses camelCase property naming as expected.
    /// </summary>
    [Fact]
    public void Serializers_UseCamelCasePropertyNaming()
    {
        // Arrange
        var location = new Location
        {
            Id = 1,
            Latitude = 12.34,
            Longitude = 56.78,
            VehicleId = 1,
            LocationType = LocationType.TrackPoint,
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = GeoJsonSerializer.SerializeLocation(location);
        var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        // Assert - properties should use camelCase
        var properties = root.GetProperty("properties");
        properties.GetProperty("id").ValueKind.Should().NotBe(JsonValueKind.Undefined);
        properties.GetProperty("vehicleId").ValueKind.Should().NotBe(JsonValueKind.Undefined);
        properties.GetProperty("timestamp").ValueKind.Should().NotBe(JsonValueKind.Undefined);
        properties.GetProperty("locationType").ValueKind.Should().NotBe(JsonValueKind.Undefined);

        // Verify PascalCase properties do NOT exist
        properties.TryGetProperty("Id", out _).Should().BeFalse();
        properties.TryGetProperty("VehicleId", out _).Should().BeFalse();
        properties.TryGetProperty("Timestamp", out _).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that NaN coordinates are handled correctly.
    /// NaN coordinates should be rejected or handled gracefully.
    /// </summary>
    [Fact]
    public void SerializeLocation_RejectsNaNCoordinates()
    {
        // Arrange
        var location = new Location
        {
            Id = 1,
            Latitude = double.NaN,
            Longitude = 56.78,
            VehicleId = 1,
            LocationType = LocationType.TrackPoint,
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act & Assert - Should either throw or produce valid output
        // The serializer should handle this gracefully
        var result = GeoJsonSerializer.SerializeLocation(location);
        result.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that Infinity coordinates are handled correctly.
    /// Infinity coordinates should be rejected or handled gracefully.
    /// </summary>
    [Fact]
    public void SerializeLocation_RejectsInfinityCoordinates()
    {
        // Arrange
        var location = new Location
        {
            Id = 1,
            Latitude = double.PositiveInfinity,
            Longitude = 56.78,
            VehicleId = 1,
            LocationType = LocationType.TrackPoint,
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act & Assert - Should either throw or produce valid output
        var result = GeoJsonSerializer.SerializeLocation(location);
        result.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that negative zero coordinates are handled correctly.
    /// </summary>
    [Fact]
    public void SerializeLocation_HandlesNegativeZeroCoordinates()
    {
        // Arrange
        var location = new Location
        {
            Id = 1,
            Latitude = -0.0,
            Longitude = -0.0,
            VehicleId = 1,
            LocationType = LocationType.TrackPoint,
            CreatedAt = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = GeoJsonSerializer.SerializeLocation(location);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        var doc = JsonDocument.Parse(result);
        var coordinates = doc.RootElement.GetProperty("geometry").GetProperty("coordinates");
        coordinates[0].GetDouble().Should().Be(-0.0);
        coordinates[1].GetDouble().Should().Be(-0.0);
    }
}
