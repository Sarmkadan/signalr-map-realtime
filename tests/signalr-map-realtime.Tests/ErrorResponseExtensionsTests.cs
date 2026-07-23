using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using SignalRMapRealtime.Models;

namespace SignalRMapRealtime.Tests;

public class ErrorResponseExtensionsTests
{
    private ErrorResponse CreateSampleErrorResponse()
    {
        return new ErrorResponse
        {
            Message = "Original error",
            ErrorCode = "ERR001",
            Errors = new Dictionary<string, string[]>
            {
                { "FieldA", new[] { "Invalid value" } },
                { "FieldB", new[] { "Required", "Too short" } }
            },
            StatusCode = 400,
            Timestamp = DateTime.UtcNow.AddMinutes(-5),
            TraceId = "trace-123",
            StackTrace = "Line1\nLine2\nLine3",
            InnerException = "Inner exception message"
        };
    }

    [Fact]
    public void WithMessage_HappyPath_ReturnsNewInstanceWithUpdatedMessage()
    {
        var original = CreateSampleErrorResponse();
        var updated = original.WithMessage("New message");

        Assert.NotSame(original, updated);
        Assert.Equal("New message", updated.Message);
        Assert.Equal(original.ErrorCode, updated.ErrorCode);
        Assert.Equal(original.StatusCode, updated.StatusCode);
        Assert.Equal(original.TraceId, updated.TraceId);
        Assert.Equal(original.StackTrace, updated.StackTrace);
        Assert.Equal(original.InnerException, updated.InnerException);
        Assert.NotEqual(original.Timestamp, updated.Timestamp);
        Assert.Equal(original.Errors, updated.Errors);
    }

    [Fact]
    public void WithMessage_NullErrorResponse_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((ErrorResponse?)null)!.WithMessage("msg"));
    }

    [Fact]
    public void WithMessage_NullNewMessage_ThrowsArgumentNullException()
    {
        var original = CreateSampleErrorResponse();
        Assert.Throws<ArgumentNullException>(() => original.WithMessage(null!));
    }

    [Fact]
    public void WithStatusCode_HappyPath_ReturnsNewInstanceWithUpdatedStatusCode()
    {
        var original = CreateSampleErrorResponse();
        var updated = original.WithStatusCode(404);

        Assert.NotSame(original, updated);
        Assert.Equal(404, updated.StatusCode);
        Assert.Equal(original.Message, updated.Message);
        Assert.Equal(original.ErrorCode, updated.ErrorCode);
        Assert.Equal(original.TraceId, updated.TraceId);
        Assert.Equal(original.StackTrace, updated.StackTrace);
        Assert.Equal(original.InnerException, updated.InnerException);
        Assert.NotEqual(original.Timestamp, updated.Timestamp);
        Assert.Equal(original.Errors, updated.Errors);
    }

    [Fact]
    public void WithStatusCode_BoundaryValues_ThrowsArgumentOutOfRangeException()
    {
        var original = CreateSampleErrorResponse();

        // Below 100
        Assert.Throws<ArgumentOutOfRangeException>(() => original.WithStatusCode(99));

        // Above 999
        Assert.Throws<ArgumentOutOfRangeException>(() => original.WithStatusCode(1000));
    }

    [Fact]
    public void ToJson_HappyPath_ExcludesStackTraceByDefault()
    {
        var error = CreateSampleErrorResponse();
        var json = error.ToJson();

        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        Assert.True(dict.ContainsKey("message"));
        Assert.True(dict.ContainsKey("errorCode"));
        Assert.True(dict.ContainsKey("statusCode"));
        Assert.True(dict.ContainsKey("timestamp"));
        Assert.True(dict.ContainsKey("traceId"));
        Assert.True(dict.ContainsKey("errors"));
        Assert.False(dict.ContainsKey("stackTrace"));
        Assert.True(dict.ContainsKey("innerException"));
    }

    [Fact]
    public void ToJson_WithStackTrace_IncludesStackTraceArray()
    {
        var error = CreateSampleErrorResponse();
        var json = error.ToJson(includeStackTrace: true);

        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        Assert.True(dict.ContainsKey("stackTrace"));
        var stackArray = dict["stackTrace"].Deserialize<string[]>()!;
        Assert.Equal(3, stackArray.Length);
        Assert.Contains("Line1", stackArray);
        Assert.Contains("Line2", stackArray);
        Assert.Contains("Line3", stackArray);
    }

    [Fact]
    public void ToJson_EmptyErrors_DoesNotIncludeErrorsKey()
    {
        var error = new ErrorResponse
        {
            Message = "No errors",
            ErrorCode = "OK",
            Errors = new Dictionary<string, string[]>(),
            StatusCode = 200,
            Timestamp = DateTime.UtcNow,
            TraceId = null,
            StackTrace = null,
            InnerException = null
        };

        var json = error.ToJson();
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
        Assert.False(dict.ContainsKey("errors"));
    }

    [Fact]
    public void ToFriendlyMessage_HappyPath_ReturnsFormattedString()
    {
        var error = CreateSampleErrorResponse();
        var friendly = error.ToFriendlyMessage();

        Assert.Contains("Error: Original error", friendly);
        Assert.Contains("Code: ERR001 (Status: 400)", friendly);
        Assert.Contains("Trace ID: trace-123", friendly);
        Assert.Contains("Validation Errors:", friendly);
        Assert.Contains("FieldA:", friendly);
        Assert.Contains("- Invalid value", friendly);
        Assert.Contains("FieldB:", friendly);
        Assert.Contains("- Required", friendly);
        Assert.Contains("- Too short", friendly);
        Assert.Contains("Inner Exception: Inner exception message", friendly);
        Assert.Contains("Stack Trace:", friendly);
        Assert.Contains("Line1", friendly);
        Assert.Contains("Line2", friendly);
        Assert.Contains("Line3", friendly);
    }

    [Fact]
    public void ToFriendlyMessage_NullErrorResponse_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((ErrorResponse?)null)!.ToFriendlyMessage());
    }
}
