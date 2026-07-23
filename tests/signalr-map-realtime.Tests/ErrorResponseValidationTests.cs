using Xunit;
using SignalRMapRealtime.Models;
using System.Collections.Generic;

namespace SignalRMapRealtime.Tests;

public class ErrorResponseValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Message = "Test message",
            ErrorCode = "Test error code",
            Errors = new Dictionary<string, string[]> { { "Test key", new[] { "Test error" } } },
            StatusCode = 400,
            Timestamp = DateTime.UtcNow,
        };

        // Act
        var problems = ErrorResponseValidation.Validate(errorResponse);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Message = "Test message",
            ErrorCode = "Test error code",
            Errors = new Dictionary<string, string[]> { { "Test key", new[] { "Test error" } } },
            StatusCode = 400,
            Timestamp = DateTime.UtcNow,
        };

        // Act
        var isValid = ErrorResponseValidation.IsValid(errorResponse);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Message = "Test message",
            ErrorCode = "Test error code",
            Errors = new Dictionary<string, string[]> { { "Test key", new[] { "Test error" } } },
            StatusCode = 400,
            Timestamp = DateTime.UtcNow,
        };

        // Act and Assert
        ErrorResponseValidation.EnsureValid(errorResponse);
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorResponseValidation.Validate(null));
    }

    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorResponseValidation.IsValid(null));
    }

    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ErrorResponseValidation.EnsureValid(null));
    }

    [Fact]
    public void Validate_InvalidInput_ReturnsProblems()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Message = string.Empty,
            ErrorCode = string.Empty,
            Errors = null,
            StatusCode = 200,
            Timestamp = default,
        };

        // Act
        var problems = ErrorResponseValidation.Validate(errorResponse);

        // Assert
        Assert.NotEmpty(problems);
    }

    [Fact]
    public void EnsureValid_InvalidInput_ThrowsArgumentException()
    {
        // Arrange
        var errorResponse = new ErrorResponse
        {
            Message = string.Empty,
            ErrorCode = string.Empty,
            Errors = null,
            StatusCode = 200,
            Timestamp = default,
        };

        // Act and Assert
        Assert.Throws<ArgumentException>(() => ErrorResponseValidation.EnsureValid(errorResponse));
    }
}
