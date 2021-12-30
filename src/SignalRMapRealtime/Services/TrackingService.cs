// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

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

    /// <summary>
    /// Initializes a new instance of TrackingService.
    /// </summary>
    public TrackingService(TrackingSessionRepository sessionRepository, VehicleRepository vehicleRepository, RouteRepository routeRepository)
    {
        ArgumentNullException.ThrowIfNull(sessionRepository);
        ArgumentNullException.ThrowIfNull(vehicleRepository);
        ArgumentNullException.ThrowIfNull(routeRepository);
        _sessionRepository = sessionRepository;
        _vehicleRepository = vehicleRepository;
        _routeRepository = routeRepository;
    }

    /// <summary>
    /// Starts a new tracking session for a vehicle.
    /// </summary>
    public async Task<int> StartTrackingSessionAsync(int vehicleId, string sessionName = "", int? routeId = null, CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle == null)
            throw new VehicleNotFoundException(vehicleId);

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

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return session.Id;
    }

    /// <summary>
    /// Pauses an active tracking session.
    /// </summary>
    public async Task<bool> PauseSessionAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.Status != SessionStatus.Active)
            return false;

        session.PauseSession();
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Resumes a paused tracking session.
    /// </summary>
    public async Task<bool> ResumeSessionAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.Status != SessionStatus.Paused)
            return false;

        session.ResumeSession();
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Completes a tracking session with final statistics.
    /// </summary>
    public async Task<bool> CompleteSessionAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetSessionWithDetailsAsync(sessionId);
        if (session == null || session.Status == SessionStatus.Completed)
            return false;

        session.CompleteSession();
        var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId, cancellationToken);
        if (vehicle != null)
        {
            vehicle.UpdateStatus(VehicleStatus.AtDepot);
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
            await _vehicleRepository.SaveChangesAsync(cancellationToken);
        }

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Cancels a tracking session abnormally.
    /// </summary>
    public async Task<bool> CancelSessionAsync(int sessionId, string reason = "", CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
            return false;

        session.CancelSession(reason);
        var vehicle = await _vehicleRepository.GetByIdAsync(session.VehicleId, cancellationToken);
        if (vehicle != null)
        {
            vehicle.UpdateStatus(VehicleStatus.Idle);
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
            await _vehicleRepository.SaveChangesAsync(cancellationToken);
        }

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets the active session for a vehicle.
    /// </summary>
    public async Task<object?> GetActiveSessionAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetActiveSessionByVehicleAsync(vehicleId);
        return session;
    }

    /// <summary>
    /// Gets all sessions for a vehicle.
    /// </summary>
    public async Task<IEnumerable<object>> GetSessionHistoryAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetSessionsByVehicleAsync(vehicleId);
        return sessions.Cast<object>();
    }

    /// <summary>
    /// Gets sessions by status.
    /// </summary>
    public async Task<IEnumerable<object>> GetSessionsByStatusAsync(SessionStatus status, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetSessionsByStatusAsync(status);
        return sessions.Cast<object>();
    }

    /// <summary>
    /// Gets recently expired sessions.
    /// </summary>
    public async Task<IEnumerable<object>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetExpiredSessionsAsync();
        return sessions.Cast<object>();
    }

    /// <summary>
    /// Checks if a session is currently active.
    /// </summary>
    public async Task<bool> IsSessionActiveAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        return session?.Status == SessionStatus.Active;
    }

    /// <summary>
    /// Gets session details with statistics.
    /// </summary>
    public async Task<object?> GetSessionDetailsAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _sessionRepository.GetSessionWithDetailsAsync(sessionId);
    }

    /// <summary>
    /// Calculates total distance traveled in a session.
    /// </summary>
    public async Task<double> GetSessionDistanceAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetSessionWithDetailsAsync(sessionId);
        return session?.TotalDistance ?? 0;
    }

    /// <summary>
    /// Gets average speed for a session.
    /// </summary>
    public async Task<double> GetSessionAverageSpeedAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetSessionWithDetailsAsync(sessionId);
        return session?.AverageSpeed ?? 0;
    }
}
