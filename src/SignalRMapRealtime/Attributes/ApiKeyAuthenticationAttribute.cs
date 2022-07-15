#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Attributes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SignalRMapRealtime.Models;

/// <summary>
/// Custom authorization filter attribute for API key authentication.
/// Can be applied to controllers or action methods to require API key authentication.
/// Checks for API key in X-API-Key header or api_key query parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ApiKeyQueryParamName = "api_key";

    /// <summary>
    /// Executes the authorization filter to validate API key.
    /// </summary>
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Get API key from header or query parameter
        var apiKey = ExtractApiKey(context.HttpContext.Request);

        if (string.IsNullOrEmpty(apiKey))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "API key is required",
                message = $"Please provide an API key using '{ApiKeyHeaderName}' header or '{ApiKeyQueryParamName}' query parameter"
            });
            return Task.CompletedTask;
        }

        // Validate API key against the configured value
        if (!IsValidApiKey(apiKey, context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Invalid API key",
                message = "The provided API key is not valid"
            });
            return Task.CompletedTask;
        }

        // Authentication successful - add claims or user context as needed
        return Task.CompletedTask;
    }

    /// <summary>
    /// Extracts API key from request headers or query parameters.
    /// Checks X-API-Key header first, then api_key query parameter.
    /// </summary>
    private string? ExtractApiKey(HttpRequest request)
    {
        // Check header first
        if (request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue))
        {
            return headerValue.ToString();
        }

        // Check query parameter
        if (request.Query.TryGetValue(ApiKeyQueryParamName, out var queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }

    /// <summary>
    /// Validates the API key against the value configured under "Authentication:ApiKey".
    /// </summary>
    private static bool IsValidApiKey(string apiKey, IConfiguration? configuration)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        var validApiKey = configuration?["Authentication:ApiKey"];
        if (string.IsNullOrWhiteSpace(validApiKey))
            return false;

        return apiKey == validApiKey;
    }
}

/// <summary>
/// Action filter for validating model state and returning formatted error responses.
/// Automatically validates model binding and returns consistent error format.
/// </summary>
public class ValidateModelStateAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Executes before the action method to validate model state.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var errorResponse = ErrorResponse.ValidationError(errors, "Model validation failed");

            context.Result = new BadRequestObjectResult(errorResponse);
        }

        base.OnActionExecuting(context);
    }
}

/// <summary>
/// Extension methods for applying custom attributes to controllers.
/// </summary>
public static class AuthenticationAttributeExtensions
{
    /// <summary>
    /// Requires API key authentication for an endpoint.
    /// Usage: [ApiKeyAuthentication]
    /// </summary>
    public static void RequireApiKey(this ControllerBase controller)
    {
        // This is a marker method; actual implementation is in the attribute
    }
}
