// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Service interface for managing tracking sessions.
/// </summary>
public interface ITrackingService
{
    /// <summary>
    /// Starts a new tracking session for a vehicle.
    /// </summary>
    Task<int> StartTrackingSessionAsync(int vehicleId, string sessionName = "", int? routeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses an active tracking session.
    /// </summary>
    Task<bool> PauseSessionAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused tracking session.
    /// </summary>
    Task<bool> ResumeSessionAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a tracking session with final statistics.
    /// </summary>
    Task<bool> CompleteSessionAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a tracking session abnormally.
    /// </summary>
    Task<bool> CancelSessionAsync(int sessionId, string reason = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active session for a vehicle.
    /// </summary>
    Task<object?> GetActiveSessionAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sessions for a vehicle.
    /// </summary>
    Task<IEnumerable<object>> GetSessionHistoryAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sessions by status.
    /// </summary>
    Task<IEnumerable<object>> GetSessionsByStatusAsync(SessionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently expired sessions that need cleanup.
    /// </summary>
    Task<IEnumerable<object>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a session is currently active.
    /// </summary>
    Task<bool> IsSessionActiveAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session details with statistics.
    /// </summary>
    Task<object?> GetSessionDetailsAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates total distance traveled in a session.
    /// </summary>
    Task<double> GetSessionDistanceAsync(int sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets average speed for a session.
    /// </summary>
    Task<double> GetSessionAverageSpeedAsync(int sessionId, CancellationToken cancellationToken = default);
}
