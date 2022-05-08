using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalRMapRealtime.Domain.Models
{
    public static class AssetExtensions
    {
        public static bool IsAssigned(this Asset asset)
        {
            return asset.VehicleId.HasValue;
        }

        public static string GetAssetDetails(this Asset asset)
        {
            return $"Asset {asset.Name} ({asset.SerialNumber}) - Type: {asset.AssetType}, Value: {asset.Value}, Condition: {asset.Condition}";
        }

        public static bool NeedsSpecialHandling(this Asset asset)
        {
            return asset.RequiresSpecialHandling && !string.IsNullOrEmpty(asset.SpecialHandlingInstructions);
        }

        public static int GetLocationHistoryCount(this Asset asset)
        {
            return asset.LocationHistory?.Count() ?? 0;
        }
    }
}
