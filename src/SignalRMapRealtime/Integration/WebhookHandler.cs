// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Integration;

using System.Text.Json;
using System.Text.Json.Serialization;
using SignalRMapRealtime.Events;

/// <summary>
/// Handles incoming webhooks from external services.
/// Validates webhook signatures, deserializes payloads, and triggers appropriate handlers.
/// Supports multiple webhook providers with different signature schemes.
/// </summary>
public interface IWebhookHandler
{
    /// <summary>
    /// Processes an incoming webhook request.
    /// </summary>
    Task<WebhookProcessingResult> ProcessWebhookAsync(
        string provider,
        string payload,
        Dictionary<string, string> headers);

    /// <summary>
    /// Validates webhook signature to ensure authenticity.
    /// </summary>
    bool ValidateSignature(string provider, string payload, Dictionary<string, string> headers);
}

/// <summary>
/// Processes webhooks from various external services.
/// Validates signatures, parses payloads, and routes to appropriate handlers.
/// </summary>
public class WebhookHandler : IWebhookHandler
{
    private readonly ILogger<WebhookHandler> _logger;
    private readonly IEventBus _eventBus;

    public WebhookHandler(ILogger<WebhookHandler> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    /// <summary>
    /// Processes an incoming webhook from an external service.
    /// </summary>
    public async Task<WebhookProcessingResult> ProcessWebhookAsync(
        string provider,
        string payload,
        Dictionary<string, string> headers)
    {
        try
        {
            // Validate signature first to prevent processing unauthorized webhooks
            if (!ValidateSignature(provider, payload, headers))
            {
                _logger.LogWarning("Invalid webhook signature from provider {Provider}", provider);
                return new WebhookProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Invalid signature"
                };
            }

            // Parse and handle webhook based on provider type
            var result = provider.ToLowerInvariant() switch
            {
                "tracking-service" => await HandleTrackingServiceWebhook(payload),
                "notification-service" => await HandleNotificationWebhook(payload),
                "route-optimization" => await HandleRouteOptimizationWebhook(payload),
                _ => new WebhookProcessingResult
                {
                    Success = false,
                    ErrorMessage = $"Unknown provider: {provider}"
                }
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook from {Provider}", provider);
            return new WebhookProcessingResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Validates webhook signature based on provider's signature scheme.
    /// Different providers use different schemes (HMAC-SHA256, Bearer tokens, etc.).
    /// </summary>
    public bool ValidateSignature(string provider, string payload, Dictionary<string, string> headers)
    {
        // In production, implement actual signature validation
        // This would verify HMAC-SHA256 or provider-specific signatures

        return provider.ToLowerInvariant() switch
        {
            "tracking-service" => ValidateTrackingServiceSignature(payload, headers),
            "notification-service" => ValidateNotificationServiceSignature(payload, headers),
            "route-optimization" => ValidateRouteOptimizationSignature(payload, headers),
            _ => false
        };
    }

    /// <summary>
    /// Handles webhooks from the tracking service.
    /// </summary>
    private async Task<WebhookProcessingResult> HandleTrackingServiceWebhook(string payload)
    {
        var data = JsonSerializer.Deserialize<TrackingWebhookPayload>(payload);
        if (data == null)
        {
            return new WebhookProcessingResult { Success = false, ErrorMessage = "Invalid payload" };
        }

        _logger.LogInformation("Processing tracking webhook for vehicle {VehicleId}", data.VehicleId);

        // Publish events based on tracking data
        var locationEvent = new LocationUpdatedEvent
        {
            VehicleId = data.VehicleId,
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            Speed = data.Speed,
            Heading = data.Heading,
            Accuracy = data.Accuracy ?? 0,
            TriggeredBy = "WebhookService"
        };

        await _eventBus.PublishAsync(locationEvent);

        return new WebhookProcessingResult { Success = true, ProcessedAt = DateTime.UtcNow };
    }

    /// <summary>
    /// Handles webhooks from the notification service.
    /// </summary>
    private async Task<WebhookProcessingResult> HandleNotificationWebhook(string payload)
    {
        var data = JsonSerializer.Deserialize<NotificationWebhookPayload>(payload);
        if (data == null)
        {
            return new WebhookProcessingResult { Success = false, ErrorMessage = "Invalid payload" };
        }

        _logger.LogInformation("Processing notification webhook for delivery {DeliveryId}", data.DeliveryId);

        // Log notification status
        // In production, update notification status in database

        return new WebhookProcessingResult { Success = true, ProcessedAt = DateTime.UtcNow };
    }

    /// <summary>
    /// Handles webhooks from the route optimization service.
    /// </summary>
    private async Task<WebhookProcessingResult> HandleRouteOptimizationWebhook(string payload)
    {
        var data = JsonSerializer.Deserialize<RouteOptimizationWebhookPayload>(payload);
        if (data == null)
        {
            return new WebhookProcessingResult { Success = false, ErrorMessage = "Invalid payload" };
        }

        _logger.LogInformation("Processing route optimization webhook for route {RouteId}", data.RouteId);

        // Update route with optimized waypoints
        // In production, update route in database

        return new WebhookProcessingResult { Success = true, ProcessedAt = DateTime.UtcNow };
    }

    /// <summary>
    /// Validates signature from tracking service.
    /// Placeholder for actual HMAC validation.
    /// </summary>
    private bool ValidateTrackingServiceSignature(string payload, Dictionary<string, string> headers)
    {
        // TODO: Implement HMAC-SHA256 signature validation
        // For now, just check if signature header exists
        return headers.ContainsKey("X-Signature") || headers.ContainsKey("Authorization");
    }

    /// <summary>
    /// Validates signature from notification service.
    /// </summary>
    private bool ValidateNotificationServiceSignature(string payload, Dictionary<string, string> headers)
    {
        return headers.ContainsKey("X-Webhook-Secret");
    }

    /// <summary>
    /// Validates signature from route optimization service.
    /// </summary>
    private bool ValidateRouteOptimizationSignature(string payload, Dictionary<string, string> headers)
    {
        return headers.ContainsKey("Authorization");
    }
}

/// <summary>
/// Result of webhook processing.
/// </summary>
public class WebhookProcessingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

/// <summary>
/// Payload structure for tracking service webhooks.
/// </summary>
public class TrackingWebhookPayload
{
    [JsonPropertyName("vehicleId")]
    public Guid VehicleId { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("speed")]
    public double? Speed { get; set; }

    [JsonPropertyName("heading")]
    public double? Heading { get; set; }

    [JsonPropertyName("accuracy")]
    public double? Accuracy { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Payload structure for notification service webhooks.
/// </summary>
public class NotificationWebhookPayload
{
    [JsonPropertyName("deliveryId")]
    public Guid DeliveryId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Payload structure for route optimization webhooks.
/// </summary>
public class RouteOptimizationWebhookPayload
{
    [JsonPropertyName("routeId")]
    public Guid RouteId { get; set; }

    [JsonPropertyName("optimizationScore")]
    public double OptimizationScore { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
