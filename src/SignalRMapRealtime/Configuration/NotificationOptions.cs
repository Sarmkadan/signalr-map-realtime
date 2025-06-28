// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Configuration options for notification services including email, SMS, and push notifications.
/// These options are loaded from appsettings.json under the "Notifications" section.
/// Supports multiple notification channels with independent configuration.
/// </summary>
public class NotificationOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Notifications";

    /// <summary>
    /// Enable or disable notifications globally.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Email notification configuration.
    /// </summary>
    public EmailNotificationOptions Email { get; set; } = new();

    /// <summary>
    /// SMS notification configuration.
    /// </summary>
    public SmsNotificationOptions Sms { get; set; } = new();

    /// <summary>
    /// Push notification configuration.
    /// </summary>
    public PushNotificationOptions Push { get; set; } = new();

    /// <summary>
    /// Default sender/from address for notifications.
    /// </summary>
    public string DefaultSender { get; set; } = "noreply@signalrmaptracking.com";

    /// <summary>
    /// Enable notification queuing for async processing.
    /// Recommended for production to avoid blocking HTTP responses.
    /// </summary>
    public bool UseAsyncProcessing { get; set; } = true;

    /// <summary>
    /// Maximum retries for failed notification sending.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay in seconds between retry attempts.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;
}

/// <summary>
/// Email-specific notification configuration.
/// </summary>
public class EmailNotificationOptions
{
    /// <summary>
    /// Enable email notifications.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// SMTP server hostname.
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (typically 587 for TLS, 465 for SSL, 25 for unencrypted).
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Username for SMTP authentication.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Password for SMTP authentication.
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Enable TLS encryption for SMTP connections.
    /// </summary>
    public bool UsesTls { get; set; } = true;

    /// <summary>
    /// From address for email notifications.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the from address.
    /// </summary>
    public string FromDisplayName { get; set; } = "SignalR Map Realtime";

    /// <summary>
    /// Template directory path for email templates.
    /// </summary>
    public string TemplateDirectory { get; set; } = "/templates/email";
}

/// <summary>
/// SMS-specific notification configuration.
/// </summary>
public class SmsNotificationOptions
{
    /// <summary>
    /// Enable SMS notifications.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// SMS provider API key (e.g., Twilio AccountSid).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// SMS provider API secret (e.g., Twilio AuthToken).
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Provider type: "Twilio", "Nexmo", "AWS_SNS", "Custom".
    /// </summary>
    public string Provider { get; set; } = "Twilio";

    /// <summary>
    /// Sender phone number or sender ID.
    /// </summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for custom SMS provider.
    /// </summary>
    public string CustomProviderUrl { get; set; } = string.Empty;
}

/// <summary>
/// Push notification-specific configuration.
/// </summary>
public class PushNotificationOptions
{
    /// <summary>
    /// Enable push notifications.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Provider type: "Firebase", "OneSignal", "APNs", "Custom".
    /// </summary>
    public string Provider { get; set; } = "Firebase";

    /// <summary>
    /// Firebase project ID.
    /// </summary>
    public string FirebaseProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Firebase credentials JSON file path.
    /// </summary>
    public string FirebaseCredentialsPath { get; set; } = string.Empty;

    /// <summary>
    /// OneSignal application ID.
    /// </summary>
    public string OneSignalAppId { get; set; } = string.Empty;

    /// <summary>
    /// OneSignal API key.
    /// </summary>
    public string OneSignalApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Time-to-live for push notifications in seconds.
    /// </summary>
    public int TimeToLiveSeconds { get; set; } = 3600; // 1 hour
}
