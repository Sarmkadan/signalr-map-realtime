#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Configuration options for webhook signature verification and security.
/// Controls HMAC-SHA256 signature validation and timestamp-based replay attack prevention.
/// </summary>
public class WebhookOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Webhooks";

    /// <summary>
    /// Gets or sets the timestamp tolerance in minutes for replay attack prevention.
    /// Set to 0 to disable timestamp validation for a provider.
    /// </summary>
    public Dictionary<string, int> TimestampToleranceMinutes { get; set; } = new()
    {
        ["tracking-service"] = 5,      // 5 minutes tolerance for tracking service
        ["notification-service"] = 10,   // 10 minutes tolerance for notification service
        ["route-optimization"] = 15    // 15 minutes tolerance for route optimization
    };

    /// <summary>
    /// Gets the timestamp tolerance in minutes for the specified provider.
    /// </summary>
    /// <param name="provider">The webhook provider name.</param>
    /// <returns>Tolerance in minutes, or 0 if not configured.</returns>
    public int GetTimestampToleranceMinutes(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return 0;
        }

        var key = provider.ToLowerInvariant();
        return TimestampToleranceMinutes.TryGetValue(key, out var tolerance) ? tolerance : 0;
    }
}
