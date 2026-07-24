#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Events;

/// <summary>
/// Service that detects when vehicles become stale (no location updates within configured window)
/// and when they recover (start sending location updates again after being stale).
/// This service runs periodically to check vehicle status and publish appropriate domain events.
/// </summary>
public class VehicleStaleDetectionService : BackgroundService
{
    private readonly ILogger<VehicleStaleDetectionService> _logger;
    private readonly IEventBus _eventBus;
    private readonly VehicleRepository _vehicleRepository;
    private readonly LocationRepository _locationRepository;
    private readonly SignalrMapRealtimeOptions _options;
    private readonly Dictionary<int, DateTime> _lastActiveTimes = new();
    private readonly HashSet<int> _currentlyStaleVehicles = new();

    /// <summary>
    /// Initializes a new instance of the VehicleStaleDetectionService.
    /// </summary>
    /// <param name="logger">Logger for tracking service operations.</param>
    /// <param name="eventBus">Event bus for publishing domain events.</param>
    /// <param name="vehicleRepository">Repository for accessing vehicle data.</param>
    /// <param name="locationRepository">Repository for accessing location data.</param>
    /// <param name="options">Application configuration options.</param>
    public VehicleStaleDetectionService(
        ILogger<VehicleStaleDetectionService> logger,
        IEventBus eventBus,
        VehicleRepository vehicleRepository,
        LocationRepository locationRepository,
        IOptions<SignalrMapRealtimeOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(locationRepository);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _eventBus = eventBus;
        _vehicleRepository = vehicleRepository;
        _locationRepository = locationRepository;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the stale detection window in minutes from configuration.
    /// </summary>
    private int StaleWindowMinutes => _options.AppInfo.LocationUpdateIntervalSeconds / 60 * 2;

    /// <summary>
    /// Background service execution that periodically checks for stale vehicles.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vehicle stale detection service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DetectStaleVehiclesAsync(stoppingToken).ConfigureAwait(false);
                await DetectRecoveredVehiclesAsync(stoppingToken).ConfigureAwait(false);

                // Run every 30 seconds to check for stale vehicles
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Vehicle stale detection service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vehicle stale detection service");
                // Continue on error to prevent worker from stopping
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("Vehicle stale detection service stopped");
    }

    /// <summary>
    /// Detects vehicles that have become stale (no location updates within the stale window).
    /// </summary>
    private async Task DetectStaleVehiclesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var staleWindow = TimeSpan.FromMinutes(StaleWindowMinutes);
            var cutoffTime = now - staleWindow;

            _logger.LogDebug("Checking for stale vehicles (cutoff: {CutoffTime})", cutoffTime);

            // Get all online vehicles that have location tracking
            var onlineVehicles = await _vehicleRepository.GetOnlineVehiclesAsync().ConfigureAwait(false);

