#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;

/// <summary>
/// Spatial clustering engine that groups geographic points using a grid-based approach
/// with configurable resolution. Operates on raw coordinate pairs for maximum reuse.
/// </summary>
public sealed class ClusterEngine
{
    private readonly ILogger<ClusterEngine> _logger;
    private readonly double _defaultCellSizeDegrees;

    /// <summary>Initializes a new instance of the <see cref="ClusterEngine"/> class.</summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="defaultCellSizeDegrees">Default grid cell size in degrees. Defaults to 0.01 (~1.1 km at equator).</param>
    public ClusterEngine(ILogger<ClusterEngine> logger, double defaultCellSizeDegrees = 0.01)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (defaultCellSizeDegrees <= 0)
            throw new ArgumentOutOfRangeException(nameof(defaultCellSizeDegrees), "Cell size must be positive.");

        _defaultCellSizeDegrees = defaultCellSizeDegrees;
    }

    /// <summary>
    /// Clusters a set of points into grid cells and returns the cluster centroids with member counts.
    /// </summary>
    /// <param name="points">Input points as (latitude, longitude, optional payload key).</param>
    /// <param name="cellSizeDegrees">Grid cell size override. Uses default when <c>null</c>.</param>
    /// <returns>List of computed clusters.</returns>
    public IReadOnlyList<ClusterResult> ComputeClusters(
        IEnumerable<(double Latitude, double Longitude, string? Key)> points,
        double? cellSizeDegrees = null)
    {
        var cellSize = cellSizeDegrees ?? _defaultCellSizeDegrees;
        var grid = new Dictionary<(int Row, int Col), List<(double Lat, double Lon, string? Key)>>();

        foreach (var (lat, lon, key) in points)
        {
            var row = (int)Math.Floor(lat / cellSize);
            var col = (int)Math.Floor(lon / cellSize);
            var cell = (row, col);

            if (!grid.TryGetValue(cell, out var list))
            {
                list = new List<(double, double, string?)>();
                grid[cell] = list;
            }
            list.Add((lat, lon, key));
        }

        var results = new List<ClusterResult>(grid.Count);

        foreach (var (cell, members) in grid)
        {
            var centroidLat = members.Average(m => m.Lat);
            var centroidLon = members.Average(m => m.Lon);
            var keys = members.Where(m => m.Key is not null).Select(m => m.Key!).ToList();
            results.Add(new ClusterResult(centroidLat, centroidLon, members.Count, keys));
        }

        _logger.LogDebug("Computed {ClusterCount} clusters from input points with cell size {CellSize}",
            results.Count, cellSize);

        return results;
    }

    /// <summary>
    /// Computes the density (points per square degree) for each occupied grid cell.
    /// </summary>
    /// <param name="points">Input coordinate pairs.</param>
    /// <param name="cellSizeDegrees">Grid cell size override.</param>
    /// <returns>Dictionary mapping cell identifiers to density values.</returns>
    public IReadOnlyDictionary<string, double> ComputeDensityMap(
        IEnumerable<(double Latitude, double Longitude)> points,
        double? cellSizeDegrees = null)
    {
        var cellSize = cellSizeDegrees ?? _defaultCellSizeDegrees;
        var cellArea = cellSize * cellSize;
        var counts = new Dictionary<string, int>();

        foreach (var (lat, lon) in points)
        {
            var row = (int)Math.Floor(lat / cellSize);
            var col = (int)Math.Floor(lon / cellSize);
            var cellKey = $"{row}:{col}";
            counts[cellKey] = counts.GetValueOrDefault(cellKey) + 1;
        }

        return counts.ToDictionary(kv => kv.Key, kv => kv.Value / cellArea);
    }

    /// <summary>
    /// Finds the single densest cluster cell from a set of points.
    /// </summary>
    /// <param name="points">Input coordinate pairs.</param>
    /// <param name="cellSizeDegrees">Grid cell size override.</param>
    /// <returns>The centroid and count of the densest cell, or <c>null</c> if input is empty.</returns>
    public ClusterResult? FindDensestCluster(
        IEnumerable<(double Latitude, double Longitude, string? Key)> points,
        double? cellSizeDegrees = null)
    {
        var clusters = ComputeClusters(points, cellSizeDegrees);
        return clusters.MaxBy(c => c.PointCount);
    }
}

/// <summary>Result of a single spatial cluster computation.</summary>
/// <param name="CentroidLatitude">Average latitude of clustered points.</param>
/// <param name="CentroidLongitude">Average longitude of clustered points.</param>
/// <param name="PointCount">Number of points in this cluster.</param>
/// <param name="MemberKeys">Optional payload keys of the clustered points.</param>
public sealed record ClusterResult(double CentroidLatitude, double CentroidLongitude, int PointCount, IReadOnlyList<string> MemberKeys);
