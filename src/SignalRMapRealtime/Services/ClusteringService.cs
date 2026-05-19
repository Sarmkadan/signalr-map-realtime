#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;

/// <summary>Service for computing location clustering and heatmap density visualisations.</summary>
public interface IClusteringService
{
    /// <summary>
    /// Groups location points that fall within the same geographic grid cell into discrete clusters.
    /// Points that form their own cell are still returned as single-point clusters.
    /// </summary>
    /// <param name="request">Query parameters including time window, optional vehicle filter, and grid resolution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ClusterResponseDto"/> containing computed clusters and metadata.</returns>
    Task<ClusterResponseDto> GetClustersAsync(ClusterQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a normalised heatmap from location density within a geographic grid.
    /// Each cell's intensity is scaled to [0, 1] relative to the most active tile in the result.
    /// </summary>
    /// <param name="request">Query parameters including time window, optional vehicle filter, and grid resolution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="HeatmapResponseDto"/> with intensity-weighted tile points.</returns>
    Task<HeatmapResponseDto> GetHeatmapAsync(ClusterQueryRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Grid-based clustering and heatmap implementation.
/// Points are assigned to cells using a fixed-size geographic grid; each occupied cell
/// becomes a cluster or heatmap tile whose intensity is normalised across the result set.
/// Register as scoped to match the lifetime of its <see cref="ILocationService"/> dependency.
/// </summary>
public sealed class ClusteringService : IClusteringService
{
    private readonly ILocationService _locationService;
    private readonly ILogger<ClusteringService> _logger;

    /// <summary>Initialises a new instance of <see cref="ClusteringService"/>.</summary>
    public ClusteringService(ILocationService locationService, ILogger<ClusteringService> logger)
    {
        ArgumentNullException.ThrowIfNull(locationService);
        ArgumentNullException.ThrowIfNull(logger);
        _locationService = locationService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ClusterResponseDto> GetClustersAsync(
        ClusterQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var points = await LoadPointsAsync(request, cancellationToken).ConfigureAwait(false);
        var cellKm = Math.Max(0.1, request.GridCellKm);
        var cells = BucketIntoGrid(points, cellKm);

        var clusters = cells.Values.Select(cell =>
        {
            double avgLat = cell.Sum(p => p.Lat) / cell.Count;
            double avgLon = cell.Sum(p => p.Lon) / cell.Count;
            return new LocationClusterDto(
                CenterLatitude: avgLat,
                CenterLongitude: avgLon,
                Count: cell.Count,
                MinLatitude: cell.Min(p => p.Lat),
                MaxLatitude: cell.Max(p => p.Lat),
                MinLongitude: cell.Min(p => p.Lon),
                MaxLongitude: cell.Max(p => p.Lon));
        }).ToList();

        _logger.LogDebug(
            "Clustered {PointCount} points into {ClusterCount} clusters (grid cell {GridKm} km)",
            points.Count, clusters.Count, cellKm);

        return new ClusterResponseDto
        {
            Clusters = clusters,
            TotalPoints = points.Count,
            GridCellKm = cellKm,
        };
    }

    /// <inheritdoc/>
    public async Task<HeatmapResponseDto> GetHeatmapAsync(
        ClusterQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var points = await LoadPointsAsync(request, cancellationToken).ConfigureAwait(false);
        var cellKm = Math.Max(0.1, request.GridCellKm);
        var cells = BucketIntoGrid(points, cellKm);

        int maxCount = cells.Values.Select(c => c.Count).DefaultIfEmpty(0).Max();

        var heatPoints = cells.Values.Select(cell =>
        {
            double avgLat = cell.Sum(p => p.Lat) / cell.Count;
            double avgLon = cell.Sum(p => p.Lon) / cell.Count;
            double intensity = maxCount > 0 ? (double)cell.Count / maxCount : 0.0;
            return new HeatmapPointDto(avgLat, avgLon, intensity, cell.Count);
        })
        .OrderByDescending(h => h.Intensity)
        .ToList();

        _logger.LogDebug(
            "Built heatmap from {PointCount} samples across {TileCount} tiles (peak density {MaxCount})",
            points.Count, heatPoints.Count, maxCount);

        return new HeatmapResponseDto
        {
            Points = heatPoints,
            TotalSamples = points.Count,
            MaxCount = maxCount,
        };
    }

    private async Task<List<(double Lat, double Lon)>> LoadPointsAsync(
        ClusterQueryRequest request, CancellationToken cancellationToken)
    {
        var from = request.From ?? DateTime.UtcNow.AddHours(-24);
        var to = request.To ?? DateTime.UtcNow;

        IEnumerable<LocationDto> locations;

        if (request.VehicleId.HasValue)
        {
            locations = await _locationService
                .GetLocationHistoryAsync(request.VehicleId.Value, from, to, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            var paged = await _locationService
                .GetLocationsAsync(1, 10_000, cancellationToken)
                .ConfigureAwait(false);
            locations = paged.Items.Where(l => l.RecordedAt >= from && l.RecordedAt <= to);
        }

        return locations
            .Where(l => IsWithinBounds(request, l.Latitude, l.Longitude))
            .Select(l => (l.Latitude, l.Longitude))
            .ToList();
    }

    private static bool IsWithinBounds(ClusterQueryRequest request, double lat, double lon)
    {
        if (request.MinLatitude.HasValue && lat < request.MinLatitude.Value) return false;
        if (request.MaxLatitude.HasValue && lat > request.MaxLatitude.Value) return false;
        if (request.MinLongitude.HasValue && lon < request.MinLongitude.Value) return false;
        if (request.MaxLongitude.HasValue && lon > request.MaxLongitude.Value) return false;
        return true;
    }

    /// <summary>
    /// Assigns each coordinate to a grid cell using a fixed-size geographic grid.
    /// Cell row and column are computed by dividing the coordinate by the approximate
    /// degree-width of the cell at that latitude.
    /// </summary>
    private static Dictionary<(int Row, int Col), List<(double Lat, double Lon)>> BucketIntoGrid(
        List<(double Lat, double Lon)> points, double cellKm)
    {
        const double LatDegPerKm = 1.0 / 110.574;

        var cells = new Dictionary<(int, int), List<(double Lat, double Lon)>>();

        foreach (var (lat, lon) in points)
        {
            double lonDegPerKm = 1.0 / (111.32 * Math.Max(0.001, Math.Cos(lat * Math.PI / 180.0)));
            int row = (int)Math.Floor(lat / (cellKm * LatDegPerKm));
            int col = (int)Math.Floor(lon / (cellKm * lonDegPerKm));

            var key = (row, col);
            if (!cells.ContainsKey(key))
                cells[key] = new List<(double, double)>();

            cells[key].Add((lat, lon));
        }

        return cells;
    }
}

/// <summary>Extension methods for registering the clustering service in the DI container.</summary>
public static class ClusteringExtensions
{
    /// <summary>
    /// Registers the clustering and heatmap service as scoped to align with
    /// its <see cref="ILocationService"/> dependency lifetime.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddClustering(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<IClusteringService, ClusteringService>();
        return services;
    }
}
