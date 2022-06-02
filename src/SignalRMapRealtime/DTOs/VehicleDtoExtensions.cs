namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Provides extension methods for <see cref="VehicleDto"/>.
/// </summary>
public static class VehicleDtoExtensions
{
    /// <summary>
    /// Determines if a vehicle is currently online.
    /// </summary>
    /// <param name="vehicle">The vehicle to check.</param>
    /// <returns>true if the vehicle is online; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="vehicle"/> is null.</exception>
    public static bool IsOnline(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return vehicle.IsOnline;
    }

    /// <summary>
    /// Gets a readable vehicle status string.
    /// </summary>
    /// <param name="vehicle">The vehicle to get the status for.</param>
    /// <returns>A string representation of the vehicle's status.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="vehicle"/> is null.</exception>
    public static string GetStatusString(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return vehicle.Status.ToString();
    }

    /// <summary>
    /// Gets a vehicle's location details.
    /// </summary>
    /// <param name="vehicle">The vehicle to get the location for.</param>
    /// <returns>A string representation of the vehicle's location, or null if no location is available.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="vehicle"/> is null.</exception>
    public static string? GetLocationDetails(this VehicleDto vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        if (vehicle.LastLocation is null) return null;
        return $"Latitude: {vehicle.LastLocation.Latitude}, Longitude: {vehicle.LastLocation.Longitude}";
    }
}
