#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.Utilities;

/// <summary>
/// Extension methods for <see cref="LocationController"/> providing additional location filtering and retrieval capabilities.
/// </summary>
public static class LocationControllerExtensions
{
    /// <summary>
    /// Gets locations filtered by vehicle ID and date range.
    /// Returns 200 OK with filtered locations.
    /// </summary>
    /// <param name="controller">The <see cref="LocationController"/> instance.</param>
    /// <param name="vehicleId">Vehicle ID to filter by.</param>
    /// <param name="startDate">Optional start date for filtering (inclusive).</param>
    /// <param name="endDate">Optional end date for filtering (inclusive).</param>
    /// <returns>Action result with filtered locations.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    [HttpGet("vehicle/{vehicleId}")]
    public static async Task<IActionResult> GetLocationsByVehicleAndDateRange(
        this LocationController controller,
        [FromRoute] int vehicleId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            return controller.BadRequest(ErrorResponse.ValidationError(
                new Dictionary<string, string[]> {
                    { "dateRange", new[] { "Start date must be before or equal to end date" } }
                },
                "Invalid date range",
                controller.HttpContext.TraceIdentifier));
        }

        try
        {
            // Get all locations for the vehicle using the controller's existing method
            var result = await controller.GetLocations(1, 1000, vehicleId);

            if (result is not OkObjectResult okResult)
            {
                return result;
            }

            var apiResponse = okResult.Value as ApiResponse<PaginatedResponse<LocationDto>>;

            if (apiResponse?.Data?.Items == null)
            {
                return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve locations", controller.HttpContext.TraceIdentifier));
            }

            var filteredLocations = apiResponse.Data.Items.AsEnumerable();

            // Apply date filtering if specified
            if (startDate.HasValue || endDate.HasValue)
            {
                filteredLocations = filteredLocations.Where(l =>
                {
                    var locationDate = l.Timestamp.Date;
                    return (!startDate.HasValue || locationDate >= startDate.Value.Date) &&
                           (!endDate.HasValue || locationDate <= endDate.Value.Date);
                });
            }

            var filteredList = filteredLocations.ToList();
            var filteredResponse = PaginatedResponse<LocationDto>.FromList(
                filteredList,
                1,
                filteredList.Count);

            var response = ApiResponse<PaginatedResponse<LocationDto>>.SuccessResponse(
                filteredResponse,
                "Filtered locations retrieved successfully",
                200,
                controller.HttpContext.TraceIdentifier);

            return controller.Ok(response);
        }
        catch
        {
            return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve vehicle locations", controller.HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets the most recent location for a specific vehicle.
    /// Returns 200 OK with the latest location or 404 if not found.
    /// </summary>
    /// <param name="controller">The <see cref="LocationController"/> instance.</param>
    /// <param name="vehicleId">Vehicle ID to get latest location for.</param>
    /// <returns>Action result with the latest location or 404 if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    [HttpGet("vehicle/{vehicleId}/latest")]
    public static async Task<IActionResult> GetLatestLocationByVehicle(
        this LocationController controller,
        [FromRoute] int vehicleId)
    {
        ArgumentNullException.ThrowIfNull(controller);

        try
        {
            // Get locations for the vehicle (limit to 1 page with 1 item to get the most recent)
            var result = await controller.GetLocations(1, 1, vehicleId);

            if (result is not OkObjectResult okResult)
            {
                return result;
            }

            var apiResponse = okResult.Value as ApiResponse<PaginatedResponse<LocationDto>>;

            if (apiResponse?.Data?.Items == null || !apiResponse.Data.Items.Any())
            {
                return controller.NotFound(ErrorResponse.NotFoundError(
                    $"No locations found for vehicle ID {vehicleId}",
                    controller.HttpContext.TraceIdentifier));
            }

            // Get the latest location by timestamp
            var latestLocation = apiResponse.Data.Items.MaxBy(l => l.Timestamp);

            var response = ApiResponse<LocationDto>.SuccessResponse(
                latestLocation!,
                "Latest location retrieved successfully",
                200,
                controller.HttpContext.TraceIdentifier);

            return controller.Ok(response);
        }
        catch
        {
            return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve latest location", controller.HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets locations by multiple IDs in a single request.
    /// Returns 200 OK with all found locations.
    /// </summary>
    /// <param name="controller">The <see cref="LocationController"/> instance.</param>
    /// <param name="ids">Array of location IDs to retrieve.</param>
    /// <returns>Action result with found locations and any not found IDs.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="controller"/> is <see langword="null"/>.</exception>
    [HttpGet("batch")]
    public static async Task<IActionResult> GetLocationsByIds(
        this LocationController controller,
        [FromQuery] Guid[] ids)
    {
        ArgumentNullException.ThrowIfNull(controller);

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
        catch
        {
            return controller.BadRequest(ErrorResponse.ServerError("Failed to retrieve batch locations", controller.HttpContext.TraceIdentifier));
        }
    }
}