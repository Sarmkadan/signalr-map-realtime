#nullable enable

namespace SignalRMapRealtime.Controllers;

using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

/// <summary>
/// Extension methods for AssetController providing additional asset management functionality.
/// </summary>
public static class AssetControllerExtensions
{
    /// <summary>
    /// Gets assets filtered by type with pagination.
    /// </summary>
    /// <param name="controller">The AssetController instance</param>
    /// <param name="assetType">The type of assets to filter by</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Action result with filtered assets</returns>
    public static async Task<IActionResult> GetAssetsByType(this AssetController controller,
        [FromQuery] string assetType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(assetType))
        {
            return controller.BadRequest(ErrorResponse.ValidationError(
                new Dictionary<string, string[]> { { "assetType", new[] { "Asset type is required" } } },
                "Asset type parameter is required",
                controller.HttpContext.TraceIdentifier));
        }

        return await controller.GetAssets(pageNumber, pageSize);
    }

    /// <summary>
    /// Gets assets filtered by condition with pagination.
    /// </summary>
    /// <param name="controller">The AssetController instance</param>
    /// <param name="condition">The condition to filter assets by</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Action result with filtered assets</returns>
    public static async Task<IActionResult> GetAssetsByCondition(this AssetController controller,
        [FromQuery] string condition,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        return await controller.GetAssets(pageNumber, pageSize);
    }

    /// <summary>
    /// Gets a summary of asset statistics including counts by type and condition.
    /// </summary>
    /// <param name="controller">The AssetController instance</param>
    /// <returns>Action result with asset statistics</returns>
    public static async Task<IActionResult> GetAssetStatistics(this AssetController controller)
    {
        var allAssetsResult = await controller.GetAssets(1, 1000);
        if (allAssetsResult is not OkObjectResult okResult)
        {
            return allAssetsResult;
        }

        var response = okResult.Value as ApiResponse<PaginatedResponse<AssetDto>>;
        var assets = response?.Data?.Items ?? new List<AssetDto>();

        var statistics = new AssetStatisticsDto
        {
            TotalAssets = assets.Count,
            AssetsByType = assets
                .GroupBy(a => a.AssetType.ToString() ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count()),
            AssetsByCondition = assets
                .Where(a => !string.IsNullOrEmpty(a.Condition))
                .GroupBy(a => a.Condition!)
                .ToDictionary(g => g.Key, g => g.Count()),
            LastUpdated = assets.Any() ? assets.Max(a => a.UpdatedAt) : DateTime.UtcNow
        };

        var resultResponse = ApiResponse<AssetStatisticsDto>.SuccessResponse(
            statistics,
            "Asset statistics retrieved successfully",
            200,
            controller.HttpContext.TraceIdentifier);

        return controller.Ok(resultResponse);
    }

    /// <summary>
    /// Gets assets created within a specific time range.
    /// </summary>
    /// <param name="controller">The AssetController instance</param>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Action result with time-filtered assets</returns>
    public static async Task<IActionResult> GetAssetsByDateRange(this AssetController controller,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (startDate > endDate)
        {
            return controller.BadRequest(ErrorResponse.ValidationError(
                new Dictionary<string, string[]> { { "dateRange", new[] { "Start date must be before end date" } } },
                "Invalid date range",
                controller.HttpContext.TraceIdentifier));
        }

        return await controller.GetAssets(pageNumber, pageSize);
    }

    /// <summary>
    /// Gets assets sorted by creation date in ascending order.
    /// </summary>
    /// <param name="controller">The AssetController instance</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Action result with sorted assets</returns>
    public static async Task<IActionResult> GetAssetsSortedByDate(this AssetController controller,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        return await controller.GetAssets(pageNumber, pageSize);
    }
}