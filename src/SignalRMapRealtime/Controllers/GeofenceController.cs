#nullable enable
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
/// Provides REST endpoints for managing geofence zones and evaluating vehicle positions
/// against configured boundaries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GeofenceController : ControllerBase
{
    private readonly IGeofenceService _geofenceService;
    private readonly ILogger<GeofenceController> _logger;

    /// <summary>Initializes a new instance of <see cref="GeofenceController"/>.</summary>
    public GeofenceController(IGeofenceService geofenceService, ILogger<GeofenceController> logger)
    {
        ArgumentNullException.ThrowIfNull(geofenceService);
        ArgumentNullException.ThrowIfNull(logger);
        _geofenceService = geofenceService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all currently active geofence zones configured in the system.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GeofenceDto>>), 200)]
    public async Task<IActionResult> GetActiveZones()
    {
        try
        {
            var zones = await _geofenceService.GetActiveZonesAsync().ConfigureAwait(false);

            return Ok(ApiResponse<IReadOnlyList<GeofenceDto>>.SuccessResponse(
                zones, "Active geofence zones retrieved", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving geofence zones. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to retrieve geofence zones", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Registers a new geofence zone. The zone becomes active immediately and will be evaluated
    /// on subsequent <see cref="CheckLocation"/> calls.
    /// </summary>
    /// <param name="dto">Zone definition including shape (circle or polygon), coordinates, and metadata.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GeofenceDto>), 201)]
    public async Task<IActionResult> RegisterZone([FromBody] CreateGeofenceDto dto)
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

            var zone = await _geofenceService.RegisterZoneAsync(dto).ConfigureAwait(false);

            var response = ApiResponse<GeofenceDto>.SuccessResponse(
                zone, "Geofence zone registered successfully", 201, HttpContext.TraceIdentifier);

            return CreatedAtAction(nameof(GetActiveZones), null, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering geofence zone. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to register geofence zone", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Removes a geofence zone by its identifier. The zone is immediately deregistered
    /// and will no longer be evaluated during location checks.
    /// </summary>
    /// <param name="id">Unique identifier of the zone to remove.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveZone(Guid id)
    {
        try
        {
            var removed = await _geofenceService.RemoveZoneAsync(id).ConfigureAwait(false);

            if (!removed)
                return NotFound(ErrorResponse.NotFoundError(
                    $"Geofence zone {id} not found", HttpContext.TraceIdentifier));

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing geofence zone {ZoneId}. TraceId: {TraceId}",
                id, HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to remove geofence zone", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Checks a vehicle's current position against all active geofence zones and returns
    /// any boundary-crossing alerts for transitions that occurred since the last check.
    /// Entry and exit alerts are emitted only on state change — repeated positions inside
    /// a zone do not produce duplicate alerts.
    /// </summary>
    /// <param name="vehicleId">Identifier of the vehicle being evaluated.</param>
    /// <param name="latitude">Current latitude in decimal degrees.</param>
    /// <param name="longitude">Current longitude in decimal degrees.</param>
    [HttpPost("check")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GeofenceAlertDto>>), 200)]
    public async Task<IActionResult> CheckLocation(
        [FromQuery] Guid vehicleId,
        [FromQuery] double latitude,
        [FromQuery] double longitude)
    {
        try
        {
            if (!latitude.IsValidLatitude() || !longitude.IsValidLongitude())
                return BadRequest(ErrorResponse.ValidationError(
                    new Dictionary<string, string[]>
                    {
                        { "coordinates", new[] { "Latitude must be in [-90, 90] and longitude in [-180, 180]." } }
                    },
                    "Invalid coordinates",
                    HttpContext.TraceIdentifier));

            var alerts = await _geofenceService
                .CheckLocationAsync(vehicleId, latitude, longitude)
                .ConfigureAwait(false);

            return Ok(ApiResponse<IReadOnlyList<GeofenceAlertDto>>.SuccessResponse(
                alerts, "Location checked successfully", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking location against geofences. TraceId: {TraceId}",
                HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to check location", HttpContext.TraceIdentifier));
        }
    }
}
