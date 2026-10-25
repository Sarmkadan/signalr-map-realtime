#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.DependencyInjection;
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
    /// <summary>
    /// Number of location update intervals that may elapse before a vehicle is considered stale.
    /// The stale window is derived as <c>LocationUpdateIntervalSeconds / 60 * StaleWindowIntervalMultiplier</c> minutes.
    /// </summary>
    private const int StaleWindowIntervalMultiplier = 2;

    /// <summary>
    /// Age in minutes of the most recent location below which a previously stale vehicle is considered recovered.
    /// </summary>
    private const int RecoveryWindowMinutes = 5;

    private readonly ILogger<VehicleStaleDetectionService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SignalrMapRealtimeOptions _options;
    private readonly Dictionary<int, DateTime> _lastActiveTimes = new();
    private readonly HashSet<int> _currentlyStaleVehicles = new();

    /// <summary>
    /// Initializes a new instance of the VehicleStaleDetectionService.
    /// </summary>
    /// <param name="logger">Logger for tracking service operations.</param>
    /// <param name="scopeFactory">Factory used to create a scope per detection cycle, so scoped services
    /// (repositories, domain event dispatcher) are resolved from a valid lifetime.</param>
    /// <param name="options">Application configuration options.</param>
    public VehicleStaleDetectionService(
        ILogger<VehicleStaleDetectionService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<SignalrMapRealtimeOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the stale detection window in minutes from configuration.
    /// </summary>
    private int StaleWindowMinutes => _options.AppInfo.LocationUpdateIntervalSeconds / 60 * StaleWindowIntervalMultiplier;

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
                using (var scope = _scopeFactory.CreateScope())
                {
                    await DetectStaleVehiclesAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);
                    await DetectRecoveredVehiclesAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);
                }

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
    private async Task DetectStaleVehiclesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var vehicleRepository = services.GetRequiredService<VehicleRepository>();
        var eventDispatcher = services.GetRequiredService<IDomainEventDispatcher>();

        try
        {
            var now = DateTime.UtcNow;
            var staleWindow = TimeSpan.FromMinutes(StaleWindowMinutes);
            var cutoffTime = now - staleWindow;

            _logger.LogDebug("Checking for stale vehicles (cutoff: {CutoffTime})", cutoffTime);

            // Get all online vehicles that have location tracking
            var onlineVehicles = await vehicleRepository.GetOnlineVehiclesAsync().ConfigureAwait(false);

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

                    await eventDispatcher.DispatchAsync(staleEvent).ConfigureAwait(false);
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
    private async Task DetectRecoveredVehiclesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var vehicleRepository = services.GetRequiredService<VehicleRepository>();
        var locationRepository = services.GetRequiredService<LocationRepository>();
        var eventDispatcher = services.GetRequiredService<IDomainEventDispatcher>();

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
                    var latestLocation = await locationRepository.GetLatestLocationByVehicleAsync(vehicleId).ConfigureAwait(false);

                    if (latestLocation != null)
                    {
                        var lastUpdateTime = latestLocation.RecordedAt;
                        var timeInStaleState = now - _lastActiveTimes[vehicleId];

                        // Check if vehicle has sent a new location update (recovered)
                        if (now - lastUpdateTime < TimeSpan.FromMinutes(RecoveryWindowMinutes)) // Recent update means recovered
                        {
                            var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
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

                                await eventDispatcher.DispatchAsync(activeEvent).ConfigureAwait(false);
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