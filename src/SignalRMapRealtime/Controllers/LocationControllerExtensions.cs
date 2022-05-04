#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

/// <summary>
/// Extension methods for LocationController providing additional functionality
/// such as location filtering and retrieval capabilities.
/// </summary>
public static class LocationControllerExtensions
{
    /// <summary>
    /// Gets locations filtered by vehicle ID and date range.
    /// Returns 200 OK with filtered locations.
    /// </summary>
    /// <param name="controller">The LocationController instance</param>
    /// <param name="vehicleId">Vehicle ID to filter by</param>
    /// <param name="startDate">Optional start date for filtering</param>
    /// <param name="endDate">Optional end date for filtering</param>
    [HttpGet("vehicle/{vehicleId}")]
    public static async Task<IActionResult> GetLocationsByVehicleAndDateRange(
        this LocationController controller,
        [FromRoute] int vehicleId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var result = await controller.GetLocations(1, 100, vehicleId);

            if (result is OkObjectResult okResult && okResult.Value is ApiResponse<PaginatedResponse<LocationDto>> apiResponse && apiResponse.Data?.Items != null)
            {
                var filteredLocations = apiResponse.Data.Items.AsEnumerable();

                if (startDate.HasValue || endDate.HasValue)
                {
                    filteredLocations = filteredLocations.Where(l =>
                    {
                        var locationDate = l.Timestamp.Date;
                        return (!startDate.HasValue || locationDate >= startDate.Value.Date) &&
                               (!endDate.HasValue || locationDate <= endDate.Value.Date);
                    });
                }

                var filteredResponse = PaginatedResponse<LocationDto>.FromList(
                    filteredLocations.ToList(),
                    1,
                    filteredLocations.Count());

                var response = ApiResponse<PaginatedResponse<LocationDto>>.SuccessResponse(
                    filteredResponse,
                    "Filtered locations retrieved successfully",
                    200,
                    controller.HttpContext.TraceIdentifier);

                return controller.Ok(response);
            }

            return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve locations", controller.HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve vehicle locations", controller.HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets the most recent location for a specific vehicle.
    /// Returns 200 OK with the latest location or 404 if not found.
    /// </summary>
    /// <param name="controller">The LocationController instance</param>
    /// <param name="vehicleId">Vehicle ID to get latest location for</param>
    [HttpGet("vehicle/{vehicleId}/latest")]
    public static async Task<IActionResult> GetLatestLocationByVehicle(
        this LocationController controller,
        [FromRoute] int vehicleId)
    {
        try
        {
            // Get all locations for the vehicle, sorted by timestamp descending
            var result = await controller.GetLocations(1, 1, vehicleId);

            if (result is OkObjectResult okResult && okResult.Value is ApiResponse<PaginatedResponse<LocationDto>> apiResponse && apiResponse.Data?.Items != null && apiResponse.Data.Items.Any())
            {
                var latestLocation = apiResponse.Data.Items.OrderByDescending(l => l.Timestamp).First();

                var response = ApiResponse<LocationDto>.SuccessResponse(
                    latestLocation,
                    "Latest location retrieved successfully",
                    200,
                    controller.HttpContext.TraceIdentifier);

                return controller.Ok(response);
            }

            return controller.NotFound(ErrorResponse.NotFoundError(
                $"No locations found for vehicle ID {vehicleId}",
                controller.HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve latest location", controller.HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets locations by multiple IDs in a single request.
    /// Returns 200 OK with all found locations.
    /// </summary>
    /// <param name="controller">The LocationController instance</param>
    /// <param name="ids">Array of location IDs to retrieve</param>
    [HttpGet("batch")]
    public static async Task<IActionResult> GetLocationsByIds(
        this LocationController controller,
        [FromQuery] Guid[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return controller.BadRequest(ErrorResponse.ValidationError(
                new Dictionary<string, string[]> {
                    { "ids", new[] { "At least one location ID must be provided" } }
                },
                "No IDs provided",
                controller.HttpContext.TraceIdentifier));
        }

        try
        {
            var results = new List<LocationDto>();
            var notFoundIds = new List<Guid>();

            foreach (var id in ids)
            {
                var result = await controller.GetLocationById(id);
                if (result is OkObjectResult okResult && okResult.Value is ApiResponse<LocationDto> apiResponse && apiResponse.Data != null)
                {
                    results.Add(apiResponse.Data);
                }
                else
                {
                    notFoundIds.Add(id);
                }
            }

            var response = ApiResponse<List<LocationDto>>.SuccessResponse(
                results,
                notFoundIds.Count == 0
                    ? "Locations retrieved successfully"
                    : $"Retrieved {results.Count} locations, {notFoundIds.Count} not found",
                200,
                controller.HttpContext.TraceIdentifier);

            return controller.Ok(response);
        }
        catch (Exception ex)
        {
            return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve batch locations", controller.HttpContext.TraceIdentifier));
        }
    }
}