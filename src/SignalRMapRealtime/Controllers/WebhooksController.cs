#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Controllers;

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Events;
using SignalRMapRealtime.Integration;

/// <summary>
/// Controller for receiving and processing webhook events from external services.
/// Validates HMAC-SHA256 signatures to ensure payload authenticity and prevents replay attacks.
/// Rejects unsigned or invalid requests with HTTP 401 Unauthorized before processing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookHandler _webhookHandler;
    private readonly ILogger<WebhooksController> _logger;
    private readonly WebhookOptions _webhookOptions;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhooksController"/> class.
    /// </summary>
    /// <param name="webhookHandler">Webhook handler for processing validated payloads.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="webhookOptions">Webhook configuration options.</param>
    /// <param name="configuration">Application configuration.</param>
    public WebhooksController(
        IWebhookHandler webhookHandler,
        ILogger<WebhooksController> logger,
        IOptions<WebhookOptions> webhookOptions,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(webhookHandler);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(webhookOptions);
        ArgumentNullException.ThrowIfNull(configuration);

        _webhookHandler = webhookHandler;
        _logger = logger;
        _webhookOptions = webhookOptions.Value;
        _configuration = configuration;
    }

    /// <summary>
    /// Receives a webhook from the tracking service.
    /// Validates HMAC-SHA256 signature in X-Signature header.
    /// </summary>
    /// <param name="payload">The webhook payload as JSON.</param>
    /// <returns>200 OK if processed successfully, 401 Unauthorized if signature is invalid.</returns>
    [HttpPost("tracking-service")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Track([FromBody] string payload)
    {
        return await ProcessWebhookAsync("tracking-service", payload);
    }

    /// <summary>
    /// Receives a webhook from the notification service.
    /// Validates HMAC-SHA256 signature in X-Webhook-Secret header.
    /// </summary>
    /// <param name="payload">The webhook payload as JSON.</param>
    /// <returns>200 OK if processed successfully, 401 Unauthorized if signature is invalid.</returns>
    [HttpPost("notification-service")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Notify([FromBody] string payload)
    {
        return await ProcessWebhookAsync("notification-service", payload);
    }

    /// <summary>
    /// Receives a webhook from the route optimization service.
    /// Validates HMAC-SHA256 signature in Authorization header (Bearer token format).
    /// </summary>
    /// <param name="payload">The webhook payload as JSON.</param>
    /// <returns>200 OK if processed successfully, 401 Unauthorized if signature is invalid.</returns>
    [HttpPost("route-optimization")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Optimize([FromBody] string payload)
    {
        return await ProcessWebhookAsync("route-optimization", payload);
    }

    /// <summary>
    /// Generic webhook processing endpoint that validates signature before deserialization.
    /// Rejects unsigned or invalid requests with HTTP 401 Unauthorized.
    /// </summary>
    /// <param name="provider">The webhook provider name.</param>
    /// <param name="payload">The webhook payload as JSON.</param>
    /// <returns>ActionResult indicating success or failure.</returns>
    private async Task<IActionResult> ProcessWebhookAsync(string provider, string payload)
    {
        try
        {
            // Validate request signature FIRST - before any deserialization or processing
            if (!ValidateSignature(provider, payload))
            {
                _logger.LogWarning("Unauthorized webhook attempt from provider {Provider}. Missing or invalid signature.", provider);
                return Unauthorized(new { error = "Invalid signature" });
            }

            // Validate timestamp to prevent replay attacks
            if (!ValidateTimestamp(provider))
            {
                _logger.LogWarning("Rejected potential replay attack from provider {Provider}. Timestamp is too old or invalid.", provider);
                return Unauthorized(new { error = "Request too old or timestamp invalid" });
            }

            // Process the webhook using the handler
            var result = await _webhookHandler.ProcessWebhookAsync(provider, payload, GetHeadersDictionary());

            if (result.Success)
            {
                _logger.LogInformation("Successfully processed webhook from {Provider}", provider);
                return Ok(new { success = true, processedAt = result.ProcessedAt });
            }
            else
            {
                _logger.LogError("Webhook processing failed for {Provider}: {ErrorMessage}", provider, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage ?? "Processing failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook from {Provider}", provider);
            return BadRequest(new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Validates the HMAC-SHA256 signature for the given provider.
    /// Extracts signature from the appropriate header based on provider.
    /// </summary>
    /// <param name="provider">The webhook provider name.</param>
    /// <param name="payload">The webhook payload.</param>
    /// <returns>True if signature is valid, false otherwise.</returns>
    private bool ValidateSignature(string provider, string payload)
    {
        ArgumentException.ThrowIfNullOrEmpty(provider);
        ArgumentNullException.ThrowIfNull(payload);

        return provider.ToLowerInvariant() switch
        {
            "tracking-service" => ValidateTrackingServiceSignature(payload),
            "notification-service" => ValidateNotificationServiceSignature(payload),
            "route-optimization" => ValidateRouteOptimizationSignature(payload),
            _ => false
        };
    }

    /// <summary>
    /// Validates the tracking service's HMAC-SHA256 signature.
    /// Expects signature in X-Signature header.
    /// </summary>
    private bool ValidateTrackingServiceSignature(string payload)
    {
        if (!Request.Headers.TryGetValue("X-Signature", out var signature) || signature.Count == 0)
        {
            _logger.LogDebug("Missing X-Signature header for tracking service webhook");
            return false;
        }

        var secret = _configuration["Webhooks:TrackingService:Secret"];
        return VerifyHmacSignature(payload, signature[0], secret);
    }

    /// <summary>
    /// Validates the notification service's HMAC-SHA256 signature.
    /// Expects signature in X-Webhook-Secret header.
    /// </summary>
    private bool ValidateNotificationServiceSignature(string payload)
    {
        if (!Request.Headers.TryGetValue("X-Webhook-Secret", out var signature) || signature.Count == 0)
        {
            _logger.LogDebug("Missing X-Webhook-Secret header for notification service webhook");
            return false;
        }

        var secret = _configuration["Webhooks:NotificationService:Secret"];
        return VerifyHmacSignature(payload, signature[0], secret);
    }

    /// <summary>
    /// Validates the route optimization service's HMAC-SHA256 signature.
    /// Expects signature in Authorization header (Bearer token format).
    /// </summary>
    private bool ValidateRouteOptimizationSignature(string payload)
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || authHeader.Count == 0)
        {
            _logger.LogDebug("Missing Authorization header for route optimization webhook");
            return false;
        }

        // Extract token from "Bearer <token>" format
        var token = authHeader[0];
        if (string.IsNullOrWhiteSpace(token) || !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Invalid Authorization header format for route optimization webhook");
            return false;
        }

        var secret = _configuration["Webhooks:RouteOptimization:Secret"];
        var signature = token["Bearer ".Length..];
        return VerifyHmacSignature(payload, signature, secret);
    }

    /// <summary>
    /// Validates timestamp header to prevent replay attacks.
    /// Checks if timestamp is recent (within tolerance window).
    /// </summary>
    private bool ValidateTimestamp(string provider)
    {
        // Check if timestamp validation is enabled for this provider
        var toleranceMinutes = _webhookOptions.GetTimestampToleranceMinutes(provider);
        if (toleranceMinutes <= 0)
        {
            // Timestamp validation disabled for this provider
            return true;
        }

        if (!Request.Headers.TryGetValue("X-Timestamp", out var timestampHeader) || timestampHeader.Count == 0)
        {
            _logger.LogDebug("Missing X-Timestamp header for {Provider} webhook", provider);
            return false;
        }

        if (!long.TryParse(timestampHeader[0], out var timestampSeconds))
        {
            _logger.LogDebug("Invalid X-Timestamp header format for {Provider} webhook", provider);
            return false;
        }

        var timestamp = DateTimeOffset.FromUnixTimeSeconds(timestampSeconds).UtcDateTime;
        var now = DateTime.UtcNow;
        var timeDifference = now - timestamp;

        if (timeDifference.TotalMinutes > toleranceMinutes || timeDifference.TotalMinutes < -toleranceMinutes)
        {
            _logger.LogWarning(
                "Timestamp validation failed for {Provider} webhook. Timestamp: {Timestamp}, Now: {Now}, Difference: {Difference} minutes",
                provider,
                timestamp,
                now,
                timeDifference.TotalMinutes);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verifies HMAC-SHA256 signature using constant-time comparison.
    /// Handles hex-encoded signatures with optional "sha256=" prefix.
    /// </summary>
    private bool VerifyHmacSignature(string payload, string providedSignature, string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogWarning("Webhook signature validation skipped: no shared secret configured for provider");
            return false;
        }

        var normalizedSignature = providedSignature.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? providedSignature["sha256=".Length..]
            : providedSignature;

        byte[] providedBytes;
        try
        {
            providedBytes = Convert.FromHexString(normalizedSignature.Trim());
        }
        catch (FormatException ex)
        {
            _logger.LogDebug(ex, "Invalid hex signature format");
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        return CryptographicOperations.FixedTimeEquals(providedBytes, computedBytes);
    }

    /// <summary>
    /// Converts request headers to a dictionary for the webhook handler.
    /// </summary>
    private Dictionary<string, string> GetHeadersDictionary()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in Request.Headers)
        {
            headers[header.Key] = header.Value!;
        }
        return headers;
    }
}
