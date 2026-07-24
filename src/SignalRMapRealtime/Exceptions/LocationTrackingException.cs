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
        ArgumentNullException.ThrowIfNull(message);
    }

    /// <summary>
    /// Initializes a new instance of LocationTrackingException with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
    public LocationTrackingException(string message, Exception innerException) : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(message);
    }

    /// <summary>
    /// Validates that an identifier string is not null or empty.
    /// </summary>
    /// <param name="identifier">The identifier to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="identifier"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="identifier"/> is empty.</exception>
    protected static string ValidateIdentifier(string identifier, string paramName)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        if (identifier.Length == 0)
        {
            throw new ArgumentException("Identifier cannot be empty.", paramName);
        }

        return identifier;
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
    /// <exception cref="ArgumentException">Thrown if <paramref name="vehicleId"/> is less than or equal to 0.</exception>
    public VehicleNotFoundException(int vehicleId)
        : base(GetDefaultMessage(vehicleId))
    {
        if (vehicleId <= 0)
        {
            throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(vehicleId));
        }

        VehicleId = vehicleId;
    }

    /// <summary>
    /// Initializes a new instance with vehicle ID and custom message.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID that was not found.</param>
    /// <param name="message">Custom error message.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="vehicleId"/> is less than or equal to 0.</exception>
    public VehicleNotFoundException(int vehicleId, string message)
        : base(message)
    {
        if (vehicleId <= 0)
        {
            throw new ArgumentException("Vehicle ID must be a positive integer.", nameof(vehicleId));
        }

        VehicleId = vehicleId;
    }

    /// <summary>
    /// Initializes a new instance with string vehicle ID.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID that was not found.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="vehicleId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="vehicleId"/> is empty or whitespace.</exception>
    public VehicleNotFoundException(string vehicleId)
        : base($"Vehicle with ID '{vehicleId}' was not found.")
    {
        VehicleId = int.Parse(ValidateIdentifier(vehicleId, nameof(vehicleId)));
    }

    private static string GetDefaultMessage(int vehicleId) =>
        $"Vehicle with ID {vehicleId} was not found.";
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
    /// <exception cref="ArgumentException">Thrown if <paramref name="latitude"/> is not a valid coordinate value.</exception>
    public InvalidLocationException(double latitude, double longitude)
        : base($"Invalid location coordinates: Latitude={FormatCoordinate(latitude)}, Longitude={FormatCoordinate(longitude)}")
    {
        if (latitude < -90.0 || latitude > 90.0)
        {
            throw new ArgumentException("Latitude must be between -90 and 90 degrees.", nameof(latitude));
        }

        if (longitude < -180.0 || longitude > 180.0)
        {
            throw new ArgumentException("Longitude must be between -180 and 180 degrees.", nameof(longitude));
        }

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
        ArgumentNullException.ThrowIfNull(message);
    }

    private static string FormatCoordinate(double coordinate) =>
        coordinate.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
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
    /// <exception cref="ArgumentException">Thrown if <paramref name="assetId"/> is less than or equal to 0.</exception>
    public AssetNotFoundException(int assetId)
        : base(GetDefaultMessage(assetId))
    {
        if (assetId <= 0)
        {
            throw new ArgumentException("Asset ID must be a positive integer.", nameof(assetId));
        }

        AssetId = assetId;
    }

    /// <summary>
    /// Initializes a new instance with string asset ID.
    /// </summary>
    /// <param name="assetId">The asset ID that was not found.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="assetId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="assetId"/> is empty or whitespace.</exception>
    public AssetNotFoundException(string assetId)
        : base($"Asset with ID '{assetId}' was not found.")
    {
        AssetId = int.Parse(ValidateIdentifier(assetId, nameof(assetId)));
    }

    private static string GetDefaultMessage(int assetId) =>
        $"Asset with ID {assetId} was not found.";
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
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is less than or equal to 0.</exception>
    public TrackingSessionNotFoundException(int sessionId)
        : base(GetDefaultMessage(sessionId))
    {
        if (sessionId <= 0)
        {
            throw new ArgumentException("Session ID must be a positive integer.", nameof(sessionId));
        }

        SessionId = sessionId;
    }

    /// <summary>
    /// Initializes a new instance with string session ID.
    /// </summary>
    /// <param name="sessionId">The session ID that was not found.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sessionId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="sessionId"/> is empty or whitespace.</exception>
    public TrackingSessionNotFoundException(string sessionId)
        : base($"Tracking session with ID '{sessionId}' was not found.")
    {
        SessionId = int.Parse(ValidateIdentifier(sessionId, nameof(sessionId)));
    }

    private static string GetDefaultMessage(int sessionId) =>
        $"Tracking session with ID {sessionId} was not found.";
}