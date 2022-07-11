#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Globalization;
using Microsoft.Extensions.Logging;
using SignalRMapRealtime.Domain.Enums;
using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Extension methods for <see cref="TrackingService"/> that provide additional tracking functionality.
/// </summary>
public static class TrackingServiceExtensions
{
    /// <summary>
    /// Starts a new tracking session with automatic session naming based on current time.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <param name="routeId">Optional route identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created session identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if vehicleId is not positive.</exception>
    public static async Task<int> StartTrackingSessionAsync(
        this TrackingService service,
        int vehicleId,
        int? routeId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(vehicleId);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        return await service.StartTrackingSessionAsync(
            vehicleId,
            sessionName: $"Session-{timestamp}",
            routeId,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the active session for a vehicle with strongly-typed result.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active session or null if none exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if vehicleId is not positive.</exception>
    public static async Task<TrackingSession?> GetActiveSessionAsync(
        this TrackingService service,
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(vehicleId);

        var result = await service.GetActiveSessionAsync(vehicleId, cancellationToken).ConfigureAwait(false);
        return result as TrackingSession;
    }

    /// <summary>
    /// Gets session history for a vehicle with strongly-typed results.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="vehicleId">The vehicle identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of tracking sessions.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if vehicleId is not positive.</exception>
    public static async Task<IReadOnlyList<TrackingSession>> GetSessionHistoryAsync(
        this TrackingService service,
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(vehicleId);

        var result = await service.GetSessionHistoryAsync(vehicleId, cancellationToken).ConfigureAwait(false);
        return result.Cast<TrackingSession>().ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets sessions filtered by status with strongly-typed results.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="status">The session status to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of tracking sessions with the specified status.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    public static async Task<IReadOnlyList<TrackingSession>> GetSessionsByStatusAsync(
        this TrackingService service,
        SessionStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetSessionsByStatusAsync(status, cancellationToken).ConfigureAwait(false);
        return result.Cast<TrackingSession>().ToList().AsReadOnly();
    }

    /// <summary>
    /// Safely pauses a session if it exists and is active.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if session was paused, false if session doesn't exist or isn't active.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sessionId is not positive.</exception>
    public static async Task<bool> TryPauseSessionAsync(
        this TrackingService service,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);

        return await service.PauseSessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Safely resumes a session if it exists and is paused.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if session was resumed, false if session doesn't exist or isn't paused.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sessionId is not positive.</exception>
    public static async Task<bool> TryResumeSessionAsync(
        this TrackingService service,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);

        return await service.ResumeSessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Safely completes a session if it exists and is not already completed.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if session was completed, false if session doesn't exist or is already completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sessionId is not positive.</exception>
    public static async Task<bool> TryCompleteSessionAsync(
        this TrackingService service,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);

        return await service.CompleteSessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Safely cancels a session if it exists.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="reason">Optional cancellation reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if session was cancelled, false if session doesn't exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sessionId is not positive.</exception>
    public static async Task<bool> TryCancelSessionAsync(
        this TrackingService service,
        int sessionId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);

        return await service.CancelSessionAsync(sessionId, reason ?? string.Empty, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets session details with strongly-typed result.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session details or null if session doesn't exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sessionId is not positive.</exception>
    public static async Task<TrackingSession?> GetSessionDetailsAsync(
        this TrackingService service,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);

        var result = await service.GetSessionDetailsAsync(sessionId, cancellationToken).ConfigureAwait(false);
        return result as TrackingSession;
    }

    /// <summary>
    /// Gets recently expired sessions with strongly-typed results.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of expired tracking sessions.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    public static async Task<IReadOnlyList<TrackingSession>> GetExpiredSessionsAsync(
        this TrackingService service,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetExpiredSessionsAsync(cancellationToken).ConfigureAwait(false);
        return result.Cast<TrackingSession>().ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the distance traveled in a session, returning 0 if session doesn't exist.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total distance in kilometers, or 0 if session doesn't exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sessionId is not positive.</exception>
    public static async Task<double> GetSessionDistanceAsync(
        this TrackingService service,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);

        return await service.GetSessionDistanceAsync(sessionId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the average speed for a session, returning 0 if session doesn't exist.
    /// </summary>
    /// <param name="service">The tracking service instance.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The average speed in km/h, or 0 if session doesn't exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sessionId is not positive.</exception>
    public static async Task<double> GetSessionAverageSpeedAsync(
        this TrackingService service,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionId);

        return await service.GetSessionAverageSpeedAsync(sessionId, cancellationToken).ConfigureAwait(false);
    }
}