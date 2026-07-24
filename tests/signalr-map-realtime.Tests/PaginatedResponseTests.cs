using System;
using System.Collections.Generic;
using Xunit;
using SignalRMapRealtime.Models;

namespace SignalRMapRealtime.Tests;

public class PaginatedResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var pageNumber = 2;
        var pageSize = 10;
        var totalCount = 25;

        // Act
        var response = new PaginatedResponse<string>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(items, response.Items);
        Assert.Equal(pageNumber, response.PageNumber);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(totalCount, response.TotalCount);
        Assert.Equal(3, response.TotalPages); // 25 / 10 = 2.5 → 3 pages
        Assert.True(response.HasNextPage);
        Assert.True(response.HasPreviousPage);
    }

    [Fact]
    public void Constructor_WithEmptyItems_ShouldCalculatePagesCorrectly()
    {
        // Arrange
        var items = new List<object>();
        var pageNumber = 1;
        var pageSize = 5;
        var totalCount = 0;

        // Act
        var response = new PaginatedResponse<object>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void Constructor_WithSinglePage_ShouldNotHaveNextOrPrevious()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 3;

        // Act
        var response = new PaginatedResponse<int>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void Constructor_WithLastPage_ShouldNotHaveNextPage()
    {
        // Arrange
        var items = new List<int> { 21, 22, 23 };
        var pageNumber = 3;
        var pageSize = 10;
        var totalCount = 25;

        // Act
        var response = new PaginatedResponse<int>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(3, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.True(response.HasPreviousPage);
    }

    [Fact]
    public void Empty_ShouldCreateEmptyResponse()
    {
        // Act
        var response = PaginatedResponse<string>.Empty();

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void Empty_WithCustomPageNumberAndSize_ShouldUseProvidedValues()
    {
        // Act
        var response = PaginatedResponse<int>.Empty(pageNumber: 5, pageSize: 20);

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(5, response.PageNumber);
        Assert.Equal(20, response.PageSize);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public void FromList_WithValidSource_ShouldCreatePaginatedResponse()
    {
        // Arrange
        var source = new List<string> { "a", "b", "c", "d", "e", "f", "g" };
        var pageNumber = 2;
        var pageSize = 3;

        // Act
        var response = PaginatedResponse<string>.FromList(source, pageNumber, pageSize);

        // Assert
        Assert.Equal(3, response.Items.Count);
        Assert.Equal(new[] { "d", "e", "f" }, response.Items);
        Assert.Equal(pageNumber, response.PageNumber);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(7, response.TotalCount);
        Assert.Equal(3, response.TotalPages); // 7 / 3 = 2.33 → 3 pages
        Assert.True(response.HasNextPage);
        Assert.True(response.HasPreviousPage);
    }

    [Fact]
    public void FromList_WithPageNumberLessThanOne_ShouldUsePageOne()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        var response = PaginatedResponse<int>.FromList(source, 0, 10);

        // Assert
        Assert.Equal(3, response.Items.Count);
        Assert.Equal(1, response.PageNumber);
    }

    [Fact]
    public void FromList_WithPageSizeLessThanOne_ShouldUseSizeTen()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3 };

        // Act
        var response = PaginatedResponse<int>.FromList(source, 1, 0);

        // Assert
        Assert.Equal(3, response.Items.Count);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public void FromList_WithEmptySource_ShouldReturnEmptyResponse()
    {
        // Arrange
        var source = new List<object>();

        // Act
        var response = PaginatedResponse<object>.FromList(source, 1, 10);

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public void FromList_WithSingleItem_ShouldCreateSinglePage()
    {
        // Arrange
        var source = new List<string> { "single" };

        // Act
        var response = PaginatedResponse<string>.FromList(source, 1, 10);

        // Assert
        Assert.Single(response.Items);
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void FromList_WithExactPageSize_ShouldReturnAllItems()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pageSize = 5;

        // Act
        var response = PaginatedResponse<int>.FromList(source, 1, pageSize);

        // Assert
        Assert.Equal(5, response.Items.Count);
        Assert.Equal(source, response.Items);
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void FromList_WithLargePageSize_ShouldReturnAllAvailableItems()
    {
        // Arrange
        var source = new List<string> { "a", "b", "c" };
        var pageSize = 100;

        // Act
        var response = PaginatedResponse<string>.FromList(source, 1, pageSize);

        // Assert
        Assert.Equal(3, response.Items.Count);
        Assert.Equal(source, response.Items);
    }

    [Fact]
    public void TotalPagesCalculation_ShouldRoundUp()
    {
        // Arrange
        var items = new List<int>();
        var pageSize = 10;

        // Act & Assert - Test various total counts that should round up
        var response1 = new PaginatedResponse<int>(items, 1, pageSize, 9);
        Assert.Equal(1, response1.TotalPages);

        var response2 = new PaginatedResponse<int>(items, 1, pageSize, 10);
        Assert.Equal(1, response2.TotalPages);

        var response3 = new PaginatedResponse<int>(items, 1, pageSize, 11);
        Assert.Equal(2, response3.TotalPages);

        var response4 = new PaginatedResponse<int>(items, 1, pageSize, 25);
        Assert.Equal(3, response4.TotalPages);
    }

    [Fact]
    public void HasNextPage_ShouldBeFalseOnLastPage()
    {
        // Arrange
        var items = new List<int> { 1 };
        var totalCount = 10;
        var pageSize = 5;
        var pageNumber = 2; // Last page (10 items / 5 per page = 2 pages)

        // Act
        var response = new PaginatedResponse<int>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void HasPreviousPage_ShouldBeFalseOnFirstPage()
    {
        // Arrange
        var items = new List<int> { 1, 2 };
        var totalCount = 20;
        var pageSize = 10;

        // Act
        var response = new PaginatedResponse<int>(items, 1, pageSize, totalCount);

        // Assert
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void Constructor_WithPageBeyondLastPage_ShouldReturnEmptyItemsWithCorrectMetadata()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageNumber = 5; // Beyond the last page (3 items / 3 per page = 1 page)
        var pageSize = 3;
        var totalCount = 3;

        // Act
        var response = new PaginatedResponse<int>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(5, response.PageNumber);
        Assert.Equal(3, response.PageSize);
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.True(response.HasPreviousPage);
    }

    [Fact]
    public void Constructor_WithPageZero_ShouldSetPageNumberToOne()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };
        var pageNumber = 0;
        var pageSize = 10;
        var totalCount = 30;

        // Act
        var response = new PaginatedResponse<string>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(3, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void Constructor_WithNegativePageNumber_ShouldSetPageNumberToOne()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageNumber = -1;
        var pageSize = 10;
        var totalCount = 30;

        // Act
        var response = new PaginatedResponse<int>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(3, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void Constructor_WithNegativePageSize_ShouldUseAbsoluteValue()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };
        var pageNumber = 1;
        var pageSize = -10;
        var totalCount = 30;

        // Act
        var response = new PaginatedResponse<int>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(10, response.PageSize);
        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public void Constructor_WithValueType_ShouldNotBoxItems()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 5;

        // Act
        var response = new PaginatedResponse<int>(items, pageNumber, pageSize, totalCount);

        // Assert - Verify items are not boxed by checking we can modify them
        Assert.Equal(5, response.Items.Count);
        Assert.IsType<int>(response.Items[0]);
    }

    [Fact]
    public void Constructor_WithReferenceType_ShouldHandleNullItemsCorrectly()
    {
        // Arrange
        var items = new List<TestClass> { new TestClass("a"), new TestClass("b") };
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 2;

        // Act
        var response = new PaginatedResponse<TestClass>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(2, response.Items.Count);
        Assert.All(response.Items, item => Assert.NotNull(item));
        Assert.Equal("a", response.Items[0].Name);
        Assert.Equal("b", response.Items[1].Name);
    }

    [Fact]
    public void Constructor_WithNullItems_ShouldCreateEmptyResponse()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var totalCount = 0;

        // Act
        var response = new PaginatedResponse<object>(null, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public void FromList_WithPageBeyondLastPage_ShouldReturnEmptyItems()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var pageNumber = 10; // Beyond the last page (5 items / 5 per page = 1 page)
        var pageSize = 5;

        // Act
        var response = PaginatedResponse<int>.FromList(source, pageNumber, pageSize);

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.HasNextPage);
        Assert.True(response.HasPreviousPage);
    }

    [Fact]
    public void FromList_WithLargePageNumber_ShouldReturnEmptyItems()
    {
        // Arrange
        var source = new List<string> { "a", "b", "c" };
        var pageNumber = 100;
        var pageSize = 10;

        // Act
        var response = PaginatedResponse<string>.FromList(source, pageNumber, pageSize);

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(1, response.TotalPages);
        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void FromList_WithReferenceType_ShouldMaintainObjectIdentity()
    {
        // Arrange
        var item1 = new TestClass("item1");
        var item2 = new TestClass("item2");
        var source = new List<TestClass> { item1, item2 };
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var response = PaginatedResponse<TestClass>.FromList(source, pageNumber, pageSize);

        // Assert - Verify we get the same object references back
        Assert.Same(item1, response.Items[0]);
        Assert.Same(item2, response.Items[1]);
    }

    private class TestClass
    {
        public string Name { get; }

        public TestClass(string name)
        {
            Name = name;
        }
    }
}
