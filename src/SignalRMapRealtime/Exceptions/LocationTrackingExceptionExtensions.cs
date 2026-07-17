using System;
using System.Collections.Generic;
using System.Globalization;
using SignalRMapRealtime.Exceptions;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="LocationTrackingException"/> and its derived exceptions.
/// </summary>
public static class LocationTrackingExceptionExtensions
{
    /// <summary>
    /// Gets the vehicle ID from a VehicleNotFoundException, or null if the exception is not of that type.
    /// </summary>
    /// <param name="exception">The location tracking exception.</param>
    /// <returns>The vehicle ID if available, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static int? GetVehicleId(this LocationTrackingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            VehicleNotFoundException vehicleEx => vehicleEx.VehicleId,
            _ => null
        };
    }

    /// <summary>
    /// Gets the asset ID from an AssetNotFoundException, or null if the exception is not of that type.
    /// </summary>
    /// <param name="exception">The location tracking exception.</param>
    /// <returns>The asset ID if available, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static int? GetAssetId(this LocationTrackingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            AssetNotFoundException assetEx => assetEx.AssetId,
            _ => null
        };
    }

    /// <summary>
    /// Gets the session ID from a TrackingSessionNotFoundException, or null if the exception is not of that type.
    /// </summary>
    /// <param name="exception">The location tracking exception.</param>
    /// <returns>The session ID if available, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static int? GetSessionId(this LocationTrackingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TrackingSessionNotFoundException sessionEx => sessionEx.SessionId,
            _ => null
        };
    }

    /// <summary>
    /// Gets the latitude and longitude coordinates from an InvalidLocationException, or null values if the exception is not of that type.
    /// </summary>
    /// <param name="exception">The location tracking exception.</param>
    /// <param name="latitude">Output parameter for latitude, or null if not available.</param>
    /// <param name="longitude">Output parameter for longitude, or null if not available.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static void GetCoordinates(this LocationTrackingException exception, out double? latitude, out double? longitude)
    {
        ArgumentNullException.ThrowIfNull(exception);

        latitude = exception switch
        {
            InvalidLocationException locEx => locEx.Latitude,
            _ => null
        };

        longitude = exception switch
        {
            InvalidLocationException locEx => locEx.Longitude,
            _ => null
        };
    }

    /// <summary>
    /// Creates a formatted error message that includes all relevant context from the exception hierarchy.
    /// </summary>
    /// <param name="exception">The location tracking exception.</param>
    /// <returns>A formatted error message with all available context.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static string ToErrorMessage(this LocationTrackingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var parts = new List<string>();

        parts.Add(exception.Message);

        switch (exception)
        {
            case VehicleNotFoundException vehicleEx:
                parts.Add($"Vehicle ID: {vehicleEx.VehicleId}");
                break;

            case AssetNotFoundException assetEx:
                parts.Add($"Asset ID: {assetEx.AssetId}");
                break;

            case TrackingSessionNotFoundException sessionEx:
                parts.Add($"Session ID: {sessionEx.SessionId}");
                break;

            case InvalidLocationException locEx:
                parts.Add($"Coordinates: Latitude={FormatCoordinate(locEx.Latitude)}, Longitude={FormatCoordinate(locEx.Longitude)}");
                break;
        }

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Determines whether the exception represents a "not found" error (vehicle, asset, or session).
    /// </summary>
    /// <param name="exception">The location tracking exception.</param>
    /// <returns>True if the exception is a not-found error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool IsNotFoundError(this LocationTrackingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is VehicleNotFoundException
            or AssetNotFoundException
            or TrackingSessionNotFoundException;
    }

    /// <summary>
    /// Determines whether the exception represents an invalid location error.
    /// </summary>
    /// <param name="exception">The location tracking exception.</param>
    /// <returns>True if the exception is an invalid location error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool IsInvalidLocationError(this LocationTrackingException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is InvalidLocationException;
    }

    /// <summary>
    /// Formats a coordinate value for display, handling null values.
    /// </summary>
    /// <param name="coordinate">The coordinate value.</param>
    /// <returns>A formatted string representation of the coordinate.</returns>
    private static string FormatCoordinate(double? coordinate) =>
        coordinate.HasValue
            ? coordinate.Value.ToString("F6", CultureInfo.InvariantCulture)
            : "null";
}