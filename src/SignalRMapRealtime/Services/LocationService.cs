#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using SignalRMapRealtime.Constants;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Exceptions;
using SignalRMapRealtime.Models;

/// <summary>
/// Service for managing location tracking operations.
/// </summary>
public class LocationService : ILocationService
{
    private readonly LocationRepository _locationRepository;
    private readonly VehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<LocationService> _logger;

    /// <summary>
    /// Initializes a new instance of LocationService.
    /// </summary>
    public LocationService(
        LocationRepository locationRepository,
        VehicleRepository vehicleRepository,
        IMapper mapper,
        ILogger<LocationService> logger)
    {
        ArgumentNullException.ThrowIfNull(locationRepository);
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(logger);
        _locationRepository = locationRepository;
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Records a new location point for a vehicle with validation and persistence.
    /// </summary>
    public async Task<LocationDto> RecordLocationAsync(CreateLocationDto locationDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Recording location for vehicle {VehicleId}: Lat={Latitude}, Lng={Longitude}, Speed={Speed} km/h, Type={LocationType}",
            locationDto.VehicleId,
            locationDto.Latitude,
            locationDto.Longitude,
            locationDto.Speed,
            locationDto.LocationType);

        ArgumentNullException.ThrowIfNull(locationDto);

        if (!ValidateCoordinates(locationDto.Latitude, locationDto.Longitude))
        {
            _logger.LogWarning(
                "Invalid coordinates detected: Lat={Latitude}, Lng={Longitude}. Valid range: Lat [{MinLat}-{MaxLat}], Lng [{MinLng}-{MaxLng}]",
                locationDto.Latitude,
                locationDto.Longitude,
                LocationConstants.MinLatitude,
                LocationConstants.MaxLatitude,
                LocationConstants.MinLongitude,
                LocationConstants.MaxLongitude);
            throw new InvalidLocationException(locationDto.Latitude, locationDto.Longitude);
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(locationDto.VehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found when recording location", locationDto.VehicleId);
            throw new VehicleNotFoundException(locationDto.VehicleId);
        }

        var location = new Location
        {
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            Altitude = locationDto.Altitude,
            Accuracy = locationDto.Accuracy,
            Speed = locationDto.Speed,
            Bearing = locationDto.Bearing,
            LocationType = locationDto.LocationType,
            Address = locationDto.Address,
            Notes = locationDto.Notes,
            VehicleId = locationDto.VehicleId,
            RecordedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _locationRepository.AddAsync(location, cancellationToken).ConfigureAwait(false);
        await _locationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        vehicle.RecordLocation(location);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Successfully recorded location {LocationId} for vehicle {VehicleId}", location.Id, locationDto.VehicleId);
        return _mapper.Map<LocationDto>(location);
    }

    /// <summary>
    /// Gets the latest location for a vehicle.
    /// </summary>
    public async Task<LocationDto?> GetLatestLocationAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving latest location for vehicle {VehicleId}", vehicleId);
        var location = await _locationRepository.GetLatestLocationByVehicleAsync(vehicleId).ConfigureAwait(false);
        return location is null ? null : _mapper.Map<LocationDto>(location);
    }

    /// <summary>
    /// Gets location history for a vehicle within a time range.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetLocationHistoryAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving location history for vehicle {VehicleId} from {StartTime} to {EndTime}", vehicleId, startTime, endTime);
        var locations = await _locationRepository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Gets locations in the last N hours.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetRecentLocationsAsync(int vehicleId, int hoursBack = 24, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving recent locations for vehicle {VehicleId} (last {HoursBack} hours)", vehicleId, hoursBack);
        var locations = await _locationRepository.GetRecentLocationsAsync(vehicleId, hoursBack).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Finds locations within a geographic radius.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetLocationsNearbyAsync(double centerLat, double centerLng, double radiusKm = 1.0, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Searching for locations near {CenterLat}, {CenterLng} within {RadiusKm} km radius", centerLat, centerLng, radiusKm);
        var locations = await _locationRepository.GetLocationsNearbyAsync(centerLat, centerLng, radiusKm).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Gets location statistics for a vehicle.
    /// </summary>
    public async Task<LocationStatsDto> GetLocationStatsAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calculating location statistics for vehicle {VehicleId} from {StartTime} to {EndTime}", vehicleId, startTime, endTime);
        var (count, minSpeed, maxSpeed, avgSpeed) = await _locationRepository.GetLocationStatsAsync(vehicleId, startTime, endTime).ConfigureAwait(false);

        var locations = await _locationRepository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime).ConfigureAwait(false);
        double totalDistance = 0;
        var sortedLocations = locations.OrderBy(l => l.RecordedAt).ToList();

        for (int i = 1; i < sortedLocations.Count; i++)
        {
            totalDistance += sortedLocations[i].CalculateDistanceTo(sortedLocations[i - 1]);
        }

        _logger.LogDebug(
            "Location stats for vehicle {VehicleId}: {Count} points, {TotalDistance} km total, {AvgSpeed} km/h avg speed",
            vehicleId,
            count,
            Math.Round(totalDistance, 2),
            avgSpeed);

        return new LocationStatsDto
        {
            PointCount = count,
            MinSpeed = minSpeed,
            MaxSpeed = maxSpeed,
            AverageSpeed = avgSpeed,
            TotalDistance = totalDistance
        };
    }

    /// <summary>
    /// Gets locations by type.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetLocationsByTypeAsync(LocationType locationType, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving locations with type {LocationType}", locationType);
        var locations = await _locationRepository.GetLocationsByTypeAsync(locationType).ConfigureAwait(false);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Updates location information.
    /// </summary>
    public async Task<LocationDto> UpdateLocationAsync(Guid locationId, UpdateLocationDto locationDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating location {LocationId}", locationId);

        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken).ConfigureAwait(false);
        if (location is null)
        {
            _logger.LogWarning("Location {LocationId} not found for update", locationId);
            throw new InvalidLocationException("Location not found.");
        }

        if (locationDto.Speed.HasValue)
            location.Speed = locationDto.Speed;
        if (locationDto.Bearing.HasValue)
            location.Bearing = locationDto.Bearing;
        if (!string.IsNullOrWhiteSpace(locationDto.Address))
            location.Address = locationDto.Address;
        if (!string.IsNullOrWhiteSpace(locationDto.Notes))
            location.Notes = locationDto.Notes;
        if (locationDto.LocationType.HasValue)
            location.LocationType = locationDto.LocationType.Value;

        await _locationRepository.UpdateAsync(location, cancellationToken).ConfigureAwait(false);
        await _locationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Updated location {LocationId}", locationId);
        return _mapper.Map<LocationDto>(location);
    }

