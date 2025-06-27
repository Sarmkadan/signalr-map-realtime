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
/// API controller for managing asset tracking.
/// Assets are trackable items like packages, equipment, or parcels.
/// Provides CRUD operations for asset management and status tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssetController : ControllerBase
{
    private readonly IRepository<Domain.Models.Asset> _assetRepository;
    private readonly ILogger<AssetController> _logger;

    public AssetController(
        IRepository<Domain.Models.Asset> assetRepository,
        ILogger<AssetController> logger)
    {
        _assetRepository = assetRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all assets with pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAssets(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var (validPageNumber, validPageSize) = PaginationExtensions.NormalizePaginationParameters(pageNumber, pageSize, 100);

            var allAssets = await _assetRepository.GetAllAsync();
            var result = PaginatedResponse<AssetDto>.FromList(
                allAssets.Select(MapToDto).ToList(),
                validPageNumber,
                validPageSize);

            var response = ApiResponse<PaginatedResponse<AssetDto>>.SuccessResponse(
                result,
                "Assets retrieved successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve assets", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Gets a specific asset by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetById(int id)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null)
                return NotFound(ErrorResponse.NotFoundError($"Asset with ID {id} not found", HttpContext.TraceIdentifier));

            var response = ApiResponse<AssetDto>.SuccessResponse(
                MapToDto(asset),
                "Asset retrieved successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset {AssetId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to retrieve asset", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Creates a new asset.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] AssetDto createAssetDto)
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

            var asset = new Domain.Models.Asset
            {
                Name = createAssetDto.Name,
                AssetType = createAssetDto.AssetType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _assetRepository.AddAsync(asset);
            await _assetRepository.SaveChangesAsync();

            var response = ApiResponse<AssetDto>.SuccessResponse(
                MapToDto(asset),
                "Asset created successfully",
                201,
                HttpContext.TraceIdentifier);

            return CreatedAtAction(nameof(GetAssetById), new { id = asset.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to create asset", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Updates an asset.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] AssetDto updateAssetDto)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null)
                return NotFound(ErrorResponse.NotFoundError($"Asset with ID {id} not found", HttpContext.TraceIdentifier));

            asset.Name = updateAssetDto.Name;
            asset.AssetType = updateAssetDto.AssetType;
            asset.UpdatedAt = DateTime.UtcNow;

            await _assetRepository.SaveChangesAsync();

            var response = ApiResponse<AssetDto>.SuccessResponse(
                MapToDto(asset),
                "Asset updated successfully",
                200,
                HttpContext.TraceIdentifier);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset {AssetId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to update asset", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Deletes an asset.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null)
                return NotFound(ErrorResponse.NotFoundError($"Asset with ID {id} not found", HttpContext.TraceIdentifier));

            await _assetRepository.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset {AssetId}. TraceId: {TraceId}", id, HttpContext.TraceIdentifier);
            return StatusCode(500, ErrorResponse.ServerError("Failed to delete asset", HttpContext.TraceIdentifier));
        }
    }

    /// <summary>
    /// Maps domain model to DTO.
    /// </summary>
    private AssetDto MapToDto(Domain.Models.Asset asset)
    {
        return new AssetDto
        {
            Id = asset.Id.ToString(),
            Name = asset.Name,
            AssetType = asset.AssetType,
            Status = asset.Condition ?? "Unknown",
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }
}
