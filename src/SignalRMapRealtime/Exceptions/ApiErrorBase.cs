#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SignalRMapRealtime.Exceptions;

/// <summary>
/// Base class for API error serialization providing consistent envelope structure
/// across all exception types in the SignalRMapRealtime system.
/// </summary>
public abstract class ApiErrorBase : IApiErrorSerializable
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _jsonOptionsWithNullHandling = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// The underlying exception being serialized.
    /// </summary>
    protected Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiErrorBase"/> class.
    /// </summary>
    /// <param name="exception">The exception to serialize.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    protected ApiErrorBase(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Exception = exception;
    }

    /// <summary>
    /// Converts the exception to a standardized error response JSON string with consistent envelope structure.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string with consistent error envelope structure.</returns>
    public string ToErrorResponse(bool indented = false)
    {
        var errorCode = GetErrorCode();
        var message = GetMessage();
        var details = GetDetails();
        var statusCode = GetHttpStatusCode();

        var errorEnvelope = new Dictionary<string, object?>
        {
            ["errorCode"] = errorCode,
            ["message"] = message,
            ["statusCode"] = statusCode,
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["traceId"] = null // Will be set by controller
        };

        // Add details if available
        if (details != null)
        {
            errorEnvelope["details"] = details;
        }

        // Include inner exception information if available
        if (Exception.InnerException != null)
        {
            errorEnvelope["innerException"] = Exception.InnerException.Message;
        }

        var options = indented ? new JsonSerializerOptions(_jsonOptionsWithNullHandling) { WriteIndented = true } : _jsonOptionsWithNullHandling;

        return JsonSerializer.Serialize(errorEnvelope, options);
    }

    /// <summary>
    /// Gets the HTTP status code that should be returned for this error type.
    /// Default implementation returns 400 for client errors, 500 for server errors.
    /// </summary>
    public virtual int GetHttpStatusCode()
    {
        var errorCode = GetErrorCode();

        return errorCode switch
        {
            "VALIDATION_ERROR" => 400,
            "INVALID_INPUT" => 400,
            "RESOURCE_NOT_FOUND" => 404,
            "NOT_FOUND" => 404,
            "UNAUTHORIZED" => 401,
            "FORBIDDEN" => 403,
            "CONFLICT" => 409,
            "INTERNAL_SERVER_ERROR" => 500,
            _ => Exception is System.ComponentModel.DataAnnotations.ValidationException
                ? 400
                : 500
        };
    }

    /// <summary>
    /// Gets the error code for programmatic error handling.
    /// Derived classes should override this to provide specific error codes.
    /// </summary>
    public abstract string GetErrorCode();

    /// <summary>
    /// Gets the base exception message.
    /// </summary>
    public virtual string GetMessage()
    {
        return Exception.Message;
    }

    /// <summary>
    /// Gets additional error details specific to the exception type.
    /// Derived classes should override this to provide type-specific details.
    /// </summary>
    public virtual object? GetDetails()
    {
        return null;
    }

    /// <summary>
    /// Creates a standardized error response dictionary that can be used by controllers.
    /// </summary>
    protected Dictionary<string, object?> CreateErrorResponse()
    {
        return new Dictionary<string, object?>
        {
            ["errorCode"] = GetErrorCode(),
            ["message"] = GetMessage(),
            ["statusCode"] = GetHttpStatusCode(),
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["traceId"] = null,
            ["details"] = GetDetails()
        };
    }
}