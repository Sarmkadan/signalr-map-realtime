// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.Services;
using SignalRMapRealtime.Utilities;

/// <summary>
/// API controller for managing location data.
/// Provides endpoints for creating, updating, retrieving, and deleting location records.
/// Demonstrates proper REST conventions and API response formatting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(
        ILocationService locationService,
        ICacheService cacheService,
        ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all locations with pagination support.
    /// Supports filtering by vehicle ID and date range.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="vehicleId">Optional vehicle ID to filter locations</param>
    [HttpGet]
    public async Task<IActionResult> GetLocations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? vehicleId = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = PaginationExtensions.NormalizePaginationParameters(pageNumber, pageSize, 100);

            var cacheKey = $"locations:page:{validPageNumber}:size:{validPageSize}:vehicle:{vehicleId}";

            var result = await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var locations = await _locationService.GetLocationsAsync();

                    if (vehicleId.HasValue)
                        locations = locations.Where(l => l.VehicleId == vehicleId.Value).ToList();

                    return PaginatedResponse<LocationDto>.FromList(locations, validPageNumber, validPageSize);
                },
                TimeSpan.FromSeconds(30));

            var response = ApiResponse<PaginatedResponse<LocationDto>>.SuccessResponse(
                result,
                "Locations retrieved successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to retrieve locations", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets a specific location by ID.
    /// Returns 404 if location not found.
    /// </summary>
    /// <param name="id">Location ID</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLocationById(Guid id)
    {
        try
        {
            var cacheKey = $"location:{id}";
            var location = await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await _locationService.GetLocationByIdAsync(id),
                TimeSpan.FromSeconds(30));

            if (location == null)
                return NotFound(ErrorResponse.NotFoundError($"Location with ID {id} not found", HttpContext.TraceIdentifier));

            var response = ApiResponse<LocationDto>.SuccessResponse(location, "Location retrieved successfully", 200, HttpContext.TraceIdentifier);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location {LocationId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to retrieve location", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Creates a new location record.
    /// Returns 201 Created with the new location.
    /// </summary>
    /// <param name="createLocationDto">Location data</param>
    [HttpPost]
    public async Task<IActionResult> CreateLocation([FromBody] LocationDto createLocationDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorResponse.ValidationError(
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .GroupBy(e => "ValidationError")
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    "Validation failed",
                    HttpContext.TraceIdentifier));

            // Validate coordinates
            if (!createLocationDto.Latitude.IsValidLatitude() || !createLocationDto.Longitude.IsValidLongitude())
                return BadRequest(ErrorResponse.ValidationError(
                    new Dictionary<string, string[]>
                    {
                        { "coordinates", new[] { "Invalid latitude or longitude" } }
                    },
                    "Invalid coordinates",
                    HttpContext.TraceIdentifier));

            var location = await _locationService.CreateLocationAsync(createLocationDto);

            // Invalidate cache
            await _cacheService.RemoveByPatternAsync("locations:*");

            var response = ApiResponse<LocationDto>.SuccessResponse(
                location,
                "Location created successfully",
                201,
                HttpContext.TraceIdentifier);

            return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to create location", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Updates an existing location record.
    /// Returns 200 OK with updated location.
    /// </summary>
    /// <param name="id">Location ID</param>
    /// <param name="updateLocationDto">Updated location data</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] LocationDto updateLocationDto)
    {
        try
        {
            var location = await _locationService.UpdateLocationAsync(id, updateLocationDto);

            if (location == null)
                return NotFound(ErrorResponse.NotFoundError($"Location with ID {id} not found", HttpContext.TraceIdentifier));

            // Invalidate cache
            await _cacheService.RemoveAsync($"location:{id}");
            await _cacheService.RemoveByPatternAsync("locations:*");

            var response = ApiResponse<LocationDto>.SuccessResponse(location, "Location updated successfully", 200, HttpContext.TraceIdentifier);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location {LocationId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to update location", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Deletes a location record.
    /// Returns 204 No Content on success.
    /// </summary>
    /// <param name="id">Location ID</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(Guid id)
    {
        try
        {
            var success = await _locationService.DeleteLocationAsync(id);

            if (!success)
                return NotFound(ErrorResponse.NotFoundError($"Location with ID {id} not found", HttpContext.TraceIdentifier));

            // Invalidate cache
            await _cacheService.RemoveAsync($"location:{id}");
            await _cacheService.RemoveByPatternAsync("locations:*");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location {LocationId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to delete location", HttpContext.TraceIdentifier));
        }
    }
}
