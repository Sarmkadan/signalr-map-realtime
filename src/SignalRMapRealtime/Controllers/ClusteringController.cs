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

/// <summary>
/// Provides REST endpoints for location clustering and heatmap density visualisation.
/// Results are computed on demand from the location history stored in the database.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClusteringController : ControllerBase
{
    private readonly IClusteringService _clusteringService;
    private readonly ILogger<ClusteringController> _logger;

    /// <summary>Initializes a new instance of <see cref="ClusteringController"/>.</summary>
    public ClusteringController(IClusteringService clusteringService, ILogger<ClusteringController> logger)
    {
        ArgumentNullException.ThrowIfNull(clusteringService);
        ArgumentNullException.ThrowIfNull(logger);
        _clusteringService = clusteringService;
        _logger = logger;
    }

    /// <summary>
    /// Returns location clusters grouped by a configurable geographic grid cell size.
    /// Useful for rendering Leaflet marker clusters at lower zoom levels.
    /// </summary>
    /// <param name="vehicleId">Optional vehicle ID to restrict results to a single asset.</param>
    /// <param name="from">Start of the time window (UTC). Defaults to 24 hours ago.</param>
    /// <param name="to">End of the time window (UTC). Defaults to now.</param>
    /// <param name="gridCellKm">Grid cell edge length in km (default 0.5, minimum 0.1).</param>
    /// <param name="minLatitude">Optional southern bounding-box limit.</param>
    /// <param name="maxLatitude">Optional northern bounding-box limit.</param>
    /// <param name="minLongitude">Optional western bounding-box limit.</param>
    /// <param name="maxLongitude">Optional eastern bounding-box limit.</param>
    [HttpGet("clusters")]
    [ProducesResponseType(typeof(ApiResponse<ClusterResponseDto>), 200)]
    public async Task<IActionResult> GetClusters(
        [FromQuery] int? vehicleId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] double gridCellKm = 0.5,
        [FromQuery] double? minLatitude = null,
        [FromQuery] double? maxLatitude = null,
        [FromQuery] double? minLongitude = null,
        [FromQuery] double? maxLongitude = null)
    {
        try
        {
            var request = new ClusterQueryRequest
            {
                VehicleId = vehicleId,
                From = from,
                To = to,
                GridCellKm = Math.Max(0.1, gridCellKm),
                MinLatitude = minLatitude,
                MaxLatitude = maxLatitude,
                MinLongitude = minLongitude,
                MaxLongitude = maxLongitude,
            };

            var result = await _clusteringService.GetClustersAsync(request).ConfigureAwait(false);

            return Ok(ApiResponse<ClusterResponseDto>.SuccessResponse(
                result, "Clusters retrieved successfully", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing location clusters. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to compute clusters", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Returns a normalised density heatmap derived from location history.
    /// Each tile carries an intensity value between 0.0 (no activity) and 1.0 (peak activity)
    /// relative to the most active tile in the response.
    /// </summary>
    /// <param name="vehicleId">Optional vehicle ID to restrict results to a single asset.</param>
    /// <param name="from">Start of the time window (UTC). Defaults to 24 hours ago.</param>
    /// <param name="to">End of the time window (UTC). Defaults to now.</param>
    /// <param name="gridCellKm">Grid cell edge length in km (default 0.5, minimum 0.1).</param>
    /// <param name="minLatitude">Optional southern bounding-box limit.</param>
    /// <param name="maxLatitude">Optional northern bounding-box limit.</param>
    /// <param name="minLongitude">Optional western bounding-box limit.</param>
    /// <param name="maxLongitude">Optional eastern bounding-box limit.</param>
    [HttpGet("heatmap")]
    [ProducesResponseType(typeof(ApiResponse<HeatmapResponseDto>), 200)]
    public async Task<IActionResult> GetHeatmap(
        [FromQuery] int? vehicleId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] double gridCellKm = 0.5,
        [FromQuery] double? minLatitude = null,
        [FromQuery] double? maxLatitude = null,
        [FromQuery] double? minLongitude = null,
        [FromQuery] double? maxLongitude = null)
    {
        try
        {
            var request = new ClusterQueryRequest
            {
                VehicleId = vehicleId,
                From = from,
                To = to,
                GridCellKm = Math.Max(0.1, gridCellKm),
                MinLatitude = minLatitude,
                MaxLatitude = maxLatitude,
                MinLongitude = minLongitude,
                MaxLongitude = maxLongitude,
            };

            var result = await _clusteringService.GetHeatmapAsync(request).ConfigureAwait(false);

            return Ok(ApiResponse<HeatmapResponseDto>.SuccessResponse(
                result, "Heatmap computed successfully", 200, HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing heatmap. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ErrorResponse.ServerError("Failed to compute heatmap", HttpContext.TraceIdentifier));
        }
    }
}
