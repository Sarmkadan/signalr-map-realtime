// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.Utilities;

/// <summary>
/// API controller for managing routes and delivery paths.
/// Routes define sequences of waypoints for vehicles to follow.
/// Provides endpoints for route creation, optimization, and tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RouteController : ControllerBase
{
    private readonly IRepository<Domain.Models.Route> _routeRepository;
    private readonly ILogger<RouteController> _logger;

    public RouteController(
        IRepository<Domain.Models.Route> routeRepository,
        ILogger<RouteController> logger)
    {
        _routeRepository = routeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all routes with pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRoutes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var (validPageNumber, validPageSize) = PaginationExtensions.NormalizePaginationParameters(pageNumber, pageSize, 100);

            var allRoutes = await _routeRepository.GetAllAsync();
            var result = PaginatedResponse<RouteDto>.FromList(
                allRoutes.Select(MapToDto).ToList(),
                validPageNumber,
                validPageSize);

            var response = ApiResponse<PaginatedResponse<RouteDto>>.SuccessResponse(
                result,
                "Routes retrieved successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve routes", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets a specific route by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteById(Guid id)
    {
        try
        {
            var route = await _routeRepository.GetByIdAsync(id);

            if (route == null)
                return NotFound(ErrorResponse.NotFoundError($"Route with ID {id} not found", HttpContext.TraceIdentifier));

            var response = ApiResponse<RouteDto>.SuccessResponse(
                MapToDto(route),
                "Route retrieved successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route {RouteId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve route", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Creates a new route with waypoints.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] RouteDto createRouteDto)
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

            if (string.IsNullOrWhiteSpace(createRouteDto.Name))
                return BadRequest(ErrorResponse.ValidationError(
                    new Dictionary<string, string[]> { { "name", new[] { "Route name is required" } } },
                    "Validation failed",
                    HttpContext.TraceIdentifier));

            var route = new Domain.Models.Route
            {
                Id = Guid.NewGuid(),
                Name = createRouteDto.Name,
                Description = createRouteDto.Description,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _routeRepository.AddAsync(route);
            await _routeRepository.SaveChangesAsync();

            var response = ApiResponse<RouteDto>.SuccessResponse(
                MapToDto(route),
                "Route created successfully",
                201,
                HttpContext.TraceIdentifier);

            return CreatedAtAction(nameof(GetRouteById), new { id = route.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating route. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to create route", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Updates a route.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(Guid id, [FromBody] RouteDto updateRouteDto)
    {
        try
        {
            var route = await _routeRepository.GetByIdAsync(id);

            if (route == null)
                return NotFound(ErrorResponse.NotFoundError($"Route with ID {id} not found", HttpContext.TraceIdentifier));

            route.Name = updateRouteDto.Name;
            route.Description = updateRouteDto.Description;
            route.UpdatedAt = DateTime.UtcNow;

            _routeRepository.Update(route);
            await _routeRepository.SaveChangesAsync();

            var response = ApiResponse<RouteDto>.SuccessResponse(
                MapToDto(route),
                "Route updated successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route {RouteId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to update route", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Deletes a route.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(Guid id)
    {
        try
        {
            var route = await _routeRepository.GetByIdAsync(id);

            if (route == null)
                return NotFound(ErrorResponse.NotFoundError($"Route with ID {id} not found", HttpContext.TraceIdentifier));

            _routeRepository.Delete(route);
            await _routeRepository.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting route {RouteId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to delete route", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Calculates the total distance and estimated time for a route.
    /// </summary>
    [HttpPost("{id}/calculate")]
    public async Task<IActionResult> CalculateRoute(Guid id)
    {
        try
        {
            var route = await _routeRepository.GetByIdAsync(id);

            if (route == null)
                return NotFound(ErrorResponse.NotFoundError($"Route with ID {id} not found", HttpContext.TraceIdentifier));

            // Placeholder calculation - in production would use actual waypoint coordinates
            var calculation = new
            {
                routeId = route.Id,
                estimatedDistance = 25.5,
                estimatedDurationMinutes = 45,
                waypointCount = 5,
                calculatedAt = DateTime.UtcNow
            };

            var response = ApiResponse<object>.SuccessResponse(
                calculation,
                "Route calculation completed",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating route {RouteId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to calculate route", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Maps domain model to DTO.
    /// </summary>
    private RouteDto MapToDto(Domain.Models.Route route)
    {
        return new RouteDto
        {
            Id = route.Id,
            Name = route.Name,
            Description = route.Description,
            Status = route.Status,
            CreatedAt = route.CreatedAt,
            UpdatedAt = route.UpdatedAt
        };
    }
}
