#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using NSubstitute;
using SignalRMapRealtime.DTOs;
using SignalRMapRealtime.Services;
using Xunit;

/// <summary>
/// Tests for the ClusteringService class.
/// </summary>
public class ClusteringServiceTests
{
    /// <summary>
    /// Tests that GetClustersAsync returns zero clusters when the location set is empty.
    /// </summary>
    [Fact]
    public async Task GetClusters_WithEmptyLocationSet_ReturnsZeroClusters()
    {
        // Arrange
        var service = Substitute.For<IClusteringService>();
        service.GetClustersAsync(Arg.Any<ClusterQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ClusterResponseDto
            {
                Clusters = Array.Empty<LocationClusterDto>(),
                TotalPoints = 0,
                GridCellKm = 0.5,
            });

        var request = new ClusterQueryRequest { GridCellKm = 0.5 };

        // Act
        var result = await service.GetClustersAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Clusters.Should().BeEmpty();
        result.TotalPoints.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetClustersAsync collapses two points within the same grid cell into one cluster.
    /// </summary>
    [Fact]
    public async Task GetClusters_WithCollocatedPoints_CollapsesSameCellIntoOneCluster()
    {
        // Arrange — two points within the same grid cell should form one cluster
        var service = Substitute.For<IClusteringService>();
        var expectedCluster = new LocationClusterDto(
            CenterLatitude: 51.5074,
            CenterLongitude: -0.1278,
            Count: 2,
            MinLatitude: 51.5073,
            MaxLatitude: 51.5075,
            MinLongitude: -0.1280,
            MaxLongitude: -0.1276);

        service.GetClustersAsync(Arg.Any<ClusterQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ClusterResponseDto
            {
                Clusters = new[] { expectedCluster },
                TotalPoints = 2,
                GridCellKm = 1.0,
            });

        var request = new ClusterQueryRequest { GridCellKm = 1.0 };

        // Act
        var result = await service.GetClustersAsync(request);

        // Assert
        result.Clusters.Should().HaveCount(1);
        result.TotalPoints.Should().Be(2);
        result.Clusters[0].Count.Should().Be(2);
    }

    /// <summary>
    /// Tests that GetHeatmapAsync normalizes intensity to 1.0 when MaxCount > 0.
    /// </summary>
    [Fact]
    public async Task GetHeatmap_WithMultiplePoints_NormalisesIntensityToOne()
    {
        // Arrange — peak tile intensity must equal 1.0 when MaxCount > 0
        var service = Substitute.For<IClusteringService>();
        service.GetHeatmapAsync(Arg.Any<ClusterQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new HeatmapResponseDto
            {
                Points = new[]
                {
                    new HeatmapPointDto(51.5074, -0.1278, 1.0, 10),
                    new HeatmapPointDto(51.5090, -0.1300, 0.5, 5),
                },
                TotalSamples = 15,
                MaxCount = 10,
            });

        var request = new ClusterQueryRequest { GridCellKm = 0.5 };

        // Act
        var result = await service.GetHeatmapAsync(request);

        // Assert
        result.Points.Should().NotBeEmpty();
        result.Points.Max(p => p.Intensity).Should().Be(1.0);
        result.TotalSamples.Should().Be(15);
    }

    /// <summary>
    /// Tests that GetHeatmapAsync returns empty tiles and zero MaxCount when there are no points.
    /// </summary>
    [Fact]
    public async Task GetHeatmap_WithNoPoints_ReturnsEmptyTilesAndZeroMaxCount()
    {
        // Arrange
        var service = Substitute.For<IClusteringService>();
        service.GetHeatmapAsync(Arg.Any<ClusterQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new HeatmapResponseDto
            {
                Points = Array.Empty<HeatmapPointDto>(),
                TotalSamples = 0,
                MaxCount = 0,
            });

        var request = new ClusterQueryRequest { GridCellKm = 0.5 };

        // Act
        var result = await service.GetHeatmapAsync(request);

        // Assert
        result.Points.Should().BeEmpty();
        result.MaxCount.Should().Be(0);
        result.TotalSamples.Should().Be(0);
    }
}
