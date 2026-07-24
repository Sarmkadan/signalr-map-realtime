#nullable enable

using System;
using Microsoft.AspNetCore.Mvc;
using SignalRMapRealtime.Exceptions;

namespace SignalRMapRealtime.Controllers;

/// <summary>
/// Extension methods for ASP.NET Core controllers to provide consistent error response handling
/// using the unified <see cref="IApiErrorSerializable"/> interface.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Returns a standardized error response for any exception using the unified error envelope structure.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="exception">The exception to convert to error response.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>An <see cref="IActionResult"/> with the standardized error response.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> or <paramref name="exception"/> is null.</exception>
    public static IActionResult ErrorResponse(this ControllerBase controller, Exception exception, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(exception);

        var error = exception.ToApiError();
        var errorJson = error.ToErrorResponse(indented);
        var statusCode = error.GetHttpStatusCode();

        return controller.StatusCode(statusCode, errorJson);
    }

    /// <summary>
    /// Returns a standardized error response for a specific exception type.
    /// </summary>
    /// <typeparam name="TException">The exception type to handle.</typeparam>
    /// <param name="controller">The controller instance.</param>
    /// <param name="exception">The exception to convert to error response.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>An <see cref="IActionResult"/> with the standardized error response.</returns>
    public static IActionResult ErrorResponse<TException>(this ControllerBase controller, TException exception, bool indented = false)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(exception);

        var error = exception.ToApiError();
        var errorJson = error.ToErrorResponse(indented);
        var statusCode = error.GetHttpStatusCode();

        return controller.StatusCode(statusCode, errorJson);
    }

    /// <summary>
    /// Creates a standardized error response object that can be returned by controllers.
    /// </summary>
    /// <param name="exception">The exception to convert to error response.</param>
    /// <param name="traceId">Optional trace identifier for logging.</param>
    /// <returns>A dictionary containing the standardized error response structure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static object CreateErrorResponse(this Exception exception, string? traceId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var error = exception.ToApiError();
        var errorEnvelope = new
        {
            errorCode = error.GetErrorCode(),
            message = error.GetMessage(),
            statusCode = error.GetHttpStatusCode(),
            timestamp = DateTime.UtcNow.ToString("o"),
            traceId = traceId,
            details = error.GetDetails()
        };

        return errorEnvelope;
    }
}