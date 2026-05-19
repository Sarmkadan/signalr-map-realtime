#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.DTOs;

/// <summary>Represents a cluster of location points grouped by geographic proximity.</summary>
/// <param name="CenterLatitude">Centroid latitude of the cluster in decimal degrees.</param>
/// <param name="CenterLongitude">Centroid longitude of the cluster in decimal degrees.</param>
/// <param name="Count">Number of location points contained in the cluster.</param>
/// <param name="MinLatitude">Southernmost latitude within the cluster.</param>
/// <param name="MaxLatitude">Northernmost latitude within the cluster.</param>
/// <param name="MinLongitude">Westernmost longitude within the cluster.</param>
/// <param name="MaxLongitude">Easternmost longitude within the cluster.</param>
public record LocationClusterDto(
    double CenterLatitude,
    double CenterLongitude,
    int Count,
    double MinLatitude,
    double MaxLatitude,
    double MinLongitude,
    double MaxLongitude);

/// <summary>A single heatmap cell representing location density within a geographic tile.</summary>
/// <param name="Latitude">Latitude of the tile centre, in decimal degrees.</param>
/// <param name="Longitude">Longitude of the tile centre, in decimal degrees.</param>
/// <param name="Intensity">
/// Normalised intensity value between 0.0 (no activity) and 1.0 (peak activity),
/// computed relative to the busiest tile in the result set.
/// </param>
/// <param name="Count">Raw count of location samples that fall within this tile.</param>
public record HeatmapPointDto(
    double Latitude,
    double Longitude,
    double Intensity,
    int Count);

/// <summary>Full heatmap response including a normalised intensity grid and aggregate metadata.</summary>
public class HeatmapResponseDto
{
    /// <summary>Collection of heatmap tiles ordered by descending intensity.</summary>
    public IReadOnlyList<HeatmapPointDto> Points { get; init; } = Array.Empty<HeatmapPointDto>();

    /// <summary>Total number of location samples used to build the heatmap.</summary>
    public int TotalSamples { get; init; }

    /// <summary>Highest raw sample count among all tiles, used for normalisation.</summary>
    public int MaxCount { get; init; }

    /// <summary>UTC timestamp at which the heatmap data was computed.</summary>
    public DateTime ComputedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>Clustering result containing grouped location clusters and aggregate metadata.</summary>
public class ClusterResponseDto
{
    /// <summary>Individual clusters derived from the input location set.</summary>
    public IReadOnlyList<LocationClusterDto> Clusters { get; init; } = Array.Empty<LocationClusterDto>();

    /// <summary>Total number of location points that were processed.</summary>
    public int TotalPoints { get; init; }

    /// <summary>Grid cell size in kilometres used during clustering.</summary>
    public double GridCellKm { get; init; }

    /// <summary>UTC timestamp at which clustering was computed.</summary>
    public DateTime ComputedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>Request parameters for a clustering or heatmap query.</summary>
public class ClusterQueryRequest
{
    /// <summary>Optional vehicle ID to restrict the query to a single asset's location history.</summary>
    public int? VehicleId { get; init; }

    /// <summary>Start of the time window (UTC). Defaults to 24 hours ago when not supplied.</summary>
    public DateTime? From { get; init; }

    /// <summary>End of the time window (UTC). Defaults to the current UTC time when not supplied.</summary>
    public DateTime? To { get; init; }

    /// <summary>
    /// Grid cell edge length in kilometres used to bucket nearby points into a single cluster or tile.
    /// Smaller values produce finer granularity; larger values reduce cell count. Defaults to 0.5 km.
    /// </summary>
    public double GridCellKm { get; init; } = 0.5;

    /// <summary>Optional bounding-box minimum latitude filter (inclusive).</summary>
    public double? MinLatitude { get; init; }

    /// <summary>Optional bounding-box maximum latitude filter (inclusive).</summary>
    public double? MaxLatitude { get; init; }

    /// <summary>Optional bounding-box minimum longitude filter (inclusive).</summary>
    public double? MinLongitude { get; init; }

    /// <summary>Optional bounding-box maximum longitude filter (inclusive).</summary>
    public double? MaxLongitude { get; init; }
}
