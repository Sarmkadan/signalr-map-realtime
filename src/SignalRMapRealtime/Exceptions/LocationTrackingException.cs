#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Base exception for location tracking system errors.
/// </summary>
public class LocationTrackingException : SignalrMapRealtimeException
{
    /// <summary>
    /// Initializes a new instance of LocationTrackingException.
    /// </summary>
    public LocationTrackingException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of LocationTrackingException with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    public LocationTrackingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of LocationTrackingException with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    public LocationTrackingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when a vehicle is not found in the system.
/// </summary>
public class VehicleNotFoundException : LocationTrackingException
{
    /// <summary>Gets the vehicle ID that was not found.</summary>
    public int VehicleId { get; }

    /// <summary>
    /// Initializes a new instance with vehicle ID.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID that was not found.</param>
    public VehicleNotFoundException(int vehicleId)
    : base($"Vehicle with ID {vehicleId} was not found.")
    {
        VehicleId = vehicleId;
    }

    /// <summary>
    /// Initializes a new instance with vehicle ID and custom message.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID that was not found.</param>
    /// <param name="message">Custom error message.</param>
    public VehicleNotFoundException(int vehicleId, string message)
    : base(message)
    {
        VehicleId = vehicleId;
    }
}

/// <summary>
/// Thrown when location data is invalid.
/// </summary>
public class InvalidLocationException : LocationTrackingException
{
    /// <summary>Gets the latitude value that was invalid.</summary>
    public double? Latitude { get; }

    /// <summary>Gets the longitude value that was invalid.</summary>
    public double? Longitude { get; }

    /// <summary>
    /// Initializes a new instance with coordinates.
    /// </summary>
    /// <param name="latitude">The invalid latitude value.</param>
    /// <param name="longitude">The invalid longitude value.</param>
    public InvalidLocationException(double latitude, double longitude)
    : base($"Invalid location coordinates: Latitude={latitude}, Longitude={longitude}")
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Initializes a new instance with custom message.
    /// </summary>
    /// <param name="message">Custom error message.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    public InvalidLocationException(string message)
    : base(message)
    {
    }
}

/// <summary>
/// Thrown when an asset is not found.
/// </summary>
public class AssetNotFoundException : LocationTrackingException
{
    /// <summary>Gets the asset ID that was not found.</summary>
    public int AssetId { get; }

    /// <summary>
    /// Initializes a new instance with asset ID.
    /// </summary>
    /// <param name="assetId">The asset ID that was not found.</param>
    public AssetNotFoundException(int assetId)
    : base($"Asset with ID {assetId} was not found.")
    {
        AssetId = assetId;
    }
}

/// <summary>
/// Thrown when a tracking session cannot be found.
/// </summary>
public class TrackingSessionNotFoundException : LocationTrackingException
{
    /// <summary>Gets the session ID that was not found.</summary>
    public int SessionId { get; }

    /// <summary>
    /// Initializes a new instance with session ID.
    /// </summary>
    /// <param name="sessionId">The session ID that was not found.</param>
    public TrackingSessionNotFoundException(int sessionId)
    : base($"Tracking session with ID {sessionId} was not found.")
    {
        SessionId = sessionId;
    }
}
