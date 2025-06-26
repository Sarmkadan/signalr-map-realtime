// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Base request model for paginated API endpoints.
/// Provides standard pagination parameters with validation.
/// Inheriting from this class ensures consistent pagination across all list endpoints.
/// </summary>
public class PagedRequest
{
    /// <summary>
    /// Page number (1-indexed). Defaults to 1.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page. Defaults to 20, maximum 100.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Optional search query string.
    /// Can be used for full-text search on the resource.
    /// </summary>
    [StringLength(200, ErrorMessage = "Search query cannot exceed 200 characters")]
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Optional sorting field name.
    /// Format: "fieldName" for ascending, "-fieldName" for descending.
    /// Example: "createdAt" or "-updatedAt"
    /// </summary>
    [StringLength(100, ErrorMessage = "Sort field cannot exceed 100 characters")]
    public string? SortBy { get; set; }

    /// <summary>
    /// Optional filtering criteria as JSON.
    /// Implementation depends on the endpoint.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Filter cannot exceed 1000 characters")]
    public string? Filter { get; set; }
}

/// <summary>
/// Request model for vehicle list endpoint.
/// Extends PagedRequest with vehicle-specific filters.
/// </summary>
public class VehicleListRequest : PagedRequest
{
    /// <summary>
    /// Filter by vehicle status (Active, Inactive, Maintenance).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by vehicle type.
    /// </summary>
    public string? VehicleType { get; set; }

    /// <summary>
    /// Filter by assigned user/driver ID.
    /// </summary>
    public Guid? AssignedToId { get; set; }
}

/// <summary>
/// Request model for location list endpoint.
/// Extends PagedRequest with location-specific filters.
/// </summary>
public class LocationListRequest : PagedRequest
{
    /// <summary>
    /// Filter locations by vehicle ID.
    /// </summary>
    public Guid? VehicleId { get; set; }

    /// <summary>
    /// Start date for filtering locations (inclusive).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering locations (inclusive).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter locations by type (GPS, Cellular, etc.).
    /// </summary>
    public string? LocationType { get; set; }

    /// <summary>
    /// Minimum accuracy threshold in meters.
    /// Only include locations with accuracy >= this value.
    /// </summary>
    public double? MinAccuracy { get; set; }
}

/// <summary>
/// Request model for route list endpoint.
/// Extends PagedRequest with route-specific filters.
/// </summary>
public class RouteListRequest : PagedRequest
{
    /// <summary>
    /// Filter by route status (Active, Completed, Cancelled).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by assigned vehicle ID.
    /// </summary>
    public Guid? VehicleId { get; set; }

    /// <summary>
    /// Only include routes created after this date.
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Only include routes created before this date.
    /// </summary>
    public DateTime? CreatedBefore { get; set; }
}

/// <summary>
/// Request model for asset list endpoint.
/// Extends PagedRequest with asset-specific filters.
/// </summary>
public class AssetListRequest : PagedRequest
{
    /// <summary>
    /// Filter by asset type (Package, Equipment, Parcel, etc.).
    /// </summary>
    public string? AssetType { get; set; }

    /// <summary>
    /// Filter by asset status (Active, Archived, Lost).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by currently assigned vehicle ID.
    /// </summary>
    public Guid? CurrentVehicleId { get; set; }
}

/// <summary>
/// Generic update status request for changing entity status.
/// Can be used by multiple endpoints for status changes.
/// </summary>
public class UpdateStatusRequest
{
    /// <summary>
    /// New status value.
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional reason for status change.
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }

    /// <summary>
    /// Optional additional notes.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
