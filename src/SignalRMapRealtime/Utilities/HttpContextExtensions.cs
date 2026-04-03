// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Utilities;

using System.Net;

/// <summary>
/// Extension methods for HttpContext operations.
/// Provides utilities for working with HTTP requests and responses.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the client's IP address from the request.
    /// Handles X-Forwarded-For header for proxy scenarios.
    /// </summary>
    public static string GetClientIpAddress(this HttpContext context)
    {
        // Check for X-Forwarded-For header (behind proxy)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',');
            var ip = ips.FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        // Check for X-Real-IP header (Nginx proxy)
        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ip = realIp.ToString().Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        // Use RemoteIpAddress as fallback
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Gets the request's user agent string.
    /// </summary>
    public static string GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers["User-Agent"].ToString();
    }

    /// <summary>
    /// Gets the referer URL from the request.
    /// </summary>
    public static string? GetReferer(this HttpContext context)
    {
        context.Request.Headers.TryGetValue("Referer", out var referer);
        return referer.ToString();
    }

    /// <summary>
    /// Gets the origin of the request.
    /// </summary>
    public static string? GetOrigin(this HttpContext context)
    {
        context.Request.Headers.TryGetValue("Origin", out var origin);
        return origin.ToString();
    }

    /// <summary>
    /// Checks if the request is using HTTPS.
    /// </summary>
    public static bool IsSecure(this HttpContext context)
    {
        return context.Request.IsHttps;
    }

    /// <summary>
    /// Gets the full request URL including query string.
    /// </summary>
    public static string GetFullUrl(this HttpContext context)
    {
        var request = context.Request;
        var scheme = request.Scheme;
        var host = request.Host.Value;
        var path = request.Path.Value;
        var query = request.QueryString.Value;

        return $"{scheme}://{host}{path}{query}";
    }

    /// <summary>
    /// Gets the base URL without path and query string.
    /// </summary>
    public static string GetBaseUrl(this HttpContext context)
    {
        var request = context.Request;
        var scheme = request.Scheme;
        var host = request.Host.Value;

        return $"{scheme}://{host}";
    }

    /// <summary>
    /// Checks if the request has a specific header.
    /// </summary>
    public static bool HasHeader(this HttpContext context, string headerName)
    {
        return context.Request.Headers.ContainsKey(headerName);
    }

    /// <summary>
    /// Gets a header value by name.
    /// </summary>
    public static string? GetHeader(this HttpContext context, string headerName)
    {
        return context.Request.Headers[headerName].ToString();
    }

    /// <summary>
    /// Checks if the request is an AJAX request.
    /// </summary>
    public static bool IsAjaxRequest(this HttpContext context)
    {
        return context.Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
    }

    /// <summary>
    /// Gets the content type of the request.
    /// </summary>
    public static string? GetContentType(this HttpContext context)
    {
        return context.Request.ContentType;
    }

    /// <summary>
    /// Checks if the request content is JSON.
    /// </summary>
    public static bool IsJsonRequest(this HttpContext context)
    {
        var contentType = context.Request.ContentType ?? string.Empty;
        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the request content is form data.
    /// </summary>
    public static bool IsFormRequest(this HttpContext context)
    {
        var contentType = context.Request.ContentType ?? string.Empty;
        return contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the HTTP method of the request.
    /// </summary>
    public static string GetMethod(this HttpContext context)
    {
        return context.Request.Method;
    }

    /// <summary>
    /// Sets a response header.
    /// </summary>
    public static void SetHeader(this HttpContext context, string headerName, string value)
    {
        context.Response.Headers[headerName] = value;
    }

    /// <summary>
    /// Sets the response status code.
    /// </summary>
    public static void SetStatusCode(this HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
    }

    /// <summary>
    /// Sets the response content type.
    /// </summary>
    public static void SetContentType(this HttpContext context, string contentType)
    {
        context.Response.ContentType = contentType;
    }

    /// <summary>
    /// Sets response to disable caching.
    /// </summary>
    public static void DisableCaching(this HttpContext context)
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }

    /// <summary>
    /// Sets response to allow caching for specified seconds.
    /// </summary>
    public static void SetCacheControl(this HttpContext context, int durationSeconds)
    {
        context.Response.Headers["Cache-Control"] = $"public, max-age={durationSeconds}";
    }

    /// <summary>
    /// Gets request parameters (from query string or form).
    /// </summary>
    public static Dictionary<string, string> GetParameters(this HttpContext context)
    {
        var parameters = new Dictionary<string, string>();

        // Add query string parameters
        foreach (var param in context.Request.Query)
        {
            parameters[param.Key] = param.Value.ToString();
        }

        return parameters;
    }
}
