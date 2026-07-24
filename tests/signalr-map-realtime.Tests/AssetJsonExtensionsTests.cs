using System;
using System.Text.Json;
using Xunit;
using SignalRMapRealtime.Domain.Models;

namespace SignalRMapRealtime.Tests;

public class AssetJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidAsset_ReturnsJsonString()
    {
        // Arrange
        var asset = new Asset();

        // Act
        var result = asset.ToJson();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.StartsWith("{", result);
        Assert.EndsWith("}", result);
    }

    [Fact]
    public void ToJson_NullAsset_ThrowsArgumentNullException()
    {
        // Arrange
        Asset? asset = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => asset.ToJson());
    }

    [Fact]
    public void ToJson_IndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var asset = new Asset();

        // Act
        var result = asset.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", result);
        Assert.Contains("  ", result);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsAsset()
    {
        // Arrange
        var asset = new Asset();
        var json = asset.ToJson();

        // Act
        var result = AssetJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Asset>(result);
    }

    [Fact]
    public void FromJson_NullOrEmptyJson_ReturnsNull()
    {
        // Act & Assert
        Assert.Null(AssetJsonExtensions.FromJson(null));
        Assert.Null(AssetJsonExtensions.FromJson(string.Empty));
        Assert.Null(AssetJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Act & Assert
        Assert.Throws<JsonException>(() => AssetJsonExtensions.FromJson("not valid json"));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndAsset()
    {
        // Arrange
        var asset = new Asset();
        var json = asset.ToJson();

        // Act
        var success = AssetJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.IsType<Asset>(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Act
        var success = AssetJsonExtensions.TryFromJson("invalid json", out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
