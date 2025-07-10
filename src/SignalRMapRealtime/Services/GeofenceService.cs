// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Events;

/// <summary>Manages configurable geofence zones and evaluates location-based boundary alerts.</summary>
public interface IGeofenceService
{
    /// <summary>Registers a new geofence zone and makes it available for evaluation.</summary>
    /// <param name="dto">Zone definition including shape, coordinates, and metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly registered zone as a DTO.</returns>
    Task<GeofenceDto> RegisterZoneAsync(CreateGeofenceDto dto, CancellationToken cancellationToken = default);

    /// <summary>Removes a zone by its identifier, stopping further evaluation.</summary>
    /// <param name="id">Zone identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the zone existed and was removed; otherwise <c>false</c>.</returns>
    Task<bool> RemoveZoneAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all currently active geofence zones.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of active zone DTOs.</returns>
    Task<IReadOnlyList<GeofenceDto>> GetActiveZonesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a vehicle's position against all active zones and returns discrete boundary-crossing alerts.
    /// Publishes a <see cref="GeofenceViolationEvent"/> for each detected transition.
    /// Entry and exit alerts are only emitted on state change — repeated positions inside a zone produce no duplicates.
    /// </summary>
    /// <param name="vehicleId">Identifier of the vehicle whose location is being checked.</param>
    /// <param name="latitude">Current latitude in decimal degrees.</param>
    /// <param name="longitude">Current longitude in decimal degrees.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of alerts for zones entered or exited since the previous check.</returns>
    Task<IReadOnlyList<GeofenceAlertDto>> CheckLocationAsync(
        Guid vehicleId, double latitude, double longitude,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory implementation of <see cref="IGeofenceService"/>.
/// Tracks per-vehicle zone presence to emit discrete entry and exit alerts on state transitions.
/// Register as a singleton so zone state and presence history persist across requests.
/// </summary>
public sealed class GeofenceService : IGeofenceService
{
    private readonly ConcurrentDictionary<Guid, Geofence> _zones = new();
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _presence = new(); // vehicleId → active zone IDs
    private readonly IEventBus _eventBus;
    private readonly ILogger<GeofenceService> _logger;

    /// <summary>Initializes a new instance of <see cref="GeofenceService"/>.</summary>
    public GeofenceService(IEventBus eventBus, ILogger<GeofenceService> logger)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(logger);
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<GeofenceDto> RegisterZoneAsync(CreateGeofenceDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var zone = new Geofence
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            IsActive = dto.IsActive,
            CenterLatitude = dto.CenterLatitude,
            CenterLongitude = dto.CenterLongitude,
            RadiusKm = dto.RadiusKm,
            PolygonCoordinates = dto.PolygonCoordinates,
            CreatedBy = dto.CreatedBy,
        };

        _zones[zone.Id] = zone;
        _logger.LogInformation("Geofence zone '{Name}' ({Id}) registered.", zone.Name, zone.Id);

        return Task.FromResult(MapToDto(zone));
    }

    /// <inheritdoc/>
    public Task<bool> RemoveZoneAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = _zones.TryRemove(id, out var zone);
        if (removed)
            _logger.LogInformation("Geofence zone '{Name}' ({Id}) removed.", zone!.Name, id);
        return Task.FromResult(removed);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<GeofenceDto>> GetActiveZonesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GeofenceDto> result = _zones.Values
            .Where(z => z.IsActive)
            .Select(MapToDto)
            .ToList();
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GeofenceAlertDto>> CheckLocationAsync(
        Guid vehicleId, double latitude, double longitude,
        CancellationToken cancellationToken = default)
    {
        var currentIds = _zones.Values
            .Where(z => z.IsActive && z.ContainsPoint(latitude, longitude))
            .Select(z => z.Id)
            .ToHashSet();

        var previousIds = _presence.GetOrAdd(vehicleId, _ => new HashSet<Guid>());
        var alerts = new List<GeofenceAlertDto>();

        foreach (var id in currentIds.Except(previousIds).ToList())
            await EmitAlertAsync(alerts, id, vehicleId, latitude, longitude, "Entered");

        foreach (var id in previousIds.Except(currentIds).ToList())
            await EmitAlertAsync(alerts, id, vehicleId, latitude, longitude, "Exited");

        _presence[vehicleId] = currentIds;

        return alerts;
    }

    private async Task EmitAlertAsync(
        List<GeofenceAlertDto> alerts, Guid zoneId, Guid vehicleId,
        double latitude, double longitude, string violationType)
    {
        if (!_zones.TryGetValue(zoneId, out var zone))
            return;

        alerts.Add(new GeofenceAlertDto
        {
            GeofenceId = zone.Id,
            GeofenceName = zone.Name,
            VehicleId = vehicleId,
            Latitude = latitude,
            Longitude = longitude,
            ViolationType = violationType,
        });

        _logger.LogWarning(
            "Geofence boundary crossed: vehicle {VehicleId} {ViolationType} zone '{ZoneName}' ({ZoneId}).",
            vehicleId, violationType.ToLowerInvariant(), zone.Name, zone.Id);

        await _eventBus.PublishAsync(new GeofenceViolationEvent
        {
            GeofenceId = zone.Id,
            VehicleId = vehicleId,
            ViolationType = violationType,
            Latitude = latitude,
            Longitude = longitude,
        });
    }

    private static GeofenceDto MapToDto(Geofence zone) => new()
    {
        Id = zone.Id,
        Name = zone.Name,
        Description = zone.Description,
        Type = zone.Type.ToString(),
        IsActive = zone.IsActive,
        CenterLatitude = zone.CenterLatitude,
        CenterLongitude = zone.CenterLongitude,
        RadiusKm = zone.RadiusKm,
        PolygonCoordinates = zone.PolygonCoordinates,
        CreatedAt = zone.CreatedAt,
        UpdatedAt = zone.UpdatedAt,
        CreatedBy = zone.CreatedBy,
    };
}

/// <summary>Extension methods for registering geofencing services in the DI container.</summary>
public static class GeofencingExtensions
{
    /// <summary>
    /// Adds the geofencing service as a singleton so zone configuration and vehicle presence
    /// history are preserved for the lifetime of the application.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGeofencing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IGeofenceService, GeofenceService>();
        return services;
    }
}
