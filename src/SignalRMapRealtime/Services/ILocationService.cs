// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

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
    Task<LocationDto> RecordLocationAsync(CreateLocationDto locationDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest location for a vehicle.
    /// </summary>
    Task<LocationDto?> GetLatestLocationAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location history for a vehicle within a time range.
    /// </summary>
    Task<IEnumerable<LocationDto>> GetLocationHistoryAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations in the last N hours.
    /// </summary>
    Task<IEnumerable<LocationDto>> GetRecentLocationsAsync(int vehicleId, int hoursBack = 24, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds locations within a geographic radius.
    /// </summary>
    Task<IEnumerable<LocationDto>> GetLocationsNearbyAsync(double centerLat, double centerLng, double radiusKm = 1.0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location statistics for a vehicle.
    /// </summary>
    Task<LocationStatsDto> GetLocationStatsAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations by type.
    /// </summary>
    Task<IEnumerable<LocationDto>> GetLocationsByTypeAsync(LocationType locationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates location information.
    /// </summary>
    Task<LocationDto> UpdateLocationAsync(int locationId, UpdateLocationDto locationDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates location coordinates.
    /// </summary>
    bool ValidateCoordinates(double latitude, double longitude);

    /// <summary>
    /// Calculates distance between two locations.
    /// </summary>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Cleans up old location records.
    /// </summary>
    Task<int> CleanupOldLocationsAsync(int daysOld = 90, CancellationToken cancellationToken = default);
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
