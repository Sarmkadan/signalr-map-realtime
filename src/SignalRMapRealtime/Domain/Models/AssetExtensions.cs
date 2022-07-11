using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalRMapRealtime.Domain.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="Asset"/> entities, allowing for additional functionality and operations.
    /// </summary>
    public static class AssetExtensions
    {
        /// <summary>
        /// Determines whether the asset is currently assigned to a vehicle.
        /// </summary>
        /// <param name="asset">The asset to check for vehicle assignment.</param>
        /// <returns><see langword="true"/> if the asset has a vehicle assigned; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asset"/> is <see langword="null"/>.</exception>
        public static bool IsAssigned(this Asset asset)
        {
            ArgumentNullException.ThrowIfNull(asset);
            return asset.VehicleId.HasValue;
        }

        /// <summary>
        /// Generates a formatted string containing detailed information about the asset.
        /// </summary>
        /// <param name="asset">The asset to get details for.</param>
        /// <returns>A formatted string with asset details, including name, serial number, type, value, and condition.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asset"/> is <see langword="null"/>.</exception>
        public static string GetAssetDetails(this Asset asset)
        {
            ArgumentNullException.ThrowIfNull(asset);
            ArgumentException.ThrowIfNullOrEmpty(asset.Name);
            ArgumentException.ThrowIfNullOrEmpty(asset.SerialNumber);

            return $"Asset {asset.Name} ({asset.SerialNumber}) - Type: {asset.AssetType}, Value: {asset.Value}, Condition: {asset.Condition}";
        }

        /// <summary>
        /// Determines whether the asset requires special handling.
        /// </summary>
        /// <param name="asset">The asset to check for special handling requirements.</param>
        /// <returns><see langword="true"/> if the asset requires special handling and has instructions; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asset"/> is <see langword="null"/>.</exception>
        public static bool NeedsSpecialHandling(this Asset asset)
        {
            ArgumentNullException.ThrowIfNull(asset);
            return asset.RequiresSpecialHandling && !string.IsNullOrEmpty(asset.SpecialHandlingInstructions);
        }

        /// <summary>
        /// Gets the count of location history entries for the asset.
        /// </summary>
        /// <param name="asset">The asset to get location history count for.</param>
        /// <returns>The number of location history entries, or 0 if null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asset"/> is <see langword="null"/>.</exception>
        public static int GetLocationHistoryCount(this Asset asset)
        {
            ArgumentNullException.ThrowIfNull(asset);
            return asset.LocationHistory?.Count() ?? 0;
        }
    }
}
