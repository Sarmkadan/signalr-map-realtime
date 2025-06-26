// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Models;

using SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Represents a trackable asset (equipment, container, package) in the system.
/// </summary>
public class Asset
{
    /// <summary>Unique identifier for the asset.</summary>
    public int Id { get; set; }

    /// <summary>Asset name or identifier code.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Asset serial number or barcode.</summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Type classification of the asset.</summary>
    public AssetType AssetType { get; set; }

    /// <summary>Current value or estimated cost of the asset.</summary>
    public decimal? Value { get; set; }

    /// <summary>Description or additional details about the asset.</summary>
    public string? Description { get; set; }

    /// <summary>Vehicle this asset is currently assigned to.</summary>
    public int? VehicleId { get; set; }

    /// <summary>Reference to the associated vehicle.</summary>
    public Vehicle? Vehicle { get; set; }

    /// <summary>Current condition or status of the asset.</summary>
    public string? Condition { get; set; }

    /// <summary>Current location of the asset.</summary>
    public Location? CurrentLocation { get; set; }

    /// <summary>Location history of this asset.</summary>
    public ICollection<Location> LocationHistory { get; set; } = new List<Location>();

    /// <summary>Indicates if the asset requires special handling (fragile, temperature-controlled, etc.).</summary>
    public bool RequiresSpecialHandling { get; set; }

    /// <summary>Special handling instructions if applicable.</summary>
    public string? SpecialHandlingInstructions { get; set; }

    /// <summary>Date when the asset was registered in the system.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date of last update to asset information.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Date when the asset was last tracked.</summary>
    public DateTime? LastTrackedAt { get; set; }

    /// <summary>
    /// Assigns the asset to a vehicle and records the assignment timestamp.
    /// </summary>
    public void AssignToVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        Vehicle = vehicle;
        VehicleId = vehicle.Id;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes the asset from its current vehicle assignment.
    /// </summary>
    public void UnassignFromVehicle()
    {
        Vehicle = null;
        VehicleId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the current location of the asset.
    /// </summary>
    public void UpdateLocation(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        CurrentLocation = location;
        LastTrackedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the asset as requiring special handling.
    /// </summary>
    public void EnableSpecialHandling(string instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions))
            throw new ArgumentException("Special handling instructions cannot be empty.");
        RequiresSpecialHandling = true;
        SpecialHandlingInstructions = instructions;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes special handling requirements from the asset.
    /// </summary>
    public void DisableSpecialHandling()
    {
        RequiresSpecialHandling = false;
        SpecialHandlingInstructions = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the condition status of the asset.
    /// </summary>
    public void UpdateCondition(string newCondition)
    {
        if (string.IsNullOrWhiteSpace(newCondition))
            throw new ArgumentException("Condition cannot be empty.");
        Condition = newCondition;
        UpdatedAt = DateTime.UtcNow;
    }
}
