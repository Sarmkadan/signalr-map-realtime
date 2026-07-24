#nullable enable
using System;
using System.Collections.Generic;
using Xunit;
using SignalRMapRealtime.Domain.Models;
using SignalRMapRealtime.Domain.Enums;

namespace SignalRMapRealtime.Tests;

public class AssetExtensionsTests
{
    [Fact]
    public void IsAssigned_AssetWithVehicleId_ReturnsTrue()
    {
        // Arrange
        var asset = new Asset
        {
            VehicleId = 123
        };

        // Act
        var result = asset.IsAssigned();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAssigned_AssetWithoutVehicleId_ReturnsFalse()
    {
        // Arrange
        var asset = new Asset();

        // Act
        var result = asset.IsAssigned();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAssigned_NullAsset_ThrowsArgumentNullException()
    {
        // Arrange
        Asset? asset = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => asset!.IsAssigned());
    }

    [Fact]
    public void GetAssetDetails_ValidAsset_ReturnsFormattedString()
    {
        // Arrange
        var asset = new Asset
        {
            Name = "Test Asset",
            SerialNumber = "SN12345",
            AssetType = AssetType.DeliveryVan,
            Value = 1000.50m,
            Condition = "Good"
        };

        // Act
        var result = asset.GetAssetDetails();

        // Assert
        Assert.Equal("Asset Test Asset (SN12345) - Type: DeliveryVan, Value: 1000.50, Condition: Good", result);
    }

    [Fact]
    public void GetAssetDetails_NullAsset_ThrowsArgumentNullException()
    {
        // Arrange
        Asset? asset = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => asset!.GetAssetDetails());
    }

    [Fact]
    public void GetAssetDetails_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var asset = new Asset
        {
            Name = "",
            SerialNumber = "SN12345"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => asset.GetAssetDetails());
    }

    [Fact]
    public void GetAssetDetails_EmptySerialNumber_ThrowsArgumentException()
    {
        // Arrange
        var asset = new Asset
        {
            Name = "Test Asset",
            SerialNumber = ""
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => asset.GetAssetDetails());
    }

    [Fact]
    public void NeedsSpecialHandling_AssetRequiresSpecialHandlingWithInstructions_ReturnsTrue()
    {
        // Arrange
        var asset = new Asset
        {
            RequiresSpecialHandling = true,
            SpecialHandlingInstructions = "Keep refrigerated"
        };

        // Act
        var result = asset.NeedsSpecialHandling();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NeedsSpecialHandling_AssetRequiresSpecialHandlingWithoutInstructions_ReturnsFalse()
    {
        // Arrange
        var asset = new Asset
        {
            RequiresSpecialHandling = true,
            SpecialHandlingInstructions = null
        };

        // Act
        var result = asset.NeedsSpecialHandling();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NeedsSpecialHandling_AssetDoesNotRequireSpecialHandling_ReturnsFalse()
    {
        // Arrange
        var asset = new Asset
        {
            RequiresSpecialHandling = false,
            SpecialHandlingInstructions = "Some instructions"
        };

        // Act
        var result = asset.NeedsSpecialHandling();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NeedsSpecialHandling_NullAsset_ThrowsArgumentNullException()
    {
        // Arrange
        Asset? asset = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => asset!.NeedsSpecialHandling());
    }

    [Fact]
    public void GetLocationHistoryCount_AssetWithLocationHistory_ReturnsCount()
    {
        // Arrange
        var asset = new Asset
        {
            LocationHistory = new List<Location>
            {
                new Location(),
                new Location(),
                new Location()
            }
        };

        // Act
        var result = asset.GetLocationHistoryCount();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetLocationHistoryCount_EmptyLocationHistory_ReturnsZero()
    {
        // Arrange
        var asset = new Asset
        {
            LocationHistory = new List<Location>()
        };

        // Act
        var result = asset.GetLocationHistoryCount();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetLocationHistoryCount_NullAsset_ThrowsArgumentNullException()
    {
        // Arrange
        Asset? asset = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => asset!.GetLocationHistoryCount());
    }

}