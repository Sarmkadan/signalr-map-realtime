using System;
using System.Collections.Generic;

namespace SignalRMapRealtime.Domain.Models
{
	/// <summary>
	/// Provides extension methods for <see cref="TrackingSession"/> to simplify common operations and calculations.
	/// </summary>
	public static class TrackingSessionExtensions
	{
		/// <summary>
		/// Calculates the total duration of the tracking session
		/// </summary>
		/// <param name="session">The tracking session to calculate duration for.</param>
		/// <returns>The total duration as a <see cref="TimeSpan"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
		public static TimeSpan GetDuration(this TrackingSession session)
		{
			ArgumentNullException.ThrowIfNull(session);

			if (session.EndTime.HasValue)
				return session.EndTime.Value - session.StartTime;

			return DateTime.UtcNow - session.StartTime;
		}

		/// <summary>
		/// Checks if the session is currently active (has not been ended)
		/// </summary>
		/// <param name="session">The tracking session to check.</param>
		/// <returns><see langword="true"/> if the session is active; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
		public static bool IsActive(this TrackingSession session)
		{
			ArgumentNullException.ThrowIfNull(session);
			return !session.EndTime.HasValue;
		}

		/// <summary>
		/// Returns true if the session has any recorded locations
		/// </summary>
		/// <param name="session">The tracking session to check.</param>
		/// <returns><see langword="true"/> if the session has locations; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
		public static bool HasLocations(this TrackingSession session)
		{
			ArgumentNullException.ThrowIfNull(session);
			return session.Locations?.Count > 0;
		}

		/// <summary>
		/// Formats the total idle time in a human-readable format
		/// </summary>
		/// <param name="session">The tracking session to format idle time for.</param>
		/// <returns>A human-readable string representation of the idle time.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
		public static string GetFormattedIdleTime(this TrackingSession session)
		{
			ArgumentNullException.ThrowIfNull(session);

			if (session.TotalIdleSeconds <= 0)
				return "0 seconds";

			var hours = session.TotalIdleSeconds / 3600;
			var minutes = (session.TotalIdleSeconds % 3600) / 60;
			var seconds = session.TotalIdleSeconds % 60;

			return $"{hours}h {minutes}m {seconds}s";
		}
	}
}
