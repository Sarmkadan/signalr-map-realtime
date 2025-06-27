// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using AutoMapper;
using SignalRMapRealtime.Constants;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Exceptions;

/// <summary>
/// Service for managing location tracking operations.
/// </summary>
public class LocationService : ILocationService
{
    private readonly LocationRepository _locationRepository;
    private readonly VehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of LocationService.
    /// </summary>
    public LocationService(LocationRepository locationRepository, VehicleRepository vehicleRepository, IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(locationRepository);
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        _locationRepository = locationRepository;
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Records a new location point for a vehicle with validation and persistence.
    /// </summary>
    public async Task<LocationDto> RecordLocationAsync(CreateLocationDto locationDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locationDto);

        if (!ValidateCoordinates(locationDto.Latitude, locationDto.Longitude))
            throw new InvalidLocationException(locationDto.Latitude, locationDto.Longitude);

        var vehicle = await _vehicleRepository.GetByIdAsync(locationDto.VehicleId, cancellationToken);
        if (vehicle == null)
            throw new VehicleNotFoundException(locationDto.VehicleId);

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

        await _locationRepository.AddAsync(location, cancellationToken);
        await _locationRepository.SaveChangesAsync(cancellationToken);

        vehicle.RecordLocation(location);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LocationDto>(location);
    }

    /// <summary>
    /// Gets the latest location for a vehicle.
    /// </summary>
    public async Task<LocationDto?> GetLatestLocationAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        var location = await _locationRepository.GetLatestLocationByVehicleAsync(vehicleId);
        return location == null ? null : _mapper.Map<LocationDto>(location);
    }

    /// <summary>
    /// Gets location history for a vehicle within a time range.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetLocationHistoryAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        var locations = await _locationRepository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Gets locations in the last N hours.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetRecentLocationsAsync(int vehicleId, int hoursBack = 24, CancellationToken cancellationToken = default)
    {
        var locations = await _locationRepository.GetRecentLocationsAsync(vehicleId, hoursBack);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Finds locations within a geographic radius.
    /// </summary>
    public async Task<IEnumerable<LocationDto>> GetLocationsNearbyAsync(double centerLat, double centerLng, double radiusKm = 1.0, CancellationToken cancellationToken = default)
    {
        var locations = await _locationRepository.GetLocationsNearbyAsync(centerLat, centerLng, radiusKm);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Gets location statistics for a vehicle.
    /// </summary>
    public async Task<LocationStatsDto> GetLocationStatsAsync(int vehicleId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        var (count, minSpeed, maxSpeed, avgSpeed) = await _locationRepository.GetLocationStatsAsync(vehicleId, startTime, endTime);

        var locations = await _locationRepository.GetLocationsByTimeRangeAsync(vehicleId, startTime, endTime);
        double totalDistance = 0;
        var sortedLocations = locations.OrderBy(l => l.RecordedAt).ToList();

        for (int i = 1; i < sortedLocations.Count; i++)
        {
            totalDistance += sortedLocations[i].CalculateDistanceTo(sortedLocations[i - 1]);
        }

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
        var locations = await _locationRepository.GetLocationsByTypeAsync(locationType);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    /// <summary>
    /// Updates location information.
    /// </summary>
    public async Task<LocationDto> UpdateLocationAsync(int locationId, UpdateLocationDto locationDto, CancellationToken cancellationToken = default)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);
        if (location == null)
            throw new InvalidLocationException("Location not found.");

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

        await _locationRepository.UpdateAsync(location, cancellationToken);
        await _locationRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LocationDto>(location);
    }

    /// <summary>
    /// Validates that coordinates are within geographic bounds.
    /// </summary>
    public bool ValidateCoordinates(double latitude, double longitude)
    {
        return latitude >= LocationConstants.MinLatitude &&
               latitude <= LocationConstants.MaxLatitude &&
               longitude >= LocationConstants.MinLongitude &&
               longitude <= LocationConstants.MaxLongitude;
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
    /// Cleans up old location records older than specified days.
    /// </summary>
    public async Task<int> CleanupOldLocationsAsync(int daysOld = 90, CancellationToken cancellationToken = default)
    {
        return await _locationRepository.DeleteOldLocationsAsync(daysOld);
    }
}
