using System;
using System.Collections.Generic;

namespace SignalRMapRealtime.Domain.Models
{
    public static class TrackingSessionExtensions
    {
        /// <summary>
        /// Calculates the total duration of the tracking session
        /// </summary>
        public static TimeSpan GetDuration(this TrackingSession session)
        {
            if (session.EndTime.HasValue)
                return session.EndTime.Value - session.StartTime;
            
            return DateTime.UtcNow - session.StartTime;
        }

        /// <summary>
        /// Checks if the session is currently active (has not been ended)
        /// </summary>
        public static bool IsActive(this TrackingSession session)
        {
            return !session.EndTime.HasValue;
        }

        /// <summary>
        /// Returns true if the session has any recorded locations
        /// </summary>
        public static bool HasLocations(this TrackingSession session)
        {
            return session.Locations != null && session.Locations.Count > 0;
        }

        /// <summary>
        /// Formats the total idle time in a human-readable format
        /// </summary>
        public static string GetFormattedIdleTime(this TrackingSession session)
        {
            if (session.TotalIdleSeconds <= 0)
                return "0 seconds";
                
            var hours = session.TotalIdleSeconds / 3600;
            var minutes = (session.TotalIdleSeconds % 3600) / 60;
            var seconds = session.TotalIdleSeconds % 60;
            
            return $"{hours}h {minutes}m {seconds}s";
        }
    }
}
