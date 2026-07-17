namespace SignalRMapRealtime.DTOs;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Provides extension methods for <see cref="VehicleDto"/> to enhance vehicle-related operations and formatting.
/// </summary>
public static class VehicleDtoExtensions
{
    /// <summary>
    /// Determines if a vehicle is currently online.
    /// </summary>
    /// <param name="vehicle">The vehicle to check. Cannot be <see langword="null"/>.</param>
    /// <returns>true if the vehicle is online; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicle"/> is <see langword="null"/>.</exception>
    public static bool IsOnline(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return vehicle.IsOnline;
    }

    /// <summary>
    /// Determines if a vehicle is in a specific operational status.
    /// </summary>
    /// <param name="vehicle">The vehicle to check. Cannot be <see langword="null"/>.</param>
    /// <param name="status">The status to compare against.</param>
    /// <returns>true if the vehicle's status matches the specified status; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicle"/> is <see langword="null"/>.</exception>
    public static bool IsInStatus(this VehicleDto vehicle, VehicleStatus status)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return vehicle.Status == status;
    }

    /// <summary>
    /// Gets a human-readable string representation of the vehicle's status.
    /// </summary>
    /// <param name="vehicle">The vehicle to get the status for. Cannot be <see langword="null"/>.</param>
    /// <returns>A string representation of the vehicle's status.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicle"/> is <see langword="null"/>.</exception>
    public static string GetStatusString(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return vehicle.Status.ToString();
    }

    /// <summary>
    /// Gets a formatted string representation of the vehicle's location details.
    /// </summary>
    /// <param name="vehicle">The vehicle to get the location for. Cannot be <see langword="null"/>.</param>
    /// <returns>A string representation of the vehicle's location, or <see langword="null"/> if no location is available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicle"/> is <see langword="null"/>.</exception>
    public static string? GetLocationDetails(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        return vehicle.LastLocation is null
            ? null
            : $"Lat: {vehicle.LastLocation.Latitude:F6}, Lng: {vehicle.LastLocation.Longitude:F6}";
    }

    /// <summary>
    /// Gets a formatted string representation of the vehicle's basic information.
    /// </summary>
    /// <param name="vehicle">The vehicle to format. Cannot be <see langword="null"/>.</param>
    /// <returns>A formatted string containing vehicle identification information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicle"/> is <see langword="null"/>.</exception>
    public static string GetInfoString(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return $"[{vehicle.Id}] {vehicle.RegistrationNumber} - {vehicle.Name} ({vehicle.Status})" ;
    }

    /// <summary>
    /// Determines if a vehicle requires attention based on its status or operational conditions.
    /// </summary>
    /// <param name="vehicle">The vehicle to check. Cannot be <see langword="null"/>.</param>
    /// <returns>true if the vehicle requires attention; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="vehicle"/> is <see langword="null"/>.</exception>
    public static bool RequiresAttention(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        return vehicle.IsOnline is false
            || vehicle.Status is VehicleStatus.Maintenance
            || vehicle.Status is VehicleStatus.Deactivated;
    }
}