    /// <summary>
    /// Gets all locations.
    /// </summary>
    public async Task<PaginatedResponse<LocationDto>> GetLocationsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving paginated locations: page {PageNumber}, size {PageSize}", pageNumber, pageSize);
        var locations = await _locationRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var locationDtos = _mapper.Map<IEnumerable<LocationDto>>(locations);
        return PaginatedResponse<LocationDto>.FromList(locationDtos, pageNumber, pageSize);
    }

    /// <summary>
    /// Gets a location by its ID.
    /// </summary>
    public async Task<LocationDto?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving location by ID {LocationId}", id);
        var location = await _locationRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return location is null ? null : _mapper.Map<LocationDto>(location);
    }

    /// <summary>
    /// Deletes a location by its ID.
    /// </summary>
    public async Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting location {LocationId}", id);

        var existing = await _locationRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            _logger.LogWarning("Cannot delete location {LocationId}: location not found", id);
            return false;
        }

        await _locationRepository.RemoveAsync(existing, cancellationToken).ConfigureAwait(false);
        await _locationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Deleted location {LocationId}", id);
        return true;
    }

    /// <summary>
    /// Validates that coordinates are within geographic bounds.
    /// </summary>
    public bool ValidateCoordinates(double latitude, double longitude)
    {
        bool isValid = latitude >= LocationConstants.MinLatitude &&
                      latitude <= LocationConstants.MaxLatitude &&
                      longitude >= LocationConstants.MinLongitude &&
                      longitude <= LocationConstants.MaxLongitude;

        if (!isValid)
        {
            _logger.LogWarning("Coordinate validation failed: Lat={Latitude}, Lng={Longitude}", latitude, longitude);
        }

        return isValid;
    }

    /// <summary>
    /// Calculates distance between two geographic points.
    /// </summary>
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = LocationConstants.EarthRadiusKm;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                  Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                  Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    /// <summary>
    /// Retrieves the N closest tracked assets (latest location per vehicle) ordered by haversine distance.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetNearestAssets(double latitude, double longitude, int count, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving {Count} nearest assets to lat {Latitude}, lon {Longitude}", count, latitude, longitude);

        // Get all locations (could be large; in a real system we'd query for latest per vehicle)
        var allLocations = await _locationRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        // Determine the latest location per vehicle
        var latestPerVehicle = allLocations
            .GroupBy(l => l.VehicleId)
            .Select(g => g.OrderByDescending(l => l.RecordedAt).First())
            .ToList();

        // Compute distance and take the closest N
        var nearest = latestPerVehicle
            .Select(l => new
            {
                Location = l,
                Distance = CalculateDistance(latitude, longitude, l.Latitude, l.Longitude)
            })
            .OrderBy(x => x.Distance)
            .Take(count)
            .Select(x => x.Location)
            .ToList();

        return _mapper.Map<IEnumerable<LocationDto>>(nearest);
    }

    /// <summary>
    /// Cleans up old location records older than specified days.
    /// </summary>
    public async Task<int> CleanupOldLocationsAsync(int daysOld = 90, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cleaning up locations older than {DaysOld} days", daysOld);
        var count = await _locationRepository.DeleteOldLocationsAsync(daysOld).ConfigureAwait(false);
        _logger.LogInformation("Cleaned up {Count} old locations", count);
        return count;
    }
}
