namespace SignalRMapRealtime.DTOs;

/// <summary>
/// Provides extension methods for <see cref="AssetDto"/>.
/// </summary>
public static class AssetDtoExtensions
{
    /// <summary>
    /// Determines whether an asset requires special handling based on its condition.
    /// </summary>
    /// <param name="asset">The asset to check.</param>
    /// <returns>true if the asset requires special handling; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="asset"/> is null.</exception>
    public static bool IsSpecialHandlingRequired(this AssetDto asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        return asset.RequiresSpecialHandling || asset.Condition?.ToLowerInvariant() == "special handling";
    }

    /// <summary>
    /// Gets a formatted string representation of the asset's value.
    /// </summary>
    /// <param name="asset">The asset to format.</param>
    /// <returns>A formatted string representation of the asset's value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="asset"/> is null.</exception>
    public static string GetFormattedValue(this AssetDto asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        return asset.Value?.ToString("C", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// Checks if two assets have the same type and serial number.
    /// </summary>
    /// <param name="asset">The first asset to compare.</param>
    /// <param name="otherAsset">The second asset to compare.</param>
    /// <returns>true if the assets have the same type and serial number; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="asset"/> or <paramref name="otherAsset"/> is null.</exception>
    public static bool HasSameTypeAndSerialNumber(this AssetDto asset, AssetDto otherAsset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(otherAsset);
        return asset.AssetType == otherAsset.AssetType && string.Equals(asset.SerialNumber, otherAsset.SerialNumber, StringComparison.OrdinalIgnoreCase);
    }
}
