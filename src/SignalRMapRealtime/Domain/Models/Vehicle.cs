// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Represents a vehicle being tracked in the real-time location system.
/// </summary>
public class Vehicle
{
    /// <summary>Unique identifier for the vehicle.</summary>
    public int Id { get; set; }

    /// <summary>Human-readable vehicle name or identifier (e.g., "Van-001").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>License plate or registration number.</summary>
    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>Current operational status of the vehicle.</summary>
    public VehicleStatus Status { get; set; } = VehicleStatus.Idle;

    /// <summary>Type or category of the asset being tracked.</summary>
    public AssetType AssetType { get; set; }

    /// <summary>Current driver or operator identifier.</summary>
    public int? DriverId { get; set; }

    /// <summary>Reference to the assigned driver/user.</summary>
    public User? Driver { get; set; }

    /// <summary>Manufacturer of the vehicle.</summary>
    public string? Manufacturer { get; set; }

    /// <summary>Model year of the vehicle.</summary>
    public int? ModelYear { get; set; }

    /// <summary>VIN or chassis number for identification.</summary>
    public string? VIN { get; set; }

    /// <summary>Maximum speed limit for this vehicle in km/h.</summary>
    public double? MaxSpeed { get; set; }

    /// <summary>Current fuel level as percentage (0-100).</summary>
    public double? FuelLevel { get; set; }

    /// <summary>Indicates if the vehicle is currently online and connected.</summary>
    public bool IsOnline { get; set; }

    /// <summary>Last known location of the vehicle.</summary>
    public Location? LastLocation { get; set; }

    /// <summary>Collection of all location records for this vehicle.</summary>
    public ICollection<Location> Locations { get; set; } = new List<Location>();

    /// <summary>Collection of tracking sessions for this vehicle.</summary>
    public ICollection<TrackingSession> TrackingSessions { get; set; } = new List<TrackingSession>();

    /// <summary>Collection of routes assigned to this vehicle.</summary>
    public ICollection<Route> Routes { get; set; } = new List<Route>();

    /// <summary>Timestamp when this vehicle was registered in the system.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of the last update to vehicle information.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Updates the vehicle status and tracks the change timestamp.
    /// </summary>
    public void UpdateStatus(VehicleStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a new location for this vehicle and updates the last known location.
    /// </summary>
    public void RecordLocation(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        LastLocation = location;
        location.Vehicle = this;
        Locations.Add(location);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines if the vehicle has exceeded its speed limit based on last location.
    /// </summary>
    public bool HasExceededSpeedLimit()
    {
        if (LastLocation?.Speed == null || MaxSpeed == null)
            return false;
        return LastLocation.Speed > MaxSpeed;
    }

    /// <summary>
    /// Checks if vehicle is idle based on status and online state.
    /// </summary>
    public bool IsIdle()
    {
        return Status == VehicleStatus.Idle || Status == VehicleStatus.AtDepot;
    }

    /// <summary>
    /// Toggles online/offline status of the vehicle.
    /// </summary>
    public void SetOnlineStatus(bool isOnline)
    {
        IsOnline = isOnline;
        UpdatedAt = DateTime.UtcNow;
    }
}
