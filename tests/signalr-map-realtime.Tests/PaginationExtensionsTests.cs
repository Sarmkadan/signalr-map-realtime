using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SignalRMapRealtime.Utilities;

namespace SignalRMapRealtime.Tests;

public class PaginationExtensionsTests
{
    private readonly List<int> _numbers = Enumerable.Range(1, 10).ToList();

    [Fact]
    public void NormalizePaginationParameters_ValidInputs_ReturnsCorrectValues()
    {
        var result = PaginationExtensions.NormalizePaginationParameters(2, 5);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);
    }

    [Fact]
    public void NormalizePaginationParameters_PageSizeExceedsMax_ReturnsClampedSize()
    {
        var result = PaginationExtensions.NormalizePaginationParameters(1, 200, 100);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public void NormalizePaginationParameters_InvalidInputs_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationExtensions.NormalizePaginationParameters(0, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationExtensions.NormalizePaginationParameters(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationExtensions.NormalizePaginationParameters(1, 5, 0));
    }

    [Fact]
    public void ValidatePaginationParameters_ValidInputs_NoException()
    {
        var ex = Record.Exception(() =>
            PaginationExtensions.ValidatePaginationParameters(1, 5, 100));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidatePaginationParameters_InvalidInputs_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationExtensions.ValidatePaginationParameters(0, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationExtensions.ValidatePaginationParameters(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PaginationExtensions.ValidatePaginationParameters(1, 200, 100));
    }

    [Fact]
    public void ApplyPagination_Enumerable_ReturnsCorrectSubset()
    {
        var result = _numbers.ApplyPagination(2, 3).ToList();
        Assert.Equal(new[] { 4, 5, 6 }, result);
    }

    [Fact]
    public void ApplyPagination_Enumerable_NullSource_Throws()
    {
        List<int>? nullList = null;
        Assert.Throws<ArgumentNullException>(() => nullList!.ApplyPagination(1, 5));
    }

    [Fact]
    public void ApplyPagination_IQueryable_ReturnsCorrectSubset()
    {
        var query = _numbers.AsQueryable();
        var result = query.ApplyPagination(3, 2).ToList();
        Assert.Equal(new[] { 5, 6 }, result);
    }

    [Fact]
    public void ApplyPaginationWithSort_IQueryable_SortsAndPaginates()
    {
        var query = _numbers.AsQueryable();
        var result = query.ApplyPaginationWithSort(2, 3, q => q.OrderByDescending(x => x)).ToList();
        // Sorted descending: 10,9,8,7,6,5,4,3,2,1
        // Page 2, size 3 => items 7,6,5
        Assert.Equal(new[] { 7, 6, 5 }, result);
    }

    [Fact]
    public void ApplyPaginationWithSort_IQueryable_OrderByNull_Throws()
    {
        var query = _numbers.AsQueryable();
        Assert.Throws<ArgumentNullException>(() =>
            query.ApplyPaginationWithSort(1, 3, null!));
    }

    [Fact]
    public void GetPagedResults_EmptyCollection_ReturnsEmptyAndZeroCount()
    {
        var empty = Enumerable.Empty<int>();
        var (items, count) = empty.GetPagedResults(1, 5);
        Assert.Empty(items);
        Assert.Equal(0, count);
    }

    [Fact]
    public void GetPagedQueryableResults_EmptyQuery_ReturnsEmptyAndZeroCount()
    {
        var empty = Enumerable.Empty<int>().AsQueryable();
        var (items, count) = empty.GetPagedQueryableResults(1, 5);
        Assert.Empty(items);
        Assert.Equal(0, count);
    }

    [Fact]
    public void CalculateSkip_ValidInputs_ReturnsCorrect()
    {
        var skip = PaginationExtensions.CalculateSkip(3, 10);
        Assert.Equal(20, skip);
    }

    [Fact]
    public void CalculateSkip_InvalidInputs_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PaginationExtensions.CalculateSkip(0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => PaginationExtensions.CalculateSkip(1, 0));
    }

    [Fact]
    public void CalculateTotalPages_ValidInputs_ReturnsCorrect()
    {
        var pages = PaginationExtensions.CalculateTotalPages(25, 10);
        Assert.Equal(3, pages);
    }

    [Fact]
    public void CalculateTotalPages_InvalidInputs_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PaginationExtensions.CalculateTotalPages(10, 0));
    }

    [Fact]
    public void IsValidPageNumber_ValidAndInvalid()
    {
        Assert.True(PaginationExtensions.IsValidPageNumber(1, 25, 10)); // page 1 of 3
        Assert.True(PaginationExtensions.IsValidPageNumber(3, 25, 10)); // page 3 of 3
        Assert.False(PaginationExtensions.IsValidPageNumber(4, 25, 10)); // page 4 of 3
        Assert.False(PaginationExtensions.IsValidPageNumber(0, 25, 10)); // page 0 invalid
        Assert.False(PaginationExtensions.IsValidPageNumber(1, 25, 0)); // pageSize 0 invalid
    }
}
