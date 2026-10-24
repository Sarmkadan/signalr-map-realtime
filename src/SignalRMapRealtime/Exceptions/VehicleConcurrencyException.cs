#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Thrown when a vehicle update is rejected because the stored vehicle changed after the client read it.
/// </summary>
public class VehicleConcurrencyException : LocationTrackingException
{
    /// <summary>
    /// Initializes a new instance with the conflicting vehicle ID and the underlying concurrency failure.
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle whose update was rejected.</param>
    /// <param name="innerException">The concurrency exception raised by the data layer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="innerException"/> is null.</exception>
    public VehicleConcurrencyException(int vehicleId, Exception innerException)
        : base($"Vehicle {vehicleId} was modified by another request. Reload the vehicle and retry with its current ETag.", innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
        VehicleId = vehicleId;
    }

    /// <summary>Gets the ID of the vehicle whose update was rejected.</summary>
    public int VehicleId { get; }
}
