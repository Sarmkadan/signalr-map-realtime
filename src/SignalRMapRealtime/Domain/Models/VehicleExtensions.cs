using System;

namespace SignalRMapRealtime.Domain.Models
{
    /// <summary>
    /// Extension methods for the <see cref="Vehicle"/> domain model.
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Determines whether the vehicle has an assigned driver.
        /// </summary>
        /// <param name="vehicle">The vehicle instance.</param>
        /// <returns><c>true</c> if both <c>DriverId</c> and <c>Driver</c> are present; otherwise <c>false</c>.</returns>
        public static bool HasDriver(this Vehicle vehicle) =>
            vehicle.DriverId.HasValue && vehicle.Driver != null;

        /// <summary>
        /// Returns a human‑readable description of the vehicle, combining make, model, year and registration details.
        /// </summary>
        /// <param name="vehicle">The vehicle instance.</param>
        /// <returns>A formatted description string.</returns>
        public static string GetFullDescription(this Vehicle vehicle)
        {
            var make = vehicle.Make ?? "Unknown";
            var model = vehicle.Model ?? "Unknown";
            var year = vehicle.Year?.ToString() ?? vehicle.ModelYear?.ToString() ?? "N/A";
            var name = vehicle.Name ?? "Unnamed";
            var reg = vehicle.RegistrationNumber ?? "No Reg";

            return $"{make} {model} {year} - {name} ({reg})";
        }

        /// <summary>
        /// Gets the total number of tracking sessions associated with the vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle instance.</param>
        /// <returns>The count of tracking sessions, or 0 if the collection is null.</returns>
        public static int GetTrackingSessionCount(this Vehicle vehicle) =>
            vehicle.TrackingSessions?.Count ?? 0;

        /// <summary>
        /// Calculates the vehicle's age in years based on the available year information.
        /// </summary>
        /// <param name="vehicle">The vehicle instance.</param>
        /// <returns>
        /// The age in years, or <c>null</c> if neither <c>Year</c> nor <c>ModelYear</c> is set.
        /// </returns>
        public static int? GetVehicleAge(this Vehicle vehicle)
        {
            var year = vehicle.Year ?? vehicle.ModelYear;
            if (!year.HasValue)
                return null;

            var currentYear = DateTime.Now.Year;
            return currentYear - year.Value;
        }
    }
}
