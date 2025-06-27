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
/// API controller for managing vehicle data.
/// Provides endpoints for CRUD operations on vehicles with status tracking.
/// Vehicles represent entities being tracked on the map (cars, trucks, couriers, etc.).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<VehicleController> _logger;

    public VehicleController(
        IVehicleService vehicleService,
        ICacheService cacheService,
        ILogger<VehicleController> logger)
    {
        _vehicleService = vehicleService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all vehicles with pagination.
    /// Optionally filter by status (Active, Inactive, Maintenance).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetVehicles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = PaginationExtensions.NormalizePaginationParameters(pageNumber, pageSize, 100);

            var cacheKey = $"vehicles:page:{validPageNumber}:size:{validPageSize}:status:{status}";

            var result = await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var vehicles = await _vehicleService.GetAllVehiclesAsync();

                    if (!string.IsNullOrEmpty(status))
                        vehicles = vehicles.Where(v => v.Status == status).ToList();

                    return PaginatedResponse<VehicleDto>.FromList(vehicles, validPageNumber, validPageSize);
                },
                TimeSpan.FromSeconds(300));

            var response = ApiResponse<PaginatedResponse<VehicleDto>>.SuccessResponse(
                result,
                "Vehicles retrieved successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve vehicles", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets a specific vehicle by ID.
    /// Returns vehicle with complete information including recent location data.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(Guid id)
    {
        try
        {
            var cacheKey = $"vehicle:{id}";
            var vehicle = await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await _vehicleService.GetVehicleByIdAsync(id),
                TimeSpan.FromSeconds(600));

            if (vehicle == null)
                return NotFound(ErrorResponse.NotFoundError($"Vehicle with ID {id} not found", HttpContext.TraceIdentifier));

            var response = ApiResponse<VehicleDto>.SuccessResponse(vehicle, "Vehicle retrieved successfully", 200, HttpContext.TraceIdentifier);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle {VehicleId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve vehicle", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Creates a new vehicle.
    /// Returns 201 Created with the new vehicle data.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] VehicleDto createVehicleDto)
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

            if (string.IsNullOrWhiteSpace(createVehicleDto.Plate))
                return BadRequest(ErrorResponse.ValidationError(
                    new Dictionary<string, string[]> { { "plate", new[] { "Vehicle plate is required" } } },
                    "Validation failed",
                    HttpContext.TraceIdentifier));

            var vehicle = await _vehicleService.CreateVehicleAsync(createVehicleDto);

            // Invalidate cache
            await _cacheService.RemoveByPatternAsync("vehicles:*");

            var response = ApiResponse<VehicleDto>.SuccessResponse(
                vehicle,
                "Vehicle created successfully",
                201,
                HttpContext.TraceIdentifier);

            return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to create vehicle", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Updates vehicle information.
    /// Allows updating vehicle properties like name, status, assignment.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] VehicleDto updateVehicleDto)
    {
        try
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, updateVehicleDto);

            if (vehicle == null)
                return NotFound(ErrorResponse.NotFoundError($"Vehicle with ID {id} not found", HttpContext.TraceIdentifier));

            // Invalidate cache
            await _cacheService.RemoveAsync($"vehicle:{id}");
            await _cacheService.RemoveByPatternAsync("vehicles:*");

            var response = ApiResponse<VehicleDto>.SuccessResponse(vehicle, "Vehicle updated successfully", 200, HttpContext.TraceIdentifier);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to update vehicle", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Deletes a vehicle.
    /// Returns 204 No Content on success.
    /// Vehicle should not have any active tracking sessions.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        try
        {
            var success = await _vehicleService.DeleteVehicleAsync(id);

            if (!success)
                return NotFound(ErrorResponse.NotFoundError($"Vehicle with ID {id} not found", HttpContext.TraceIdentifier));

            // Invalidate cache
            await _cacheService.RemoveAsync($"vehicle:{id}");
            await _cacheService.RemoveByPatternAsync("vehicles:*");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {VehicleId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to delete vehicle", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets the current status of a vehicle.
    /// Lightweight endpoint that returns only essential status information.
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetVehicleStatus(Guid id)
    {
        try
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null)
                return NotFound(ErrorResponse.NotFoundError($"Vehicle with ID {id} not found", HttpContext.TraceIdentifier));

            var status = new
            {
                id = vehicle.Id,
                plate = vehicle.Plate,
                status = vehicle.Status,
                lastUpdated = vehicle.UpdatedAt
            };

            var response = ApiResponse<object>.SuccessResponse(status, "Vehicle status retrieved successfully", 200, HttpContext.TraceIdentifier);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle status for {VehicleId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve vehicle status", HttpContext.TraceIdentifier));
        }
    }
}
