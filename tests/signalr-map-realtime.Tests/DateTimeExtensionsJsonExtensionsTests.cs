using System;
using Xunit;
using SignalRMapRealtime.Utilities;

namespace SignalRMapRealtime.Tests;

public class DateTimeExtensionsJsonExtensionsTests
{
    private readonly DateTime _testDate = new DateTime(2023, 10, 5, 14, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void ToJson_ValidDate_ReturnsJsonString()
    {
        var json = _testDate.ToJson();
        Assert.NotNull(json);
        Assert.Contains("2023-10-05", json);
    }

    [Fact]
    public void ToJson_Indented_ReturnsFormattedJson()
    {
        var json = _testDate.ToJson(indented: true);
        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsDateTime()
    {
        var json = _testDate.ToJson();
        var result = DateTimeExtensionsJsonExtensions.FromJson(json);
        Assert.NotNull(result);
        Assert.Equal(_testDate, result);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ReturnsNull()
    {
        Assert.Null(DateTimeExtensionsJsonExtensions.FromJson(null));
        Assert.Null(DateTimeExtensionsJsonExtensions.FromJson(""));
        Assert.Null(DateTimeExtensionsJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        Assert.Null(DateTimeExtensionsJsonExtensions.FromJson("invalid-date-string"));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        var json = _testDate.ToJson();
        var success = DateTimeExtensionsJsonExtensions.TryFromJson(json, out var result);
        Assert.True(success);
        Assert.Equal(_testDate, result);
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        var success = DateTimeExtensionsJsonExtensions.TryFromJson(null, out var result);
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_WhitespaceInput_ReturnsTrueAndNullValue()
    {
        var success = DateTimeExtensionsJsonExtensions.TryFromJson("   ", out var result);
        Assert.True(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var success = DateTimeExtensionsJsonExtensions.TryFromJson("bad", out var result);
        Assert.False(success);
        Assert.Null(result);
    }
}
