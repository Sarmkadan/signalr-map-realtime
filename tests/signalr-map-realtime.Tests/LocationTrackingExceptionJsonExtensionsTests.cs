using Xunit;
using System.Text.Json;
using SignalRMapRealtime.Exceptions;

namespace signalr_map_realtime.Tests
{
    public class LocationTrackingExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_VehicleNotFoundException_IncludesTypeDiscriminatorAndVehicleId()
        {
            // Arrange
            var vehicleId = 123;
            var exception = new VehicleNotFoundException(vehicleId);

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.NotNull(json);
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);

            Assert.True(element.TryGetProperty("$type", out var typeProperty));
            Assert.Equal("VehicleNotFoundException", typeProperty.GetString());

            Assert.True(element.TryGetProperty("vehicleId", out var vehicleIdProperty));
            Assert.Equal(vehicleId, vehicleIdProperty.GetInt32());

            Assert.True(element.TryGetProperty("message", out var messageProperty));
            Assert.Contains(vehicleId.ToString(), messageProperty.GetString());
        }

        [Fact]
        public void ToJson_AssetNotFoundException_IncludesTypeDiscriminatorAndAssetId()
        {
            // Arrange
            var assetId = 456;
            var exception = new AssetNotFoundException(assetId);

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.True(element.TryGetProperty("$type", out var typeProperty));
            Assert.Equal("AssetNotFoundException", typeProperty.GetString());

            Assert.True(element.TryGetProperty("assetId", out var assetIdProperty));
            Assert.Equal(assetId, assetIdProperty.GetInt32());
        }

        [Fact]
        public void ToJson_TrackingSessionNotFoundException_IncludesTypeDiscriminatorAndSessionId()
        {
            // Arrange
            var sessionId = 789;
            var exception = new TrackingSessionNotFoundException(sessionId);

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.True(element.TryGetProperty("$type", out var typeProperty));
            Assert.Equal("TrackingSessionNotFoundException", typeProperty.GetString());

            Assert.True(element.TryGetProperty("sessionId", out var sessionIdProperty));
            Assert.Equal(sessionId, sessionIdProperty.GetInt32());
        }

        [Fact]
        public void ToJson_InvalidLocationException_IncludesTypeDiscriminatorAndCoordinates()
        {
            // Arrange
            var latitude = 10.5;
            var longitude = -20.3;
            var exception = new InvalidLocationException(latitude, longitude);

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.True(element.TryGetProperty("$type", out var typeProperty));
            Assert.Equal("InvalidLocationException", typeProperty.GetString());

            Assert.True(element.TryGetProperty("latitude", out var latitudeProperty));
            Assert.Equal(latitude, latitudeProperty.GetDouble());

            Assert.True(element.TryGetProperty("longitude", out var longitudeProperty));
            Assert.Equal(longitude, longitudeProperty.GetDouble());
        }

        [Fact]
        public void ToJson_LocationTrackingExceptionBase_IncludesTypeDiscriminator()
        {
            // Arrange
            var exception = new LocationTrackingException("Base exception");

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.True(element.TryGetProperty("$type", out var typeProperty));
            Assert.Equal("LocationTrackingException", typeProperty.GetString());

            Assert.True(element.TryGetProperty("message", out var messageProperty));
            Assert.Equal("Base exception", messageProperty.GetString());
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => LocationTrackingExceptionJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_VehicleNotFoundException_RoundTripPreservesProperties()
        {
            // Arrange
            var vehicleId = 999;
            var exception = new VehicleNotFoundException(vehicleId);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<VehicleNotFoundException>(deserialized);
            Assert.Equal(vehicleId, ((VehicleNotFoundException)deserialized).VehicleId);
            Assert.Equal(exception.Message, deserialized.Message);
        }

        [Fact]
        public void FromJson_AssetNotFoundException_RoundTripPreservesProperties()
        {
            // Arrange
            var assetId = 888;
            var exception = new AssetNotFoundException(assetId);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<AssetNotFoundException>(deserialized);
            Assert.Equal(assetId, ((AssetNotFoundException)deserialized).AssetId);
            Assert.Equal(exception.Message, deserialized.Message);
        }

        [Fact]
        public void FromJson_TrackingSessionNotFoundException_RoundTripPreservesProperties()
        {
            // Arrange
            var sessionId = 777;
            var exception = new TrackingSessionNotFoundException(sessionId);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<TrackingSessionNotFoundException>(deserialized);
            Assert.Equal(sessionId, ((TrackingSessionNotFoundException)deserialized).SessionId);
            Assert.Equal(exception.Message, deserialized.Message);
        }

        [Fact]
        public void FromJson_InvalidLocationException_RoundTripPreservesProperties()
        {
            // Arrange
            var latitude = 45.123;
            var longitude = -122.456;
            var exception = new InvalidLocationException(latitude, longitude);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<InvalidLocationException>(deserialized);
            var locationEx = (InvalidLocationException)deserialized;
            Assert.Equal(latitude, locationEx.Latitude);
            Assert.Equal(longitude, locationEx.Longitude);
            Assert.Equal(exception.Message, deserialized.Message);
        }

        [Fact]
        public void FromJson_LocationTrackingExceptionBase_RoundTripPreservesMessage()
        {
            // Arrange
            var exception = new LocationTrackingException("Base exception message");
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<LocationTrackingException>(deserialized);
            Assert.Equal("Base exception message", deserialized.Message);
        }

        [Fact]
        public void FromJson_NullInput_ReturnsNull()
        {
            // Act
            var exception = LocationTrackingExceptionJsonExtensions.FromJson(null);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var exception = LocationTrackingExceptionJsonExtensions.FromJson("");

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void FromJson_MalformedJson_ReturnsNull()
        {
            // Act
            var exception = LocationTrackingExceptionJsonExtensions.FromJson("{ invalid json");

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void TryFromJson_VehicleNotFoundException_ReturnsTrueAndDeserializes()
        {
            // Arrange
            var vehicleId = 555;
            var exception = new VehicleNotFoundException(vehicleId);
            var json = exception.ToJson();

            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserialized);
            Assert.IsType<VehicleNotFoundException>(deserialized);
            Assert.Equal(vehicleId, ((VehicleNotFoundException)deserialized).VehicleId);
        }

        [Fact]
        public void TryFromJson_AssetNotFoundException_ReturnsTrueAndDeserializes()
        {
            // Arrange
            var assetId = 666;
            var exception = new AssetNotFoundException(assetId);
            var json = exception.ToJson();

            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserialized);
            Assert.IsType<AssetNotFoundException>(deserialized);
            Assert.Equal(assetId, ((AssetNotFoundException)deserialized).AssetId);
        }

        [Fact]
        public void TryFromJson_TrackingSessionNotFoundException_ReturnsTrueAndDeserializes()
        {
            // Arrange
            var sessionId = 333;
            var exception = new TrackingSessionNotFoundException(sessionId);
            var json = exception.ToJson();

            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserialized);
            Assert.IsType<TrackingSessionNotFoundException>(deserialized);
            Assert.Equal(sessionId, ((TrackingSessionNotFoundException)deserialized).SessionId);
        }

        [Fact]
        public void TryFromJson_InvalidLocationException_ReturnsTrueAndDeserializes()
        {
            // Arrange
            var latitude = 37.7749;
            var longitude = -122.4194;
            var exception = new InvalidLocationException(latitude, longitude);
            var json = exception.ToJson();

            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserialized);
            Assert.IsType<InvalidLocationException>(deserialized);
            var locationEx = (InvalidLocationException)deserialized;
            Assert.Equal(latitude, locationEx.Latitude);
            Assert.Equal(longitude, locationEx.Longitude);
        }

        [Fact]
        public void TryFromJson_NullInput_ReturnsFalse()
        {
            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(null, out _);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson("", out _);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void TryFromJson_MalformedJson_ReturnsFalse()
        {
            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson("{ invalid", out _);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void ToJson_WithIndentedFlag_ProducesFormattedJson()
        {
            // Arrange
            var exception = new VehicleNotFoundException(123);

            // Act
            var json = exception.ToJson(true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("VehicleNotFoundException", json);
            Assert.Contains("vehicleId", json);
            Assert.Contains("message", json);
        }

        [Fact]
        public void FromJson_WithCustomMessage_PreservesMessage()
        {
            // Arrange
            var vehicleId = 111;
            var customMessage = "Custom vehicle not found error message";
            var exception = new VehicleNotFoundException(vehicleId, customMessage);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<VehicleNotFoundException>(deserialized);
            Assert.Equal(customMessage, deserialized.Message);
            Assert.Equal(vehicleId, ((VehicleNotFoundException)deserialized).VehicleId);
        }

        [Fact]
        public void ToJson_VehicleNotFoundException_WithZeroVehicleId_ProducesValidJson()
        {
            // Arrange
            var vehicleId = 0;
            var exception = new VehicleNotFoundException(vehicleId);

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.NotNull(json);
            Assert.True(element.TryGetProperty("vehicleId", out var vehicleIdProperty));
            Assert.Equal(vehicleId, vehicleIdProperty.GetInt32());
        }

        [Fact]
        public void ToJson_AssetNotFoundException_WithZeroAssetId_ProducesValidJson()
        {
            // Arrange
            var assetId = 0;
            var exception = new AssetNotFoundException(assetId);

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.NotNull(json);
            Assert.True(element.TryGetProperty("assetId", out var assetIdProperty));
            Assert.Equal(assetId, assetIdProperty.GetInt32());
        }

        [Fact]
        public void ToJson_TrackingSessionNotFoundException_WithZeroSessionId_ProducesValidJson()
        {
            // Arrange
            var sessionId = 0;
            var exception = new TrackingSessionNotFoundException(sessionId);

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.NotNull(json);
            Assert.True(element.TryGetProperty("sessionId", out var sessionIdProperty));
            Assert.Equal(sessionId, sessionIdProperty.GetInt32());
        }

        [Fact]
        public void ToJson_InvalidLocationException_WithNullCoordinates_ProducesValidJson()
        {
            // Arrange
            var exception = new InvalidLocationException("Location data is missing");

            // Act
            var json = exception.ToJson();
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            Assert.NotNull(json);
            Assert.True(element.TryGetProperty("latitude", out var latitudeProperty));
            Assert.True(element.TryGetProperty("longitude", out var longitudeProperty));
            Assert.Equal(JsonValueKind.Null, latitudeProperty.ValueKind);
            Assert.Equal(JsonValueKind.Null, longitudeProperty.ValueKind);
        }

        [Fact]
        public void FromJson_VehicleNotFoundException_WithZeroVehicleId_RoundTripPreservesZero()
        {
            // Arrange
            var vehicleId = 0;
            var exception = new VehicleNotFoundException(vehicleId);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<VehicleNotFoundException>(deserialized);
            Assert.Equal(vehicleId, ((VehicleNotFoundException)deserialized).VehicleId);
        }

        [Fact]
        public void FromJson_AssetNotFoundException_WithZeroAssetId_RoundTripPreservesZero()
        {
            // Arrange
            var assetId = 0;
            var exception = new AssetNotFoundException(assetId);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<AssetNotFoundException>(deserialized);
            Assert.Equal(assetId, ((AssetNotFoundException)deserialized).AssetId);
        }

        [Fact]
        public void FromJson_TrackingSessionNotFoundException_WithZeroSessionId_RoundTripPreservesZero()
        {
            // Arrange
            var sessionId = 0;
            var exception = new TrackingSessionNotFoundException(sessionId);
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<TrackingSessionNotFoundException>(deserialized);
            Assert.Equal(sessionId, ((TrackingSessionNotFoundException)deserialized).SessionId);
        }

        [Fact]
        public void FromJson_InvalidLocationException_WithNullCoordinates_RoundTripPreservesNulls()
        {
            // Arrange
            var exception = new InvalidLocationException("Location data is missing");
            var json = exception.ToJson();

            // Act
            var deserialized = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<InvalidLocationException>(deserialized);
            var locationEx = (InvalidLocationException)deserialized;
            Assert.Null(locationEx.Latitude);
            Assert.Null(locationEx.Longitude);
        }

        [Fact]
        public void ToJson_AllExceptionTypes_ProducesDistinctTypeDiscriminators()
        {
            // Arrange
            var exceptions = new LocationTrackingException[]
            {
                new VehicleNotFoundException(1),
                new AssetNotFoundException(2),
                new TrackingSessionNotFoundException(3),
                new InvalidLocationException(10.5, 20.3),
                new LocationTrackingException("Base exception")
            };

            // Act & Assert
            var typeNames = new HashSet<string>();
            foreach (var exception in exceptions)
            {
                var json = exception.ToJson();
                var element = JsonSerializer.Deserialize<JsonElement>(json);

                Assert.True(element.TryGetProperty("$type", out var typeProperty));
                var typeName = typeProperty.GetString();
                Assert.NotNull(typeName);
                Assert.True(typeNames.Add(typeName), $"Duplicate type discriminator found: {typeName}");
            }
        }

        [Fact]
        public void TryFromJson_MalformedJsonWithValidStructure_ReturnsFalse()
        {
            // Arrange
            var malformedJson = "{ \"message\": \"test\", \"$type\": \"VehicleNotFoundException\" invalid";

            // Act
            var success = LocationTrackingExceptionJsonExtensions.TryFromJson(malformedJson, out _);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void FromJson_UnknownTypeDiscriminator_ReturnsNull()
        {
            // Arrange
            var json = "{\"$type\":\"UnknownExceptionType\",\"message\":\"test\",\"vehicleId\":123}";

            // Act
            var exception = LocationTrackingExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(exception);
        }
    }
}