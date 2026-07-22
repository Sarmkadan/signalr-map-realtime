#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for LocationController
// =============================================================================

namespace SignalRMapRealtime.Tests.Controllers;

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using SignalRMapRealtime.Controllers;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.Services;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Unit tests for LocationController focusing on UpdateLocation endpoint behavior.
/// </summary>
public class LocationControllerTests
{
    private readonly ILocationService _locationService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<LocationController> _logger;
    private readonly LocationController _controller;

    /// <summary>
    /// Initializes a new instance of the LocationControllerTests class.
    /// </summary>
    public LocationControllerTests()
    {
        _locationService = Substitute.For<ILocationService>();
        _cacheService = Substitute.For<ICacheService>();
        _logger = Substitute.For<ILogger<LocationController>>();
        _controller = new LocationController(_locationService, _cacheService, _logger);
    }

    /// <summary>
    /// Tests that UpdateLocation returns success ApiResponse for valid update.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_ValidUpdate_ReturnsSuccessApiResponse()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5,
            Bearing = 180.0,
            Address = "123 Main St",
            Notes = "Updated location",
            LocationType = LocationType.TrackPoint
        };

        var existingLocation = new LocationDto
        {
            Id = 1,
            Latitude = 40.7128,
            Longitude = -74.0060,
            Speed = 50.0,
            Bearing = 90.0,
            Address = "Old Address",
            Notes = "Old notes",
            LocationType = LocationType.DeliveryPoint,
            VehicleId = 1,
            RecordedAt = DateTime.UtcNow
        };

        _locationService.UpdateLocationAsync(locationId, updateDto)
            .Returns(existingLocation);

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeAssignableTo<ApiResponse<LocationDto>>().Which;

        response.Should().NotBeNull();
        response.Message.Should().Be("Location updated successfully");
        response.StatusCode.Should().Be(200);
        response.Data.Should().BeEquivalentTo(existingLocation);
    }

    /// <summary>
    /// Tests that UpdateLocation returns 400 BadRequest when latitude is > 90.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_InvalidLatitudeGreaterThan90_ReturnsBadRequest()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5,
            Bearing = 180.0,
            Address = "123 Main St",
            Notes = "Updated location"
        };

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        var errorResponse = badRequestResult.Value.Should().BeAssignableTo<ErrorResponse>().Which;

        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(400);
        errorResponse.Message.Should().Contain("Validation failed");
    }

    /// <summary>
    /// Tests that UpdateLocation returns 400 BadRequest when latitude is < -90.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_InvalidLatitudeLessThanMinus90_ReturnsBadRequest()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5,
            Bearing = 180.0,
            Address = "123 Main St",
            Notes = "Updated location"
        };

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        var errorResponse = badRequestResult.Value.Should().BeAssignableTo<ErrorResponse>().Which;

        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(400);
        errorResponse.Message.Should().Contain("Validation failed");
    }

    /// <summary>
    /// Tests that UpdateLocation returns 400 BadRequest when longitude is > 180.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_InvalidLongitudeGreaterThan180_ReturnsBadRequest()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5,
            Bearing = 180.0,
            Address = "123 Main St",
            Notes = "Updated location"
        };

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        var errorResponse = badRequestResult.Value.Should().BeAssignableTo<ErrorResponse>().Which;

        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(400);
        errorResponse.Message.Should().Contain("Validation failed");
    }

    /// <summary>
    /// Tests that UpdateLocation returns 400 BadRequest when longitude is < -180.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_InvalidLongitudeLessThanMinus180_ReturnsBadRequest()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5,
            Bearing = 180.0,
            Address = "123 Main St",
            Notes = "Updated location"
        };

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        var errorResponse = badRequestResult.Value.Should().BeAssignableTo<ErrorResponse>().Which;

        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(400);
        errorResponse.Message.Should().Contain("Validation failed");
    }

    /// <summary>
    /// Tests that UpdateLocation returns 404 NotFound when location does not exist.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_MissingLocation_ReturnsNotFound()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5,
            Bearing = 180.0
        };

        _locationService.UpdateLocationAsync(locationId, updateDto)
            .Returns((LocationDto?)null);

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Which;
        var errorResponse = notFoundResult.Value.Should().BeAssignableTo<ErrorResponse>().Which;

        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(404);
        errorResponse.Message.Should().Contain("not found");
    }

    /// <summary>
    /// Tests that UpdateLocation returns success when only optional fields are provided.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_OnlyOptionalFields_ReturnsSuccess()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Notes = "Updated notes only"
        };

        var existingLocation = new LocationDto
        {
            Id = 1,
            Latitude = 40.7128,
            Longitude = -74.0060,
            VehicleId = 1,
            RecordedAt = DateTime.UtcNow
        };

        _locationService.UpdateLocationAsync(locationId, updateDto)
            .Returns(existingLocation);

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeAssignableTo<ApiResponse<LocationDto>>().Which;

        response.Should().NotBeNull();
        response.Message.Should().Be("Location updated successfully");
        response.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Tests that UpdateLocation handles exceptions and returns 400 BadRequest.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_ExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5
        };

        _locationService.UpdateLocationAsync(locationId, updateDto)
            .ThrowsAsync<InvalidOperationException>();

        // Act
        var result = await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Which;
        var errorResponse = badRequestResult.Value.Should().BeAssignableTo<ErrorResponse>().Which;

        errorResponse.Should().NotBeNull();
        errorResponse.StatusCode.Should().Be(400);
        errorResponse.Message.Should().Contain("Failed to update location");
    }

    /// <summary>
    /// Tests that UpdateLocation properly invalidates cache on success.
    /// </summary>
    [Fact]
    public async Task UpdateLocation_InvalidatesCacheOnSuccess()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var updateDto = new UpdateLocationDto
        {
            Speed = 65.5,
            Bearing = 180.0
        };

        var existingLocation = new LocationDto
        {
            Id = 1,
            Latitude = 40.7128,
            Longitude = -74.0060,
            VehicleId = 1,
            RecordedAt = DateTime.UtcNow
        };

        _locationService.UpdateLocationAsync(locationId, updateDto)
            .Returns(existingLocation);

        // Act
        await _controller.UpdateLocation(locationId, updateDto);

        // Assert
        await _cacheService.Received(1).RemoveAsync(Arg.Is<string>($"location:{locationId}"));
        await _cacheService.Received(1).RemoveByPatternAsync("locations:*");
    }
}
