using System;

namespace SignalRMapRealtime.Domain.Models
{
    /// <summary>
    /// Extension methods for the <see cref="Waypoint"/> class.
    /// </summary>
    public static class WaypointExtensions
    {
        /// <summary>
        /// Calculates the distance in kilometers between two waypoints using the Haversine formula.
        /// </summary>
        /// <param name="source">The source waypoint.</param>
        /// <param name="destination">The destination waypoint.</param>
        /// <returns>The distance in kilometers.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is null.</exception>
        public static double CalculateDistanceTo(this Waypoint source, Waypoint destination)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(destination);

            var sourceLat = source.Latitude * (Math.PI / 180.0);
            var destLat = destination.Latitude * (Math.PI / 180.0);
            var dLat = (destination.Latitude - source.Latitude) * (Math.PI / 180.0);
            var dLon = (destination.Longitude - source.Longitude) * (Math.PI / 180.0);

            var a = Math.Pow(Math.Sin(dLat / 2.0), 2.0) +
                            Math.Cos(sourceLat) * Math.Cos(destLat) *
                            Math.Pow(Math.Sin(dLon / 2.0), 2.0);

            var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            
            // Earth radius in kilometers
            const double earthRadiusKm = 6371.0;
            return earthRadiusKm * c;
        }

        /// <summary>
        /// Gets a displayable name for the waypoint, preferring Name, then Address, then Coordinates.
        /// </summary>
        /// <param name="waypoint">The waypoint.</param>
        /// <returns>A string representation suitable for display.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="waypoint"/> is null.</exception>
        public static string GetDisplayName(this Waypoint waypoint)
        {
            ArgumentNullException.ThrowIfNull(waypoint);

            return !string.IsNullOrEmpty(waypoint.Name) ? waypoint.Name
                 : !string.IsNullOrEmpty(waypoint.Address) ? waypoint.Address
                 : $"{waypoint.Latitude:F4}, {waypoint.Longitude:F4}";
        }

        /// <summary>
        /// Determines if the waypoint is currently in progress (arrived but not yet departed).
        /// </summary>
        /// <param name="waypoint">The waypoint.</param>
        /// <returns>True if the waypoint is in progress; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="waypoint"/> is null.</exception>
        public static bool IsInProgress(this Waypoint waypoint)
        {
            ArgumentNullException.ThrowIfNull(waypoint);

            // If it's already marked as completed, it is not in progress.
            if (waypoint.IsCompleted)
            {
                return false;
            }

            // If we have an arrival time but no departure time, we are currently at the waypoint.
            return waypoint.ActualArrivalTime.HasValue && !waypoint.ActualDepartureTime.HasValue;
        }
    }
}
