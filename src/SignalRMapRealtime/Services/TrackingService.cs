#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.Logging;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Exceptions;

/// <summary>
/// Service for managing tracking sessions and vehicle tracking lifecycle.
/// </summary>
public class TrackingService : ITrackingService
{
    private readonly TrackingSessionRepository _sessionRepository;
    private readonly VehicleRepository _vehicleRepository;
    private readonly RouteRepository _routeRepository;
    private readonly ILogger<TrackingService> _logger;

    /// <summary>
    /// Initializes a new instance of TrackingService.
    /// </summary>
    public TrackingService(
        TrackingSessionRepository sessionRepository,
        VehicleRepository vehicleRepository,
        RouteRepository routeRepository,
        ILogger<TrackingService> logger)
    {
        ArgumentNullException.ThrowIfNull(sessionRepository);
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(routeRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _sessionRepository = sessionRepository;
        _vehicleRepository = vehicleRepository;
        _routeRepository = routeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new tracking session for a vehicle.
    /// </summary>
    public async Task<int> StartTrackingSessionAsync(int vehicleId, string sessionName = "", int? routeId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting new tracking session for vehicle {VehicleId}", vehicleId);

        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle {VehicleId} not found when attempting to start tracking session", vehicleId);
            throw new VehicleNotFoundException(vehicleId);
        }

        var session = new TrackingSession
        {
            SessionName = string.IsNullOrWhiteSpace(sessionName) ? $"Session-{DateTime.UtcNow:yyyyMMddHHmmss}" : sessionName,
            VehicleId = vehicleId,
            RouteId = routeId,
            Status = SessionStatus.Active,
            StartTime = DateTime.UtcNow,
            TotalDistance = 0,
            AverageSpeed = 0,
            MaxSpeed = 0,
            TotalIdleSeconds = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        session.StartSession();
        vehicle.UpdateStatus(VehicleStatus.InTransit);

        await _sessionRepository.AddAsync(session, cancellationToken).ConfigureAwait(false);
        await _sessionRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken).ConfigureAwait(false);
        await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Started tracking session {SessionId} for vehicle {VehicleId}", session.Id, vehicleId);
        return session.Id;
    }

    /// <summary>
    /// Pauses an active tracking session.
    /// </summary>
    public async Task<bool> PauseSessionAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pausing tracking session {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.Status != SessionStatus.Active)
        {
            _logger.LogWarning("Cannot pause session {SessionId}: session not found or not active", sessionId);
            return false;
        }

        session.PauseSession();
        await _sessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
        await _sessionRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Paused tracking session {SessionId}", sessionId);
        return true;
    }

    /// <summary>
    /// Resumes a paused tracking session.
    /// </summary>
    public async Task<bool> ResumeSessionAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resuming tracking session {SessionId}", sessionId);

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (session is null || session.Status != SessionStatus.Paused)
        {
            _logger.LogWarning("Cannot resume session {SessionId}: session not found or not paused", sessionId);
            return false;
        }

        session.ResumeSession();
        await _sessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
        await _sessionRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Resumed tracking session {SessionId}", sessionId);
        return true;
    }

    /// <summary>
    /// Completes a tracking session with final statistics.
    /// </summary>
    public async Task<bool> CompleteSessionAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing tracking session {SessionId}", sessionId);

        var session = await _sessionRepository.GetSessionWithDetailsAsync(sessionId).ConfigureAwait(false);
        if (session is null || session.Status == SessionStatus.Completed)
        {
            _logger.LogWarning("Cannot complete session {SessionId}: session not found or already completed", sessionId);
            return false;
        }

        session.CompleteSession();
        var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is not null)
        {
            vehicle.UpdateStatus(VehicleStatus.AtDepot);
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken).ConfigureAwait(false);
            await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        await _sessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
        await _sessionRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Completed tracking session {SessionId}", sessionId);
        return true;
    }

    /// <summary>
    /// Cancels a tracking session abnormally.
    /// </summary>
    public async Task<bool> CancelSessionAsync(int sessionId, string reason = "", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling tracking session {SessionId}. Reason: {Reason}", sessionId, string.IsNullOrEmpty(reason) ? "Not specified" : reason);

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            _logger.LogWarning("Cannot cancel session {SessionId}: session not found", sessionId);
            return false;
        }

        session.CancelSession(reason);
        var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId, cancellationToken).ConfigureAwait(false);
        if (vehicle is not null)
        {
            vehicle.UpdateStatus(VehicleStatus.Idle);
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken).ConfigureAwait(false);
            await _vehicleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        await _sessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
        await _sessionRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Cancelled tracking session {SessionId}", sessionId);
        return true;
    }

    /// <summary>
    /// Gets the active session for a vehicle.
    /// </summary>
    public async Task<object?> GetActiveSessionAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving active session for vehicle {VehicleId}", vehicleId);
        var session = await _sessionRepository.GetActiveSessionByVehicleAsync(vehicleId).ConfigureAwait(false);
        return session;
    }

    /// <summary>
    /// Gets all sessions for a vehicle.
    /// </summary>
    public async Task<IEnumerable<object>> GetSessionHistoryAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving session history for vehicle {VehicleId}", vehicleId);
        var sessions = await _sessionRepository.GetSessionsByVehicleAsync(vehicleId).ConfigureAwait(false);
        return sessions.Cast<object>();
    }

    /// <summary>
    /// Gets sessions by status.
    /// </summary>
    public async Task<IEnumerable<object>> GetSessionsByStatusAsync(SessionStatus status, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving sessions with status {Status}", status);
        var sessions = await _sessionRepository.GetSessionsByStatusAsync(status).ConfigureAwait(false);
        return sessions.Cast<object>();
    }

    /// <summary>
    /// Gets recently expired sessions.
    /// </summary>
    public async Task<IEnumerable<object>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving expired sessions");
        var sessions = await _sessionRepository.GetExpiredSessionsAsync().ConfigureAwait(false);
        return sessions.Cast<object>();
    }

    /// <summary>
    /// Checks if a session is currently active.
    /// </summary>
    public async Task<bool> IsSessionActiveAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if session {SessionId} is active", sessionId);
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken).ConfigureAwait(false);
        return session?.Status == SessionStatus.Active;
    }

    /// <summary>
    /// Gets session details with statistics.
    /// </summary>
    public async Task<object?> GetSessionDetailsAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving details for session {SessionId}", sessionId);
        return await _sessionRepository.GetSessionWithDetailsAsync(sessionId).ConfigureAwait(false);
    }

    /// <summary>
    /// Calculates total distance traveled in a session.
    /// </summary>
    public async Task<double> GetSessionDistanceAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calculating distance for session {SessionId}", sessionId);
        var session = await _sessionRepository.GetSessionWithDetailsAsync(sessionId).ConfigureAwait(false);
        return session?.TotalDistance ?? 0;
    }

    /// <summary>
    /// Gets average speed for a session.
    /// </summary>
    public async Task<double> GetSessionAverageSpeedAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Calculating average speed for session {SessionId}", sessionId);
        var session = await _sessionRepository.GetSessionWithDetailsAsync(sessionId).ConfigureAwait(false);
        return session?.AverageSpeed ?? 0;
    }
}
