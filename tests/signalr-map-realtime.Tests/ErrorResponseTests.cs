using Xunit;
using SignalRMapRealtime.Models;

namespace SignalRMapRealtime.Tests;

public class ErrorResponseTests
{
    [Fact]
    public void Constructor_InitializesTimestampToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var response = new ErrorResponse();
        var after = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.InRange(response.Timestamp, before, after);
    }

    [Fact]
    public void ValidationError_SetsPropertiesCorrectly()
    {
        // Arrange
        var errors = new Dictionary<string, string[]> { { "Email", new[] { "Invalid format" } } };
        const string message = "Validation failed";
        const string traceId = "trace-123";

        // Act
        var response = ErrorResponse.ValidationError(errors, message, traceId);

        // Assert
        Assert.Equal(message, response.Message);
        Assert.Equal("VALIDATION_ERROR", response.ErrorCode);
        Assert.Equal(errors, response.Errors);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal(traceId, response.TraceId);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void NotFoundError_SetsPropertiesCorrectly()
    {
        // Arrange
        const string message = "Resource not found";
        const string traceId = "trace-456";

        // Act
        var response = ErrorResponse.NotFoundError(message, traceId);

        // Assert
        Assert.Equal(message, response.Message);
        Assert.Equal("NOT_FOUND", response.ErrorCode);
        Assert.Equal(404, response.StatusCode);
        Assert.Equal(traceId, response.TraceId);
    }

    [Fact]
    public void ServerError_IncludesOptionalDetails()
    {
        // Arrange
        const string message = "Server crash";
        const string stackTrace = "at Method()";
        const string innerException = "Inner fault";

        // Act
        var response = ErrorResponse.ServerError(message, null, stackTrace, innerException);

        // Assert
        Assert.Equal(message, response.Message);
        Assert.Equal("INTERNAL_SERVER_ERROR", response.ErrorCode);
        Assert.Equal(500, response.StatusCode);
        Assert.Equal(stackTrace, response.StackTrace);
        Assert.Equal(innerException, response.InnerException);
    }

    [Fact]
    public void UnauthorizedError_UsesDefaults()
    {
        // Act
        var response = ErrorResponse.UnauthorizedError();

        // Assert
        Assert.Equal("Unauthorized", response.Message);
        Assert.Equal("UNAUTHORIZED", response.ErrorCode);
        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public void ConflictError_SetsStatusCode()
    {
        // Arrange
        const string message = "Duplicate entry";

        // Act
        var response = ErrorResponse.ConflictError(message);

        // Assert
        Assert.Equal(message, response.Message);
        Assert.Equal("CONFLICT", response.ErrorCode);
        Assert.Equal(409, response.StatusCode);
    }

    [Fact]
    public void ForbiddenError_SetsStatusCode()
    {
        // Act
        var response = ErrorResponse.ForbiddenError();

        // Assert
        Assert.Equal("Access forbidden", response.Message);
        Assert.Equal("FORBIDDEN", response.ErrorCode);
        Assert.Equal(403, response.StatusCode);
    }
}
