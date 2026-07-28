using Xunit;
using SignalRMapRealtime.Configuration;

namespace SignalRMapRealtime.Tests;

public class CachingOptionsTests
{
    [Fact]
    public void Constructor_InitializesWithCorrectDefaults()
    {
        // Arrange & Act
        var options = new CachingOptions();

        // Assert
        Assert.True(options.Enabled);
        Assert.Equal(300, options.DefaultDurationSeconds);
        Assert.Equal(30, options.LocationCacheDurationSeconds);
        Assert.Equal(600, options.VehicleCacheDurationSeconds);
        Assert.Equal(1800, options.RouteCacheDurationSeconds);
        Assert.Equal(1800, options.AssetCacheDurationSeconds);
        Assert.Equal(600, options.SessionCacheDurationSeconds);
        Assert.False(options.UseDistributedCache);
        Assert.Null(options.DistributedCacheConnectionString);
        Assert.Equal(1440, options.RefreshTokenAbsoluteExpirationMinutes);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.Enabled = false;
        options.DefaultDurationSeconds = 120;
        options.LocationCacheDurationSeconds = 15;
        options.VehicleCacheDurationSeconds = 300;
        options.RouteCacheDurationSeconds = 900;
        options.AssetCacheDurationSeconds = 900;
        options.SessionCacheDurationSeconds = 300;
        options.UseDistributedCache = true;
        options.DistributedCacheConnectionString = "redis://localhost";
        options.RefreshTokenAbsoluteExpirationMinutes = 720;

        // Assert
        Assert.False(options.Enabled);
        Assert.Equal(120, options.DefaultDurationSeconds);
        Assert.Equal(15, options.LocationCacheDurationSeconds);
        Assert.Equal(300, options.VehicleCacheDurationSeconds);
        Assert.Equal(900, options.RouteCacheDurationSeconds);
        Assert.Equal(900, options.AssetCacheDurationSeconds);
        Assert.Equal(300, options.SessionCacheDurationSeconds);
        Assert.True(options.UseDistributedCache);
        Assert.Equal("redis://localhost", options.DistributedCacheConnectionString);
        Assert.Equal(720, options.RefreshTokenAbsoluteExpirationMinutes);
    }

    [Fact]
    public void DistributedCacheConnectionString_NullAndNonNull()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.DistributedCacheConnectionString = null;
        var nullValue = options.DistributedCacheConnectionString;

        options.DistributedCacheConnectionString = "redis://example";
        var nonNullValue = options.DistributedCacheConnectionString;

        // Assert
        Assert.Null(nullValue);
        Assert.Equal("redis://example", nonNullValue);
    }

    [Fact]
    public void BoundaryValues_NegativeAndZero()
    {
        // Arrange
        var options = new CachingOptions();

        // Act
        options.DefaultDurationSeconds = 0;
        options.LocationCacheDurationSeconds = -1;
        options.VehicleCacheDurationSeconds = -100;
        options.RouteCacheDurationSeconds = 0;
        options.AssetCacheDurationSeconds = -50;
        options.SessionCacheDurationSeconds = 0;
        options.RefreshTokenAbsoluteExpirationMinutes = -30;

        // Assert
        Assert.Equal(0, options.DefaultDurationSeconds);
        Assert.Equal(-1, options.LocationCacheDurationSeconds);
        Assert.Equal(-100, options.VehicleCacheDurationSeconds);
        Assert.Equal(0, options.RouteCacheDurationSeconds);
        Assert.Equal(-50, options.AssetCacheDurationSeconds);
        Assert.Equal(0, options.SessionCacheDurationSeconds);
        Assert.Equal(-30, options.RefreshTokenAbsoluteExpirationMinutes);
    }

    [Fact]
    public void UseDistributedCache_TrueWithConnectionString()
    {
        // Arrange
        var options = new CachingOptions
        {
            UseDistributedCache = true,
            DistributedCacheConnectionString = "redis://valid"
        };

        // Assert
        Assert.True(options.UseDistributedCache);
        Assert.Equal("redis://valid", options.DistributedCacheConnectionString);
    }

    [Fact]
    public void UseDistributedCache_FalseWithConnectionString()
    {
        // Arrange
        var options = new CachingOptions
        {
            UseDistributedCache = false,
            DistributedCacheConnectionString = "redis://ignored"
        };

        // Assert
        Assert.False(options.UseDistributedCache);
        Assert.Equal("redis://ignored", options.DistributedCacheConnectionString);
    }
}
