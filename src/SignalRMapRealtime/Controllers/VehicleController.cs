#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.Domain.Enums;
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
    /// Gets a comma-separated list of valid vehicle status values for error messages.
    /// </summary>
    /// <returns>Comma-separated string of valid status values.</returns>
    private static string GetValidStatusValues()
    {
        var validValues = Enum.GetValues<VehicleStatus>()
            .Select(v => v.ToString())
            .ToArray();
        return string.Join(", ", validValues);
    }

    /// <summary>
    /// Gets all vehicles with pagination.
    /// Optionally filter by status (Active, Inactive, Maintenance).
    /// Supports both offset-based and cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// Offset-based pagination (default):
    /// - Use <c>pageNumber</c> and <c>pageSize</c> query parameters
    /// - Page numbers start at 1
    /// - Not stable when vehicles are added/updated between requests
    ///
    /// Cursor-based pagination (recommended for live feeds):
    /// - Use <c>cursor</c> query parameter instead of <c>pageNumber</c>
    /// - Returns <c>nextCursor</c> in response for subsequent requests
    /// - Stable pagination even when data changes between requests
    /// - More efficient for large datasets and real-time updates
    /// </remarks>
    /// <param name="pageNumber">Page number for offset-based pagination (1-based).</param>
    /// <param name="pageSize">Number of items per page (max 100).</param>
    /// <param name="status">Optional vehicle status filter (Active, Inactive, Maintenance).</param>
    /// <param name="cursor">Cursor token for cursor-based pagination. If provided, uses cursor-based pagination.</param>
    /// <returns>Paginated list of vehicles with metadata.</returns>
    [HttpGet]
    public async Task<IActionResult> GetVehicles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? cursor = null)
    {
        try
        {
            // Validate pagination parameters to prevent DoS via unbounded result sets
            const int maxPageSize = 100;
            const int maxPageNumber = 10000;

            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1");
            }

            if (pageNumber > maxPageNumber)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), $"Page number cannot exceed {maxPageNumber}");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1");
            }

            if (pageSize > maxPageSize)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size cannot exceed {maxPageSize}");
            }

        // Validate status parameter if provided
        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<VehicleStatus>(status, true, out _))
            {
                throw new ArgumentException(
                    $"Invalid status value '{status}'. Valid values are: {GetValidStatusValues()}",
                    nameof(status));
            }
        }

            // Check if cursor-based pagination is being used
            var isCursorPagination = !string.IsNullOrEmpty(cursor);

            if (isCursorPagination)
            {
                // Cursor-based pagination - more stable for live data feeds
                var result = await GetVehiclesWithCursorAsync(pageSize, cursor, status);

                var response = ApiResponse<PaginatedResponse<VehicleDto>>.SuccessResponse(
                    result,
                    "Vehicles retrieved successfully",
                    200,
                    HttpContext.TraceIdentifier);

                return Ok(response);
            }
            else
            {
                // Offset-based pagination (existing behavior)
                var (validPageNumber, validPageSize) = PaginationExtensions.NormalizePaginationParameters(pageNumber, pageSize, 100);

                var cacheKey = $"vehicles:page:{validPageNumber}:size:{validPageSize}:status:{status}";

                var result = await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () =>
                    {
                        var vehicles = await _vehicleService.GetAllVehiclesAsync();

        if (!string.IsNullOrEmpty(status))
        {
            var parsedStatus = Enum.Parse<VehicleStatus>(status, true);
            vehicles = vehicles.Where(v => v.Status == parsedStatus).ToList();
        }

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
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid pagination parameters. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ValidationError(
                new Dictionary<string, string[]> { { "pagination", new[] { ex.Message } } },
                "Invalid pagination parameters",
                HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve vehicles", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets vehicles using cursor-based pagination for stable pagination when data changes frequently.
    /// Uses keyset pagination on Id to ensure stable results even when new vehicles are added.
    /// </summary>
    /// <param name="pageSize">Number of vehicles per page.</param>
    /// <param name="cursor">Cursor token from previous response (null for first page).</param>
    /// <param name="status">Optional status filter.</param>
    /// <returns>Paginated response with cursor information.</returns>
    private async Task<PaginatedResponse<VehicleDto>> GetVehiclesWithCursorAsync(
        int pageSize,
        string? cursor,
        string? status = null)
    {
        // Validate pageSize to prevent DoS via unbounded result sets
        const int maxPageSize = 100;

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be at least 1");
        }

        if (pageSize > maxPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size cannot exceed {maxPageSize}");
        }

        // Validate status parameter if provided
        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<VehicleStatus>(status, true, out _))
            {
                throw new ArgumentException(
                    $"Invalid status value '{status}'. Valid values are: {GetValidStatusValues()}",
                    nameof(status));
            }
        }

        // Get all vehicles from service
        var vehicles = await _vehicleService.GetAllVehiclesAsync();

        // Apply status filter if provided
        // Status has already been validated above, so we can safely parse it
        if (!string.IsNullOrEmpty(status))
        {
            var parsedStatus = Enum.Parse<VehicleStatus>(status, true);
            vehicles = vehicles.Where(v => v.Status == parsedStatus).ToList();
        }

        // Sort by Id for stable cursor-based pagination
        // Using Id ensures stable ordering even when UpdatedAt changes frequently
        var sortedVehicles = vehicles
            .OrderBy(v => v.Id)
            .ThenBy(v => v.UpdatedAt)
            .ToList();

        // Convert to DTOs and apply cursor-based pagination
        // Note: We convert to list first to ensure stable ordering before pagination
        var vehicleDtos = sortedVehicles
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                Name = v.Name,
                RegistrationNumber = v.RegistrationNumber,
                Status = v.Status,
                AssetType = v.AssetType,
                DriverId = v.DriverId,
                Manufacturer = v.Manufacturer,
                ModelYear = v.ModelYear,
                MaxSpeed = v.MaxSpeed,
                FuelLevel = v.FuelLevel,
                IsOnline = v.IsOnline,
                LastLocation = v.LastLocation != null ? new LocationDto
                {
                    Latitude = v.LastLocation.Latitude,
                    Longitude = v.LastLocation.Longitude,
                    Timestamp = v.LastLocation.Timestamp,
                    Speed = v.LastLocation.Speed,
                    Heading = v.LastLocation.Heading
                } : null,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                LicensePlate = v.LicensePlate,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            })
            .ToList();

        // Use cursor-based pagination with stable ordering
        return PaginatedResponse<VehicleDto>.FromCursorList(vehicleDtos, pageSize, cursor);
    }

    /// <summary>
    /// Gets a specific vehicle by ID.
    /// Returns vehicle with complete information including recent location data.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(int id)
    {
        try
        {
            var cacheKey = $"vehicle:{id}";
            var vehicle = await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await _vehicleService.GetVehicleAsync(id),
                TimeSpan.FromSeconds(600));

            if (vehicle is null)
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
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto createVehicleDto)
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

            if (string.IsNullOrWhiteSpace(createVehicleDto.RegistrationNumber))
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
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto updateVehicleDto)
    {
        try
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, updateVehicleDto);

            if (vehicle is null)
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
    public async Task<IActionResult> DeleteVehicle(int id)
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
    public async Task<IActionResult> GetVehicleStatus(int id)
    {
        try
        {
            var vehicle = await _vehicleService.GetVehicleAsync(id);

            if (vehicle is null)
                return NotFound(ErrorResponse.NotFoundError($"Vehicle with ID {id} not found", HttpContext.TraceIdentifier));

            var status = new
            {
                id = vehicle.Id,
                plate = vehicle.RegistrationNumber,
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
