using System;
using SignalRMapRealtime.Models;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class ApiResponseExtensionsTests
{
    [Fact]
    public void IsSuccessful_HappyPath_ReturnsTrue()
    {
        // Arrange
        var response = new ApiResponse { Success = true };

        // Act
        var result = ApiResponseExtensions.IsSuccessful(response);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessful_NullResponse_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ApiResponseExtensions.IsSuccessful(null));
    }

    [Fact]
    public void IsSuccessful_GenericResponse_HappyPath_ReturnsTrue()
    {
        // Arrange
        var response = new ApiResponse<string> { Success = true };

        // Act
        var result = ApiResponseExtensions.IsSuccessful(response);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetMessageOrDefault_HappyPath_ReturnsMessage()
    {
        // Arrange
        var response = new ApiResponse { Message = "Hello" };

        // Act
        var result = ApiResponseExtensions.GetMessageOrDefault(response);

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void GetMessageOrDefault_NullResponse_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ApiResponseExtensions.GetMessageOrDefault(null));
    }

    [Fact]
    public void GetMessageOrDefault_EmptyMessage_ReturnsEmptyString()
    {
        // Arrange
        var response = new ApiResponse { Message = string.Empty };

        // Act
        var result = ApiResponseExtensions.GetMessageOrDefault(response);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EnsureSuccess_HappyPath_DoesNotThrow()
    {
        // Arrange
        var response = new ApiResponse { Success = true };

        // Act and Assert
        ApiResponseExtensions.EnsureSuccess(response);
    }

    [Fact]
    public void EnsureSuccess_NullResponse_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ApiResponseExtensions.EnsureSuccess(null));
    }

    [Fact]
    public void EnsureSuccess_FailureResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new ApiResponse { Success = false };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => ApiResponseExtensions.EnsureSuccess(response));
    }

    [Fact]
    public void WithData_HappyPath_ReturnsApiResponse()
    {
        // Arrange
        var response = new ApiResponse();
        var data = new object();

        // Act
        var result = ApiResponseExtensions.WithData(response, data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(data, result.Data);
    }

    [Fact]
    public void WithData_NullResponse_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ApiResponseExtensions.WithData(null, new object()));
    }
}
