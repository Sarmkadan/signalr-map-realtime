#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides map tile URL generation and caching for Slippy Map tile coordinates (z/x/y).
/// Supports multiple tile server templates with round-robin subdomain rotation.
/// </summary>
public sealed class TileProvider : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<TileProvider> _logger;
    private readonly string _urlTemplate;
    private readonly string[] _subdomains;
    private int _subdomainIndex;
    private bool _disposed;

    /// <summary>Initializes a new instance of the <see cref="TileProvider"/> class.</summary>
    /// <param name="cache">Memory cache for tile metadata.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="urlTemplate">Tile URL template with {s}, {z}, {x}, {y} placeholders. Defaults to OSM.</param>
    /// <param name="subdomains">Subdomain rotation values for {s}. Defaults to ["a","b","c"].</param>
    public TileProvider(
        IMemoryCache cache,
        ILogger<TileProvider> logger,
        string urlTemplate = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        string[]? subdomains = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _urlTemplate = urlTemplate;
        _subdomains = subdomains ?? new[] { "a", "b", "c" };
    }

    /// <summary>
    /// Generates the tile URL for the given zoom level and tile coordinates.
    /// Rotates through configured subdomains for load distribution.
    /// </summary>
    /// <param name="zoom">Zoom level (0-19).</param>
    /// <param name="x">Tile X coordinate.</param>
    /// <param name="y">Tile Y coordinate.</param>
    /// <returns>Fully resolved tile URL.</returns>
    public string GetTileUrl(int zoom, int x, int y)
    {
        ValidateTileCoordinates(zoom, x, y);

        var subdomain = _subdomains[Interlocked.Increment(ref _subdomainIndex) % _subdomains.Length];

        return _urlTemplate
            .Replace("{s}", subdomain)
            .Replace("{z}", zoom.ToString())
            .Replace("{x}", x.ToString())
            .Replace("{y}", y.ToString());
    }

    /// <summary>
    /// Converts a WGS-84 coordinate to Slippy Map tile coordinates at the given zoom level.
    /// </summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="zoom">Zoom level (0-19).</param>
    /// <returns>Tuple of (tileX, tileY).</returns>
    public (int X, int Y) LatLonToTile(double latitude, double longitude, int zoom)
    {
        if (zoom < 0 || zoom > 19)
            throw new ArgumentOutOfRangeException(nameof(zoom), "Zoom must be between 0 and 19.");

        var n = 1 << zoom;
        var x = (int)Math.Floor((longitude + 180.0) / 360.0 * n);
        var latRad = latitude * Math.PI / 180.0;
        var y = (int)Math.Floor((1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n);

        x = Math.Clamp(x, 0, n - 1);
        y = Math.Clamp(y, 0, n - 1);

        return (x, y);
    }

    /// <summary>
    /// Returns all tile coordinates needed to cover a bounding box at a given zoom level.
    /// Results are cached to avoid repeated computation for the same viewport.
    /// </summary>
    /// <param name="southWestLat">South-west latitude.</param>
    /// <param name="southWestLon">South-west longitude.</param>
    /// <param name="northEastLat">North-east latitude.</param>
    /// <param name="northEastLon">North-east longitude.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <returns>List of (x, y) tile coordinates covering the bounds.</returns>
    public IReadOnlyList<(int X, int Y)> GetTilesForBounds(
        double southWestLat, double southWestLon,
        double northEastLat, double northEastLon,
        int zoom)
    {
        var cacheKey = $"tiles:{zoom}:{southWestLat:F4}:{southWestLon:F4}:{northEastLat:F4}:{northEastLon:F4}";

        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);

            var (minX, maxY) = LatLonToTile(southWestLat, southWestLon, zoom);
            var (maxX, minY) = LatLonToTile(northEastLat, northEastLon, zoom);

            var tiles = new List<(int X, int Y)>();
            for (var x = minX; x <= maxX; x++)
            for (var y = minY; y <= maxY; y++)
                tiles.Add((x, y));

            _logger.LogDebug("Computed {TileCount} tiles for bounds at zoom {Zoom}", tiles.Count, zoom);
            return (IReadOnlyList<(int X, int Y)>)tiles;
        })!;
    }

    /// <summary>
    /// Returns the geographic center of a tile as (latitude, longitude).
    /// </summary>
    /// <param name="zoom">Zoom level.</param>
    /// <param name="x">Tile X coordinate.</param>
    /// <param name="y">Tile Y coordinate.</param>
    /// <returns>Center coordinates as (latitude, longitude).</returns>
    public (double Latitude, double Longitude) TileCenterToLatLon(int zoom, int x, int y)
    {
        ValidateTileCoordinates(zoom, x, y);

        var n = 1 << zoom;
        var lon = (x + 0.5) / n * 360.0 - 180.0;
        var latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2.0 * (y + 0.5) / n)));
        var lat = latRad * 180.0 / Math.PI;

        return (lat, lon);
    }

    private static void ValidateTileCoordinates(int zoom, int x, int y)
    {
        if (zoom < 0 || zoom > 19)
            throw new ArgumentOutOfRangeException(nameof(zoom), "Zoom must be between 0 and 19.");

        var maxTile = (1 << zoom) - 1;
        if (x < 0 || x > maxTile)
            throw new ArgumentOutOfRangeException(nameof(x), $"X must be between 0 and {maxTile} at zoom {zoom}.");
        if (y < 0 || y > maxTile)
            throw new ArgumentOutOfRangeException(nameof(y), $"Y must be between 0 and {maxTile} at zoom {zoom}.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
