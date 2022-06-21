#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using SignalRMapRealtime.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="RoutePlaybackService"/> instances.
/// </summary>
/// <remarks>
/// Validates the internal state of <see cref="RoutePlaybackService"/> and its
/// <see cref="PlaybackState"/> to ensure they represent a valid, usable playback session.
/// </remarks>
internal static class RoutePlaybackServiceValidation
{
    /// <summary>
    /// Validates a <see cref="RoutePlaybackService"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RoutePlaybackService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Use reflection to access internal PlaybackState properties
        var sessionsField = typeof(RoutePlaybackService).GetField("_sessions", BindingFlags.NonPublic | BindingFlags.Instance);
        if (sessionsField is null)
        {
            errors.Add("Internal _sessions field not found in RoutePlaybackService.");
            return errors.AsReadOnly();
        }

        var sessions = sessionsField.GetValue(value) as ConcurrentDictionary<Guid, object>;
        if (sessions is null || sessions.Count == 0)
        {
            errors.Add("No active playback sessions found.");
            return errors.AsReadOnly();
        }

        // Validate each session's state
        foreach (var kvp in sessions)
        {
            var state = kvp.Value;
            ValidatePlaybackState(state, errors);
        }

        return errors.AsReadOnly();
    }

    private static void ValidatePlaybackState(object state, List<string> errors)
    {
        if (state is null)
        {
            errors.Add("PlaybackState instance is null.");
            return;
        }

        // Get all properties via reflection
        var playbackIdProp = state.GetType().GetProperty("PlaybackId", BindingFlags.Public | BindingFlags.Instance);
        var trackingSessionIdProp = state.GetType().GetProperty("TrackingSessionId", BindingFlags.Public | BindingFlags.Instance);
        var locationsProp = state.GetType().GetProperty("Locations", BindingFlags.Public | BindingFlags.Instance);
        var cumulativeDistancesProp = state.GetType().GetProperty("CumulativeDistances", BindingFlags.Public | BindingFlags.Instance);
        var startedAtProp = state.GetType().GetProperty("StartedAt", BindingFlags.Public | BindingFlags.Instance);
        var loopProp = state.GetType().GetProperty("Loop", BindingFlags.Public | BindingFlags.Instance);
        var ctsProp = state.GetType().GetProperty("Cts", BindingFlags.Public | BindingFlags.Instance);

        // Validate PlaybackId
        var playbackId = playbackIdProp?.GetValue(state) as Guid?;
        if (!playbackId.HasValue || playbackId.Value == Guid.Empty)
        {
            errors.Add("PlaybackId must not be empty (Guid.Empty).");
        }

        // Validate TrackingSessionId
        var trackingSessionId = trackingSessionIdProp?.GetValue(state) as int?;
        if (!trackingSessionId.HasValue || trackingSessionId.Value <= 0)
        {
            errors.Add("TrackingSessionId must be a positive integer.");
        }

        // Validate Locations
        var locations = locationsProp?.GetValue(state) as IReadOnlyList<Location>;
        if (locations is null)
        {
            errors.Add("Locations collection must not be null.");
        }
        else if (locations.Count == 0)
        {
            errors.Add("Locations collection must contain at least one location.");
        }
        else
        {
            // Validate each location
            for (int i = 0; i < locations.Count; i++)
            {
                var location = locations[i];
                if (location is null)
                {
                    errors.Add($"Locations[{i}] must not be null.");
                    continue;
                }

                if (!location.IsValidCoordinate())
                {
                    errors.Add($"Locations[{i}]: Invalid geographic coordinates (Latitude={location.Latitude}, Longitude={location.Longitude}).");
                }

                if (location.RecordedAt == default)
                {
                    errors.Add($"Locations[{i}]: RecordedAt timestamp must not be default(DateTime).");
                }

                if (location.RecordedAt.Kind != DateTimeKind.Utc)
                {
                    errors.Add($"Locations[{i}]: RecordedAt must be in UTC (DateTimeKind.Utc).");
                }
            }

            // Validate locations are in chronological order
            for (int i = 1; i < locations.Count; i++)
            {
                if (locations[i].RecordedAt < locations[i - 1].RecordedAt)
                {
                    errors.Add("Locations must be in chronological order (each location's RecordedAt must be >= previous).");
                    break;
                }
            }
        }

        // Validate CumulativeDistances
        var cumulativeDistances = cumulativeDistancesProp?.GetValue(state) as double[];
        if (cumulativeDistances is null)
        {
            errors.Add("CumulativeDistances array must not be null.");
        }
        else if (locations?.Count > 0 && cumulativeDistances.Length != locations.Count)
        {
            errors.Add("CumulativeDistances array length must match Locations count.");
        }
        else if (cumulativeDistances is not null)
        {
            // Validate distances are non-negative and monotonically increasing
            for (int i = 0; i < cumulativeDistances.Length; i++)
            {
                if (cumulativeDistances[i] < 0)
                {
                    errors.Add($"CumulativeDistances[{i}] must be non-negative, but was {cumulativeDistances[i]}.");
                }

                if (i > 0 && cumulativeDistances[i] < cumulativeDistances[i - 1])
                {
                    errors.Add($"CumulativeDistances[{i}] must be >= CumulativeDistances[{i - 1}], but was {cumulativeDistances[i]} < {cumulativeDistances[i - 1]}.");
                }
            }
        }

        // Validate StartedAt
        var startedAt = startedAtProp?.GetValue(state) as DateTime?;
        if (!startedAt.HasValue || startedAt.Value == default)
        {
            errors.Add("StartedAt timestamp must not be default(DateTime).");
        }
        else if (startedAt.Value.Kind != DateTimeKind.Utc)
        {
            errors.Add("StartedAt must be in UTC (DateTimeKind.Utc).");
        }
        else if (locations?.Count > 0)
        {
            var firstLocationTime = locations[0].RecordedAt;
            var lastLocationTime = locations[^1].RecordedAt;
            if (startedAt.Value > lastLocationTime)
            {
                errors.Add("StartedAt must not be after the last location's RecordedAt timestamp.");
            }
            else if (startedAt.Value < firstLocationTime)
            {
                errors.Add("StartedAt should not be before the first location's RecordedAt timestamp.");
            }
        }

        // Validate Cts
        var cts = ctsProp?.GetValue(state) as CancellationTokenSource;
        if (cts is null)
        {
            errors.Add("Cts (CancellationTokenSource) must not be null.");
        }
        else if (cts.IsCancellationRequested)
        {
            errors.Add("Cts must not already be cancelled.");
        }
    }

    /// <summary>
    /// Determines whether a <see cref="RoutePlaybackService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this RoutePlaybackService value)
    {
        try
        {
            return Validate(value).Count == 0;
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that a <see cref="RoutePlaybackService"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed error message if it is not.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the service is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this RoutePlaybackService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"RoutePlaybackService validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", errors)
            }");
    }
}