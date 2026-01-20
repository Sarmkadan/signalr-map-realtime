// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Base exception for location tracking system errors.
/// </summary>
public class LocationTrackingException : Exception
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
    public LocationTrackingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of LocationTrackingException with a message and inner exception.
    /// </summary>
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
    public VehicleNotFoundException(int vehicleId)
        : base($"Vehicle with ID {vehicleId} was not found.")
    {
        VehicleId = vehicleId;
    }

    /// <summary>
    /// Initializes a new instance with vehicle ID and custom message.
    /// </summary>
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
    public InvalidLocationException(double latitude, double longitude)
        : base($"Invalid location coordinates: Latitude={latitude}, Longitude={longitude}")
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Initializes a new instance with custom message.
    /// </summary>
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
    public TrackingSessionNotFoundException(int sessionId)
        : base($"Tracking session with ID {sessionId} was not found.")
    {
        SessionId = sessionId;
    }
}
