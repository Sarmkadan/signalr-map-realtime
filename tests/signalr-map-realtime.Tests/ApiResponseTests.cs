using System;
using SignalRMapRealtime.Models;
using Xunit;

namespace SignalRMapRealtime.Tests;

public class ApiResponseTests
{
    [Fact]
    public void Generic_SuccessResponse_ShouldPopulateAllProperties()
    {
        // Arrange
        var data = new[] { "value1", "value2" };
        var message = "All good";
        var statusCode = 201;
        var traceId = Guid.NewGuid().ToString();

        // Act
        var response = ApiResponse<string[]>.SuccessResponse(data, message, statusCode, traceId);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(data, response.Data);
        Assert.Equal(message, response.Message);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(traceId, response.TraceId);
        Assert.True((DateTime.UtcNow - response.Timestamp).TotalSeconds < 5,
            "Timestamp should be set to a recent UTC time");
    }

    [Fact]
    public void Generic_FailureResponse_ShouldPopulateAllProperties()
    {
        // Arrange
        var message = "Something went wrong";
        var statusCode = 422;
        var traceId = Guid.NewGuid().ToString();

        // Act
        var response = ApiResponse<int>.FailureResponse(message, statusCode, traceId);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal(message, response.Message);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(traceId, response.TraceId);
        Assert.True((DateTime.UtcNow - response.Timestamp).TotalSeconds < 5);
    }

    [Fact]
    public void NonGeneric_SuccessResponse_ShouldPopulateAllProperties()
    {
        // Arrange
        var message = "Operation succeeded";
        var statusCode = 200;
        var traceId = Guid.NewGuid().ToString();

        // Act
        var response = ApiResponse.SuccessResponse(message, statusCode, traceId);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(message, response.Message);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(traceId, response.TraceId);
        Assert.True((DateTime.UtcNow - response.Timestamp).TotalSeconds < 5);
    }

    [Fact]
    public void NonGeneric_FailureResponse_ShouldPopulateAllProperties()
    {
        // Arrange
        var message = "Bad request";
        var statusCode = 400;
        var traceId = Guid.NewGuid().ToString();

        // Act
        var response = ApiResponse.FailureResponse(message, statusCode, traceId);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(message, response.Message);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(traceId, response.TraceId);
        Assert.True((DateTime.UtcNow - response.Timestamp).TotalSeconds < 5);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetTimestampToUtcNow()
    {
        // Act
        var genericResponse = new ApiResponse<string>();
        var nonGenericResponse = new ApiResponse();

        // Assert
        var now = DateTime.UtcNow;
        Assert.True((now - genericResponse.Timestamp).TotalSeconds < 5,
            "Generic response timestamp should be close to now");
        Assert.True((now - nonGenericResponse.Timestamp).TotalSeconds < 5,
            "Non‑generic response timestamp should be close to now");
    }

    [Fact]
    public void Generic_SuccessResponse_WithNullData_ShouldHandleGracefully()
    {
        // Act
        var response = ApiResponse<string?>.SuccessResponse(null, "null data", 200, null);

        // Assert
        Assert.True(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("null data", response.Message);
        Assert.Null(response.TraceId);
    }

    [Fact]
    public void Generic_FailureResponse_WithDefaultStatusCode_ShouldUse400()
    {
        // Act
        var response = ApiResponse<int>.FailureResponse("default status");

        // Assert
        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("default status", response.Message);
    }
}
