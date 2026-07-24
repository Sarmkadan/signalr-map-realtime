using System;
using System.Collections.Generic;
using SignalRMapRealtime.Models;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class PaginatedResponseJsonExtensionsTests
{
    private static PaginatedResponse<int> CreateSampleResponse()
    {
        // The PaginatedResponse<T> class is expected to have a parameterless constructor
        // and an Items property (or similar) that can be initialized via an object initializer.
        // Adjust the property names if the actual implementation differs.
        return new PaginatedResponse<int>
        {
            Items = new List<int> { 1, 2, 3 },
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 3
        };
    }

    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var response = CreateSampleResponse();

        // Act
        var json = response.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        // Basic sanity check: the JSON should contain the items we set.
        Assert.Contains("\"items\":[1,2,3]", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        // Arrange
        var response = CreateSampleResponse();

        // Act
        var jsonIndented = response.ToJson(indented: true);
        var jsonCompact = response.ToJson(indented: false);

        // Assert
        Assert.NotEqual(jsonCompact, jsonIndented);
        // Indented JSON should contain line breaks.
        Assert.Contains(Environment.NewLine, jsonIndented);
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        PaginatedResponse<int>? response = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => response!.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_DeserializesCorrectly()
    {
        // Arrange
        var original = CreateSampleResponse();
        var json = original.ToJson();

        // Act
        var deserialized = PaginatedResponseJsonExtensions.FromJson<int>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.TotalCount, deserialized!.TotalCount);
        Assert.Equal(original.PageNumber, deserialized.PageNumber);
        Assert.Equal(original.PageSize, deserialized.PageSize);
        Assert.Equal(original.Items, deserialized.Items);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PaginatedResponseJsonExtensions.FromJson<int>(null!));
    }

    [Fact]
    public void FromJson_EmptyJson_ReturnsNull()
    {
        // Act
        var result = PaginatedResponseJsonExtensions.FromJson<int>(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndValue()
    {
        // Arrange
        var original = CreateSampleResponse();
        var json = original.ToJson();

        // Act
        var success = PaginatedResponseJsonExtensions.TryFromJson<int>(json, out var value);

        // Assert
        Assert.True(success);
        Assert.NotNull(value);
        Assert.Equal(original.TotalCount, value!.TotalCount);
        Assert.Equal(original.Items, value.Items);
    }

    [Fact]
    public void TryFromJson_EmptyJson_ReturnsFalse()
    {
        // Act
        var success = PaginatedResponseJsonExtensions.TryFromJson<int>(string.Empty, out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var success = PaginatedResponseJsonExtensions.TryFromJson<int>(invalidJson, out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }
}
