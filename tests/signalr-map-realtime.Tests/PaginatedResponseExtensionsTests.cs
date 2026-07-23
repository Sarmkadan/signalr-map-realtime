using System;
using System.Collections.Generic;
using Xunit;
using SignalRMapRealtime.Models;

namespace SignalRMapRealtime.Tests
{
    public class PaginatedResponseExtensionsTests
    {
        [Fact]
        public void WithPageSize_HappyPath_ChangesPageSize()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 10, 10);

            // Act
            var result = paginatedResponse.WithPageSize(5);

            // Assert
            Assert.Equal(5, result.PageSize);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(5, result.Items.Count);
        }

        [Fact]
        public void WithPageSize_LargerPageSize_ReturnsAllItems()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 3, 5);

            // Act
            var result = paginatedResponse.WithPageSize(10);

            // Assert
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public void WithPageSize_SmallerPageSize_RecalculatesPageNumber()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var paginatedResponse = new PaginatedResponse<int>(items, 2, 5, 10);

            // Act
            var result = paginatedResponse.WithPageSize(3);

            // Assert
            Assert.Equal(3, result.PageSize);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(4, result.TotalPages);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public void WithPageSize_NewPageSizeTooSmall_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 10, 3);

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => paginatedResponse.WithPageSize(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => paginatedResponse.WithPageSize(-1));
        }

        [Fact]
        public void WithPageSize_NullSource_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PaginatedResponseExtensions.WithPageSize<int>(null, 10));
        }

        [Fact]
        public void Where_HappyPath_FiltersItems()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 10, 10);

            // Act
            var result = paginatedResponse.Where(x => x % 2 == 0);

            // Assert
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(5, result.Items.Count);
            Assert.All(result.Items, item => Assert.Equal(0, item % 2));
        }

        [Fact]
        public void Where_FiltersToEmptyCollection_ReturnsEmptyResponse()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 10, 5);

            // Act
            var result = paginatedResponse.Where(x => x > 100);

            // Assert
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(5, result.TotalCount);
            Assert.Empty(result.Items);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }

        [Fact]
        public void Where_NullPredicate_ThrowsArgumentNullException()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 10, 3);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => paginatedResponse.Where(null));
        }

        [Fact]
        public void Where_NullSource_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PaginatedResponseExtensions.Where<int>(null, x => true));
        }

        [Fact]
        public void Select_HappyPath_ProjectsItems()
        {
            // Arrange
            var items = new List<string> { "1", "2", "3", "4", "5" };
            var paginatedResponse = new PaginatedResponse<string>(items, 1, 10, 5);

            // Act
            var result = paginatedResponse.Select(int.Parse);

            // Assert
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(5, result.Items.Count);
            Assert.All(result.Items, item => Assert.IsType<int>(item));
            Assert.Contains(1, result.Items);
            Assert.Contains(5, result.Items);
        }

        [Fact]
        public void Select_ToDifferentType_ReturnsCorrectType()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var paginatedResponse = new PaginatedResponse<int>(items, 2, 2, 5);

            // Act
            var result = paginatedResponse.Select(x => x.ToString());

            // Assert
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, item => Assert.IsType<string>(item));
        }

        [Fact]
        public void Select_NullSelector_ThrowsArgumentNullException()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 10, 3);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => paginatedResponse.Select<int, int>(null));
        }

        [Fact]
        public void Select_NullSource_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PaginatedResponseExtensions.Select<int, int>(null, x => x));
        }

        [Fact]
        public void GetCurrentPage_HappyPath_ReturnsCurrentPageItems()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var paginatedResponse = new PaginatedResponse<int>(items, 2, 5, 10);

            // Act
            var result = paginatedResponse.GetCurrentPage();

            // Assert
            Assert.Equal(5, result.Count);
            Assert.Equal(6, result[0]);
            Assert.Equal(10, result[4]);
        }

        [Fact]
        public void GetCurrentPage_EmptyItems_ReturnsEmptyCollection()
        {
            // Arrange
            var items = new List<int>();
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 10, 0);

            // Act
            var result = paginatedResponse.GetCurrentPage();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetCurrentPage_NullSource_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PaginatedResponseExtensions.GetCurrentPage<int>(null));
        }

        [Fact]
        public void WithPageSize_MaintainsReadOnlyCollection()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var paginatedResponse = new PaginatedResponse<int>(items, 1, 5, 5);
            var result = paginatedResponse.WithPageSize(3);

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<int>>(result.Items);
        }

        [Fact]
        public void Where_MaintainsPaginationMetadata()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var paginatedResponse = new PaginatedResponse<int>(items, 3, 3, 10);

            // Act - filter to only even numbers
            var result = paginatedResponse.Where(x => x % 2 == 0);

            // Assert - pagination metadata should remain unchanged
            Assert.Equal(3, result.PageNumber);
            Assert.Equal(3, result.PageSize);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(4, result.TotalPages);
        }

        [Fact]
        public void Select_MaintainsPaginationMetadata()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var paginatedResponse = new PaginatedResponse<int>(items, 2, 5, 10);

            // Act - project to strings
            var result = paginatedResponse.Select(x => x.ToString());

            // Assert - pagination metadata should remain unchanged
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(10, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
        }
    }
}
