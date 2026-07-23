using Xunit;
using SignalRMapRealtime.Models;

namespace SignalRMapRealtime.Tests
{
    public class PagedRequestExtensionsTests
    {
        [Fact]
        public void CalculateSkip_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var request = new PagedRequest { PageNumber = 2, PageSize = 10 };

            // Act
            var result = PagedRequestExtensions.CalculateSkip(request);

            // Assert
            Assert.Equal(19, result);
        }

        [Fact]
        public void CalculateSkip_NullRequest_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PagedRequestExtensions.CalculateSkip(null));
        }

        [Fact]
        public void CalculateTake_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var request = new PagedRequest { PageSize = 10 };

            // Act
            var result = PagedRequestExtensions.CalculateTake(request);

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public void CalculateTake_NullRequest_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PagedRequestExtensions.CalculateTake(null));
        }

        [Fact]
        public void HasFilters_HappyPath_ReturnsTrue()
        {
            // Arrange
            var request = new PagedRequest { SearchQuery = "test" };

            // Act
            var result = PagedRequestExtensions.HasFilters(request);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasFilters_EmptyRequest_ReturnsFalse()
        {
            // Arrange
            var request = new PagedRequest();

            // Act
            var result = PagedRequestExtensions.HasFilters(request);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetValidPageSize_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var request = new PagedRequest { PageSize = 10 };

            // Act
            var result = PagedRequestExtensions.GetValidPageSize(request);

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public void GetValidPageSize_NullRequest_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PagedRequestExtensions.GetValidPageSize(null));
        }

        [Fact]
        public void GetValidPageNumber_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var request = new PagedRequest { PageNumber = 2 };

            // Act
            var result = PagedRequestExtensions.GetValidPageNumber(request);

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetValidPageNumber_NullRequest_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PagedRequestExtensions.GetValidPageNumber(null));
        }

        [Fact]
        public void WithPagination_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var request = new PagedRequest { PageNumber = 2, PageSize = 10 };

            // Act
            var result = PagedRequestExtensions.WithPagination(request, 3, 20);

            // Assert
            Assert.Equal(3, result.PageNumber);
            Assert.Equal(20, result.PageSize);
        }

        [Fact]
        public void WithSearchQuery_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var request = new PagedRequest { SearchQuery = "test" };

            // Act
            var result = PagedRequestExtensions.WithSearchQuery(request, "new search query");

            // Assert
            Assert.Equal("new search query", result.SearchQuery);
        }

        [Fact]
        public void WithSortBy_HappyPath_ReturnsCorrectValue()
        {
            // Arrange
            var request = new PagedRequest { SortBy = "test" };

            // Act
            var result = PagedRequestExtensions.WithSortBy(request, "new sort by");

            // Assert
            Assert.Equal("new sort by", result.SortBy);
        }
    }
}
