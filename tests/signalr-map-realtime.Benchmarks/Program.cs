using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SignalRMapRealtime.Utilities;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.Benchmarks;

/// <summary>
/// A set of benchmarks for measuring the performance of geo-location related operations.
/// </summary>
[MemoryDiagnoser]
public class GeoLocationBenchmarks
{
    private readonly Location _from = new Location { Latitude = 40.7128, Longitude = -74.0060 };
    private readonly Location _to = new Location { Latitude = 34.0522, Longitude = -118.2437 };

    /// <summary>
    /// Calculates the distance between two points on the Earth's surface.
    /// </summary>
    /// <returns>The distance between the two points.</returns>
    [Benchmark]
    public double CalculateDistance()
    {
        return GeoLocationExtensions.DistanceBetween(_from.Latitude, _from.Longitude, _to.Latitude, _to.Longitude);
    }

    /// <summary>
    /// Determines the cardinal direction (e.g. N, NE, E, etc.) from a given angle in degrees.
    /// </summary>
    /// <param name="angleInDegrees">The angle in degrees.</param>
    /// <returns>The cardinal direction.</returns>
    [Benchmark]
    public string GetCardinalDirection()
    {
        return GeoLocationExtensions.GetCardinalDirection(45.0);
    }

    /// <summary>
    /// Calculates the bounding box for a given location and radius.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <returns>A tuple containing the minimum and maximum latitude and longitude.</returns>
    [Benchmark]
    public (double, double, double, double) GetBoundingBox()
    {
        return _from.GetBoundingBox(10.0);
    }

    /// <summary>
    /// Checks if a coordinate is valid (i.e. within the valid range of latitude and longitude values).
    /// </summary>
    /// <returns>True if the coordinate is valid, false otherwise.</returns>
    [Benchmark]
    public bool IsValidCoordinate()
    {
        return _from.Latitude.IsValidCoordinate(_from.Longitude);
    }
}

/// <summary>
/// The main program class.
/// </summary>
public class Program
{
    /// <summary>
    /// Runs the benchmarks.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<GeoLocationBenchmarks>();
    }
}
