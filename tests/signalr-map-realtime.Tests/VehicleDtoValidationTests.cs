using System;
using System.Collections.Generic;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Domain.Enums;
using Xunit;

namespace SignalRMapRealtime.Tests
{
    public class VehicleDtoValidationTests
    {
        private static VehicleDto CreateValidVehicle()
        {
            // Assuming the default enum values are defined (e.g., Unknown = 0)
            return new VehicleDto
            {
                Id = 1,
                Name = "Test Vehicle",
                RegistrationNumber = "REG-123",
                Status = default,          // VehicleStatus enum default value
                AssetType = default,       // AssetType enum default value
                ModelYear = 2022,
                MaxSpeed = 120.0,
                FuelLevel = 55.0,
                DriverId = 2,
                Year = 2022,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLocation = new LocationDto
                {
                    Latitude = 45.0,
                    Longitude = 90.0,
                    LocationType = default, // LocationType enum default value
                    RecordedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var vehicle = CreateValidVehicle();

            // Act
            var errors = vehicle.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var vehicle = CreateValidVehicle();

            // Act
            var result = vehicle.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var vehicle = CreateValidVehicle();

            // Act / Assert
            var exception = Record.Exception(() => vehicle.EnsureValid());
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VehicleDtoValidation.Validate(null!));
        }

        [Fact]
        public void IsValid_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VehicleDtoValidation.IsValid(null!));
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VehicleDtoValidation.EnsureValid(null!));
        }

        [Fact]
        public void EnsureValid_InvalidVehicle_ThrowsArgumentException_WithExpectedMessage()
        {
            // Arrange: create a vehicle that fails a few validations
            var vehicle = CreateValidVehicle();
            vehicle.Name = "";                     // triggers "Vehicle name is required..."
            vehicle.Id = 0;                        // triggers "Vehicle ID must be a positive integer."
            vehicle.FuelLevel = -5;                // triggers fuel level range error

            // Act
            var ex = Assert.Throws<ArgumentException>(() => vehicle.EnsureValid());

            // Assert
            Assert.StartsWith("VehicleDto validation failed:", ex.Message);
            Assert.Contains("Vehicle name is required and cannot be empty.", ex.Message);
            Assert.Contains("Vehicle ID must be a positive integer.", ex.Message);
            Assert.Contains("Fuel level must be between 0 and 100.", ex.Message);
        }

        [Fact]
        public void Validate_BoundaryValues_ReturnsExpectedErrors()
        {
            // Arrange: set values right on the edge of allowed ranges
            var vehicle = CreateValidVehicle();
            vehicle.ModelYear = 1899; // below MinReasonableYear (1900)
            vehicle.FuelLevel = 101;  // above MaxFuelLevel (100)
            vehicle.MaxSpeed = -1;    // negative speed

            // Act
            var errors = vehicle.Validate();

            // Assert
            Assert.Contains("Model year must be between 1900 and 2100.", errors);
            Assert.Contains("Fuel level must be between 0 and 100.", errors);
            Assert.Contains("Maximum speed cannot be negative.", errors);
        }

        [Fact]
        public void Validate_LastLocationInvalid_PropagatesLocationErrors()
        {
            // Arrange: create a vehicle with an invalid location
            var vehicle = CreateValidVehicle();
            vehicle.LastLocation = new LocationDto
            {
                Latitude = 200,   // invalid latitude
                Longitude = -200, // invalid longitude
                LocationType = default,
                RecordedAt = DateTime.MinValue, // invalid timestamp
                CreatedAt = DateTime.MinValue   // invalid timestamp
            };

            // Act
            var errors = vehicle.Validate();

            // Assert
            Assert.Contains("Latitude must be between -90.0 and 90.0 degrees.", errors);
            Assert.Contains("Longitude must be between -180.0 and 180.0 degrees.", errors);
            Assert.Contains("RecordedAt timestamp cannot be the default value.", errors);
            Assert.Contains("CreatedAt timestamp cannot be the default value.", errors);
        }
    }
}
