#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Service interface for location tracking operations.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Records a new location point for a vehicle.
    /// </summary>
    /// <param name="locationDto">The location data to record.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the created location DTO.</returns>
    Task<LocationDto> RecordLocationAsync(CreateLocationDto locationDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest location for a vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the latest location DTO, or null if not found.</returns>
    Task<LocationDto?> GetLatestLocationAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location history for a vehicle within a time range.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="startTime">The start time of the range.</param>
    /// <param name="endTime">The end time of the range.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of location DTOs.</returns>
    Task<IEnumerable<LocationDto>> GetLocationHistoryAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations in the last N hours.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="hoursBack">Number of hours to look back.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of location DTOs.</returns>
    Task<IEnumerable<LocationDto>> GetRecentLocationsAsync(int vehicleId, int hoursBack = 24, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds locations within a geographic radius.
    /// </summary>
    /// <param name="centerLat">Center latitude.</param>
    /// <param name="centerLng">Center longitude.</param>
    /// <param name="radiusKm">Search radius in kilometers.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of location DTOs.</returns>
    Task<IEnumerable<LocationDto>> GetLocationsNearbyAsync(double centerLat, double centerLng, double radiusKm = 1.0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location statistics for a vehicle.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="startTime">The start time of the range.</param>
    /// <param name="endTime">The end time of the range.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the location statistics DTO.</returns>
    Task<LocationStatsDto> GetLocationStatsAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations by type.
    /// </summary>
    /// <param name="locationType">The type of location.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the collection of location DTOs.</returns>
    Task<IEnumerable<LocationDto>> GetLocationsByTypeAsync(LocationType locationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates location information.
    /// </summary>
    /// <param name="locationId">The ID of the location to update.</param>
    /// <param name="locationDto">The new location data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the updated location DTO.</returns>
    Task<LocationDto> UpdateLocationAsync(Guid locationId, UpdateLocationDto locationDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all locations.
    /// </summary>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the paginated response of location DTOs.</returns>
    Task<PaginatedResponse<LocationDto>> GetLocationsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a location by its ID.
    /// </summary>
    /// <param name="id">The ID of the location.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the location DTO, or null if not found.</returns>
    Task<LocationDto?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a location by its ID.
    /// </summary>
    /// <param name="id">The ID of the location.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, true if deleted successfully, otherwise false.</returns>
    Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates location coordinates.
    /// </summary>
    /// <param name="latitude">Latitude to validate.</param>
    /// <param name="longitude">Longitude to validate.</param>
    /// <returns>True if coordinates are valid, otherwise false.</returns>
    bool ValidateCoordinates(double latitude, double longitude);

    /// <summary>
    /// Calculates distance between two locations.
    /// </summary>
    /// <param name="lat1">Latitude of point 1.</param>
    /// <param name="lon1">Longitude of point 1.</param>
    /// <param name="lat2">Latitude of point 2.</param>
    /// <param name="lon2">Longitude of point 2.</param>
    /// <returns>Distance in kilometers.</returns>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Retrieves the N closest tracked assets (latest location per vehicle) ordered by haversine distance.
    /// </summary>
    /// <param name="latitude">Reference latitude.</param>
    /// <param name="longitude">Reference longitude.</param>
    /// <param name="count">Number of nearest assets to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the nearest location DTOs.</returns>
    Task<IEnumerable<LocationDto>> GetNearestAssets(double latitude, double longitude, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old location records.
    /// </summary>
    /// <param name="daysOld">Number of days to keep records.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the number of deleted records.</returns>
    Task<int> CleanupOldLocationsAsync(int daysOld = 90, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated location history for a vehicle with time range filtering.
    /// Results are ordered by newest first (descending by RecordedAt).
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle.</param>
    /// <param name="pageNumber">Page number (1-indexed).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="startTime">Optional start time for filtering (inclusive).</param>
    /// <param name="endTime">Optional end time for filtering (inclusive).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the paginated response of location DTOs.</returns>
    Task<PaginatedResponse<LocationDto>> GetVehicleLocationHistoryAsync(
        int vehicleId,
        int pageNumber,
        int pageSize,
        DateTime? startTime = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for location statistics.
/// </summary>
public class LocationStatsDto
{
    /// <summary>Total number of location points.</summary>
    public int PointCount { get; set; }

    /// <summary>Minimum recorded speed in km/h.</summary>
    public double MinSpeed { get; set; }

    /// <summary>Maximum recorded speed in km/h.</summary>
    public double MaxSpeed { get; set; }

    /// <summary>Average speed in km/h.</summary>
    public double AverageSpeed { get; set; }

    /// <summary>Total distance traveled in km.</summary>
    public double TotalDistance { get; set; }
}
