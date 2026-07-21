#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests.Services;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Models;
using SignalRMapRealtime.Services;
using SignalRMapRealtime.Domain.Enums;
using System.Threading;
using Xunit;

/// <summary>
/// Tests for ClusteringService zoom-level behavior (grid cell size variations).
/// Tests cover: high zoom yields no/small clusters, low zoom merges nearby points,
/// single point never clusters, and empty input returns empty.
/// </summary>
public class ClusteringServiceZoomTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IClusteringService _clusteringService;
    private readonly ILocationService _locationService;
    private readonly ILogger<ClusteringService> _logger;

    public ClusteringServiceZoomTests()
    {
        var services = new ServiceCollection();

        // Setup logging
        _logger = Substitute.For<ILogger<ClusteringService>>();
        services.AddLogging(builder => builder.AddProvider(new NSubstituteLoggerProvider()));

        // Setup location service with test data
        _locationService = Substitute.For<ILocationService>();
        services.AddSingleton(_locationService);

        // Add clustering service
        services.AddClustering();

        _serviceProvider = services.BuildServiceProvider();
        _clusteringService = _serviceProvider.GetRequiredService<IClusteringService>();
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
    }

    private class NSubstituteLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => Substitute.For<ILogger>();
        public void Dispose() { }
    }

    /// <summary>
    /// Tests that high zoom level (small grid cells) yields many small clusters.
    /// Each point should be in its own cluster when grid cells are very small.
    /// </summary>
    [Fact]
    public async Task GetClusters_HighZoomLevel_SmallGridCells_YieldsManySmallClusters()
    {
        // Arrange - create points that are close together (within 0.1km of each other)
        var points = new List<LocationDto>
        {
            CreateLocationDto(51.5074, -0.1278, DateTime.UtcNow.AddMinutes(-10)),  // London
            CreateLocationDto(51.5075, -0.1277, DateTime.UtcNow.AddMinutes(-9)),   // 111m north
            CreateLocationDto(51.5073, -0.1279, DateTime.UtcNow.AddMinutes(-8)),   // 111m south
        };

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.01, // Very small grid cells (10m) - should create separate clusters
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetClustersAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalPoints.Should().Be(3);
        result.Clusters.Should().HaveCount(3, "High zoom should create separate clusters for each point");
        result.GridCellKm.Should().Be(0.01);

        // Verify each cluster has exactly 1 point
        foreach (var cluster in result.Clusters)
        {
            cluster.Count.Should().Be(1);
        }
    }

    /// <summary>
    /// Tests that low zoom level (large grid cells) merges nearby points into single clusters.
    /// Points within the same large grid cell should be merged.
    /// </summary>
    [Fact]
    public async Task GetClusters_LowZoomLevel_LargeGridCells_MergesNearbyPoints()
    {
        // Arrange - create points that are close together (within 1km of each other)
        var points = new List<LocationDto>
        {
            CreateLocationDto(51.5074, -0.1278, DateTime.UtcNow.AddMinutes(-10)),  // London
            CreateLocationDto(51.5075, -0.1277, DateTime.UtcNow.AddMinutes(-9)),   // 111m north
            CreateLocationDto(51.5073, -0.1279, DateTime.UtcNow.AddMinutes(-8)),   // 111m south
        };

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 2.0, // Large grid cells (2km) - should merge all points
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetClustersAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalPoints.Should().Be(3);
        result.Clusters.Should().HaveCount(1, "Low zoom should merge nearby points into single cluster");
        result.Clusters[0].Count.Should().Be(3);

        // Verify cluster bounds encompass all points
        result.Clusters[0].MinLatitude.Should().BeApproximately(51.5073, 0.0001);
        result.Clusters[0].MaxLatitude.Should().BeApproximately(51.5075, 0.0001);
        result.Clusters[0].MinLongitude.Should().BeApproximately(-0.1279, 0.0001);
        result.Clusters[0].MaxLongitude.Should().BeApproximately(-0.1277, 0.0001);
    }

    /// <summary>
    /// Tests that a single point never clusters - it should always return as a single-point cluster.
    /// </summary>
    [Fact]
    public async Task GetClusters_SinglePoint_NeverClusters_ReturnsSinglePointCluster()
    {
        // Arrange - single point
        var points = new List<LocationDto>
        {
            CreateLocationDto(51.5074, -0.1278, DateTime.UtcNow.AddMinutes(-10)),  // London
        };

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.5, // Medium grid cells
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetClustersAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalPoints.Should().Be(1);
        result.Clusters.Should().HaveCount(1);
        result.Clusters[0].Count.Should().Be(1);
        result.Clusters[0].CenterLatitude.Should().BeApproximately(51.5074, 0.000001);
        result.Clusters[0].CenterLongitude.Should().BeApproximately(-0.1278, 0.000001);
    }

    /// <summary>
    /// Tests that empty input returns empty clusters array and zero total points.
    /// </summary>
    [Fact]
    public async Task GetClusters_EmptyInput_ReturnsEmptyClusters()
    {
        // Arrange - empty points list
        var points = new List<LocationDto>();

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.5,
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetClustersAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalPoints.Should().Be(0);
        result.Clusters.Should().BeEmpty();
        result.GridCellKm.Should().Be(0.5);
    }

    /// <summary>
    /// Tests that high zoom level for heatmap yields many tiles with low intensity.
    /// </summary>
    [Fact]
    public async Task GetHeatmap_HighZoomLevel_SmallGridCells_YieldsManyTilesWithLowIntensity()
    {
        // Arrange - create points that are close together
        var points = new List<LocationDto>
        {
            CreateLocationDto(51.5074, -0.1278, DateTime.UtcNow.AddMinutes(-10)),
            CreateLocationDto(51.5075, -0.1277, DateTime.UtcNow.AddMinutes(-9)),
        };

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.01, // Very small grid cells
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetHeatmapAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalSamples.Should().Be(2);
        result.Points.Should().HaveCount(2);
        result.MaxCount.Should().Be(1); // Each tile has 1 point
        result.Points.Max(p => p.Intensity).Should().Be(1.0); // Normalized intensity
        result.Points.Min(p => p.Intensity).Should().Be(1.0); // All tiles have same intensity
    }

    /// <summary>
    /// Tests that low zoom level for heatmap merges nearby points into fewer tiles with higher intensity.
    /// </summary>
    [Fact]
    public async Task GetHeatmap_LowZoomLevel_LargeGridCells_MergesPointsIntoFewerTiles()
    {
        // Arrange - create 5 points that are close together
        var points = new List<LocationDto>
        {
            CreateLocationDto(51.5074, -0.1278, DateTime.UtcNow.AddMinutes(-10)),
            CreateLocationDto(51.5075, -0.1277, DateTime.UtcNow.AddMinutes(-9)),
            CreateLocationDto(51.5073, -0.1279, DateTime.UtcNow.AddMinutes(-8)),
            CreateLocationDto(51.50745, -0.12775, DateTime.UtcNow.AddMinutes(-7)),
            CreateLocationDto(51.50742, -0.12778, DateTime.UtcNow.AddMinutes(-6)),
        };

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 1.0, // Large grid cells
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetHeatmapAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalSamples.Should().Be(5);
        result.Points.Should().HaveCount(1, "Low zoom should merge all points into single tile");
        result.MaxCount.Should().Be(5); // Single tile contains all 5 points
        result.Points[0].Intensity.Should().Be(1.0); // Normalized to max intensity
        result.Points[0].Count.Should().Be(5);
    }

    /// <summary>
    /// Tests that heatmap with empty input returns empty tiles and zero max count.
    /// </summary>
    [Fact]
    public async Task GetHeatmap_EmptyInput_ReturnsEmptyTiles()
    {
        // Arrange - empty points list
        var points = new List<LocationDto>();

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.5,
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetHeatmapAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalSamples.Should().Be(0);
        result.Points.Should().BeEmpty();
        result.MaxCount.Should().Be(0);
    }

    /// <summary>
    /// Tests that single point heatmap returns one tile with intensity 1.0.
    /// </summary>
    [Fact]
    public async Task GetHeatmap_SinglePoint_ReturnsOneTileWithMaxIntensity()
    {
        // Arrange - single point
        var points = new List<LocationDto>
        {
            CreateLocationDto(51.5074, -0.1278, DateTime.UtcNow.AddMinutes(-10)),
        };

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.5,
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetHeatmapAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalSamples.Should().Be(1);
        result.Points.Should().HaveCount(1);
        result.MaxCount.Should().Be(1);
        result.Points[0].Intensity.Should().Be(1.0);
        result.Points[0].Count.Should().Be(1);
    }

    /// <summary>
    /// Tests that very high zoom level (tiny grid cells) with points far apart yields separate clusters.
    /// Points far apart should never cluster regardless of zoom level.
    /// </summary>
    [Fact]
    public async Task GetClusters_VeryHighZoom_PointsFarApart_YieldsSeparateClusters()
    {
        // Arrange - points that are far apart (London to Paris ~340km)
        var points = new List<LocationDto>
        {
            CreateLocationDto(51.5074, -0.1278, DateTime.UtcNow.AddMinutes(-10)),  // London
            CreateLocationDto(48.8566, 2.3522, DateTime.UtcNow.AddMinutes(-5)),    // Paris
        };

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.001, // Very tiny grid cells (1m)
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetClustersAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalPoints.Should().Be(2);
        result.Clusters.Should().HaveCount(2, "Points far apart should never cluster");
        result.Clusters[0].Count.Should().Be(1);
        result.Clusters[1].Count.Should().Be(1);
    }

    /// <summary>
    /// Tests that medium zoom level (default 0.5km) creates reasonable clustering for typical use case.
    /// </summary>
    [Fact]
    public async Task GetClusters_MediumZoom_DefaultGridCell_CreatesReasonableClustering()
    {
        // Arrange - create 10 points in a 1km x 1km area
        var points = new List<LocationDto>();
        var baseLat = 51.5074;
        var baseLon = -0.1278;

        for (int i = 0; i < 10; i++)
        {
            points.Add(CreateLocationDto(
                baseLat + (i * 0.0009),  // ~100m spacing
                baseLon + (i * 0.0009),
                DateTime.UtcNow.AddMinutes(-i)
            ));
        }

        _locationService.GetLocationsAsync(1, 10000, Arg.Any<CancellationToken>())
            .Returns(new PaginatedResponse<LocationDto>(points, 1, 10000, points.Count));

        var request = new ClusterQueryRequest
        {
            GridCellKm = 0.5, // Default grid cell size
            From = DateTime.UtcNow.AddHours(-1),
            To = DateTime.UtcNow
        };

        // Act
        var result = await _clusteringService.GetClustersAsync(request);

        // Assert - should create multiple clusters but not one per point
        result.Should().NotBeNull();
        result.TotalPoints.Should().Be(10);
        result.Clusters.Should().HaveCountGreaterThan(1);
        result.Clusters.Should().HaveCountLessThan(10);

        // Verify all points are accounted for
        result.Clusters.Sum(c => c.Count).Should().Be(10);
    }

    /// <summary>
    /// Helper method to create LocationDto instances for testing.
    /// </summary>
    private static LocationDto CreateLocationDto(double latitude, double longitude, DateTime recordedAt)
    {
        return new LocationDto
        {
            Latitude = latitude,
            Longitude = longitude,
            RecordedAt = recordedAt,
            VehicleId = 1,
            LocationType = LocationType.TrackPoint,
            CreatedAt = recordedAt,
            Timestamp = recordedAt
        };
    }
}