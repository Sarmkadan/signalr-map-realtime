#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using MyCollectionExtensions = SignalRMapRealtime.Utilities.CollectionExtensions;

namespace SignalRMapRealtime.Tests;

public class CollectionExtensionsTests
{
    [Fact]
    public void AddIfNotExists_AddsItem_WhenNotExists()
    {
        var list = new List<int> { 1, 2 };
        MyCollectionExtensions.AddIfNotExists(list, 3);
        Assert.Contains(3, list);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void AddIfNotExists_DoesNotAdd_WhenExists()
    {
        var list = new List<int> { 1, 2 };
        MyCollectionExtensions.AddIfNotExists(list, 2);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void AddRangeIfNotExists_AddsItems_WhenNotExists()
    {
        var list = new List<int> { 1 };
        MyCollectionExtensions.AddRangeIfNotExists(list, new[] { 2, 3 });
        Assert.Equal(3, list.Count);
        Assert.Contains(2, list);
        Assert.Contains(3, list);
    }

    [Fact]
    public void RemoveWhere_RemovesCorrectItems()
    {
        var list = new List<int> { 1, 2, 3, 4 };
        MyCollectionExtensions.RemoveWhere(list, x => x % 2 == 0);
        Assert.Equal(2, list.Count);
        Assert.DoesNotContain(2, list);
        Assert.DoesNotContain(4, list);
    }

    [Fact]
    public void GetFirstOrNull_ReturnsFirstItem_WhenNotEmpty()
    {
        var list = new List<string> { "a", "b" };
        Assert.Equal("a", MyCollectionExtensions.GetFirstOrNull(list));
    }

    [Fact]
    public void GetFirstOrNull_ReturnsNull_WhenEmpty()
    {
        var list = new List<string>();
        Assert.Null(MyCollectionExtensions.GetFirstOrNull(list));
    }

    [Fact]
    public void GetLastOrNull_ReturnsLastItem_WhenNotEmpty()
    {
        var list = new List<string> { "a", "b" };
        Assert.Equal("b", MyCollectionExtensions.GetLastOrNull(list));
    }

    [Fact]
    public void IsNullOrEmpty_ReturnsTrue_WhenNullOrEmpty()
    {
        List<int>? nullList = null;
        var emptyList = new List<int>();
        Assert.True(MyCollectionExtensions.IsNullOrEmpty(nullList));
        Assert.True(MyCollectionExtensions.IsNullOrEmpty(emptyList));
    }

    [Fact]
    public void HasItems_ReturnsTrue_WhenHasItems()
    {
        var list = new List<int> { 1 };
        Assert.True(MyCollectionExtensions.HasItems(list));
    }

    [Fact]
    public void DistinctBy_ReturnsDistinctItems()
    {
        var list = new[] { (Id: 1, Name: "A"), (Id: 2, Name: "B"), (Id: 1, Name: "C") };
        var result = MyCollectionExtensions.DistinctBy(list, x => x.Id).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Name);
        Assert.Equal("B", result[1].Name);
    }

    [Fact]
    public void ChunkBy_ReturnsCorrectChunks()
    {
        var list = new[] { 1, 2, 3, 4, 5 };
        var result = MyCollectionExtensions.ChunkBy(list, 2).ToList();
        Assert.Equal(3, result.Count);
        Assert.Equal(2, result[0].Count);
        Assert.Equal(2, result[1].Count);
        Assert.Equal(1, result[2].Count);
    }

    [Fact]
    public void Flatten_ReturnsFlattenedList()
    {
        var list = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
        var result = MyCollectionExtensions.Flatten(list).ToList();
        Assert.Equal(4, result.Count);
        Assert.Equal(new[] { 1, 2, 3, 4 }, result);
    }
}
