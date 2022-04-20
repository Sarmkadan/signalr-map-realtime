using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SignalRMapRealtime.Utilities;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.Benchmarks;

[MemoryDiagnoser]
public class GeoLocationBenchmarks
{
    private readonly Location _from = new Location { Latitude = 40.7128, Longitude = -74.0060 };
    private readonly Location _to = new Location { Latitude = 34.0522, Longitude = -118.2437 };

    [Benchmark]
    public double CalculateDistance()
    {
        return GeoLocationExtensions.DistanceBetween(_from.Latitude, _from.Longitude, _to.Latitude, _to.Longitude);
    }

    [Benchmark]
    public string GetCardinalDirection()
    {
        return GeoLocationExtensions.GetCardinalDirection(45.0);
    }

    [Benchmark]
    public (double, double, double, double) GetBoundingBox()
    {
        return _from.GetBoundingBox(10.0);
    }

    [Benchmark]
    public bool IsValidCoordinate()
    {
        return _from.Latitude.IsValidCoordinate(_from.Longitude);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<GeoLocationBenchmarks>();
    }
}