            foreach (var vehicle in onlineVehicles)
            {
                if (vehicle.LastLocation == null)
                {
                    _logger.LogDebug("Vehicle {VehicleId} is online but has no location data", vehicle.Id);
                    continue;
                }

                var vehicleId = vehicle.Id;
                var lastUpdateTime = vehicle.LastLocation.RecordedAt;
                var timeSinceLastUpdate = now - lastUpdateTime;

                // Check if vehicle is already tracked as stale
                if (_currentlyStaleVehicles.Contains(vehicleId))
                {
                    continue;
                }

                // Check if vehicle has exceeded the stale window
                if (timeSinceLastUpdate > staleWindow)
                {
                    _logger.LogInformation(
                        "Vehicle {VehicleId} ({Registration}) became stale. Last update was {LastUpdateMinutes} minutes ago (cutoff: {CutoffMinutes} minutes)",
                        vehicleId,
                        vehicle.RegistrationNumber,
                        timeSinceLastUpdate.TotalMinutes,
                        staleWindow.TotalMinutes);

                    // Mark as stale
                    _currentlyStaleVehicles.Add(vehicleId);
                    _lastActiveTimes[vehicleId] = lastUpdateTime;

                    // Publish VehicleStaleEvent
                    var staleEvent = new VehicleStaleEvent
                    {
                        VehicleId = vehicleId,
                        VehicleRegistration = vehicle.RegistrationNumber,
                        VehicleName = vehicle.Name ?? vehicle.RegistrationNumber,
                        LastUpdateTime = lastUpdateTime,
                        StaleSince = now,
                        StaleWindowMinutes = StaleWindowMinutes,
                        TimeSinceLastUpdateMinutes = timeSinceLastUpdate.TotalMinutes,
                        IsRecovery = false,
                        WasPreviouslyStale = false
                    };

                    await _eventBus.PublishAsync(staleEvent).ConfigureAwait(false);
                    _logger.LogInformation("Published VehicleStaleEvent for vehicle {VehicleId}", vehicleId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting stale vehicles");
            throw;
        }
    }

    /// <summary>
    /// Detects vehicles that have recovered from stale state (started sending location updates again).
    /// </summary>
    private async Task DetectRecoveredVehiclesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;

            _logger.LogDebug("Checking for recovered vehicles (currently tracking {StaleCount} stale vehicles)", _currentlyStaleVehicles.Count);

            // Check each vehicle that was previously marked as stale
            foreach (var vehicleId in _currentlyStaleVehicles.ToList())
            {
                try
                {
                    // Get the vehicle's latest location
                    var latestLocation = await _locationRepository.GetLatestLocationByVehicleAsync(vehicleId).ConfigureAwait(false);

                    if (latestLocation != null)
                    {
                        var lastUpdateTime = latestLocation.RecordedAt;
                        var timeInStaleState = now - _lastActiveTimes[vehicleId];

                        // Check if vehicle has sent a new location update (recovered)
                        if (now - lastUpdateTime < TimeSpan.FromMinutes(5)) // Recent update means recovered
                        {
                            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
                            if (vehicle != null)
                            {
                                _logger.LogInformation(
                                    "Vehicle {VehicleId} ({Registration}) recovered from stale state. Was stale for {StaleDuration} minutes",
                                    vehicleId,
                                    vehicle.RegistrationNumber,
                                    timeInStaleState.TotalMinutes);

                                // Remove from stale tracking
                                _currentlyStaleVehicles.Remove(vehicleId);
                                _lastActiveTimes.Remove(vehicleId);

                                // Publish VehicleActiveEvent
                                var activeEvent = new VehicleActiveEvent
                                {
                                    VehicleId = vehicleId,
                                    VehicleRegistration = vehicle.RegistrationNumber,
                                    VehicleName = vehicle.Name ?? vehicle.RegistrationNumber,
                                    RecoveryTime = now,
                                    StaleWindowMinutes = StaleWindowMinutes,
                                    TimeInStaleStateMinutes = timeInStaleState.TotalMinutes
                                };

                                await _eventBus.PublishAsync(activeEvent).ConfigureAwait(false);
                                _logger.LogInformation("Published VehicleActiveEvent for vehicle {VehicleId}", vehicleId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking recovery status for vehicle {VehicleId}", vehicleId);
                    // Remove from tracking to prevent repeated errors
                    _currentlyStaleVehicles.Remove(vehicleId);
                    _lastActiveTimes.Remove(vehicleId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting recovered vehicles");
            throw;
        }
    }

    /// <summary>
    /// Resets the stale tracking for a specific vehicle (e.g., when vehicle is removed from tracking).
    /// </summary>
    /// <param name="vehicleId">The vehicle ID to reset.</param>
    public void ResetVehicleStaleTracking(int vehicleId)
    {
        if (_currentlyStaleVehicles.Remove(vehicleId))
        {
            _lastActiveTimes.Remove(vehicleId);
            _logger.LogDebug("Reset stale tracking for vehicle {VehicleId}", vehicleId);
        }
    }
}