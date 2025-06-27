// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Services;

using Microsoft.Extensions.Options;
using SignalRMapRealtime.Configuration;

/// <summary>
/// Notification service for sending alerts across multiple channels.
/// Supports email, SMS, and push notifications with a unified interface.
/// Queues notifications for async processing to avoid blocking requests.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an email notification.
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Sends an SMS notification.
    /// </summary>
    Task SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Sends a push notification.
    /// </summary>
    Task SendPushAsync(string userId, string title, string message);

    /// <summary>
    /// Sends a multi-channel notification (email + optional SMS/push).
    /// </summary>
    Task SendMultiChannelAsync(string recipient, string subject, string body, bool includeEmail = true, bool includePush = false);
}

/// <summary>
/// In-memory implementation of the notification service.
/// Queues notifications for processing and logs them for testing.
/// For production, implement actual email/SMS/push providers.
/// </summary>
public class InMemoryNotificationService : INotificationService
{
    private readonly ILogger<InMemoryNotificationService> _logger;
    private readonly IOptions<NotificationOptions> _options;
    private static readonly Queue<NotificationItem> NotificationQueue = new();

    public InMemoryNotificationService(
        ILogger<InMemoryNotificationService> logger,
        IOptions<NotificationOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    /// <summary>
    /// Queues an email notification and logs it.
    /// In production, this would send via SMTP.
    /// </summary>
    public Task SendEmailAsync(string to, string subject, string body)
    {
        if (!_options.Value.Enabled || !_options.Value.Email.Enabled)
        {
            _logger.LogDebug("Email notifications disabled. Skipping email to {To}", to);
            return Task.CompletedTask;
        }

        var notification = new NotificationItem
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Email,
            Recipient = to,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow,
            Status = NotificationStatus.Pending
        };

        QueueNotification(notification);
        _logger.LogInformation("Email notification queued for {Recipient}. Subject: {Subject}", to, subject);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Queues an SMS notification and logs it.
    /// In production, this would send via SMS provider (Twilio, etc.).
    /// </summary>
    public Task SendSmsAsync(string phoneNumber, string message)
    {
        if (!_options.Value.Enabled || !_options.Value.Sms.Enabled)
        {
            _logger.LogDebug("SMS notifications disabled. Skipping SMS to {PhoneNumber}", phoneNumber);
            return Task.CompletedTask;
        }

        var notification = new NotificationItem
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Sms,
            Recipient = phoneNumber,
            Body = message,
            CreatedAt = DateTime.UtcNow,
            Status = NotificationStatus.Pending
        };

        QueueNotification(notification);
        _logger.LogInformation("SMS notification queued for {Recipient}", phoneNumber);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Queues a push notification and logs it.
    /// In production, this would send via Firebase, OneSignal, etc.
    /// </summary>
    public Task SendPushAsync(string userId, string title, string message)
    {
        if (!_options.Value.Enabled || !_options.Value.Push.Enabled)
        {
            _logger.LogDebug("Push notifications disabled. Skipping push for user {UserId}", userId);
            return Task.CompletedTask;
        }

        var notification = new NotificationItem
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.Push,
            Recipient = userId,
            Subject = title,
            Body = message,
            CreatedAt = DateTime.UtcNow,
            Status = NotificationStatus.Pending
        };

        QueueNotification(notification);
        _logger.LogInformation("Push notification queued for user {UserId}. Title: {Title}", userId, title);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends notifications across multiple channels simultaneously.
    /// </summary>
    public async Task SendMultiChannelAsync(string recipient, string subject, string body, bool includeEmail = true, bool includePush = false)
    {
        var tasks = new List<Task>();

        if (includeEmail && recipient.Contains("@"))
            tasks.Add(SendEmailAsync(recipient, subject, body));

        if (includePush)
            tasks.Add(SendPushAsync(recipient, subject, body));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Queues a notification for async processing.
    /// </summary>
    private void QueueNotification(NotificationItem notification)
    {
        lock (NotificationQueue)
        {
            NotificationQueue.Enqueue(notification);
        }

        _logger.LogDebug("Notification {NotificationId} queued. Queue size: {QueueSize}", notification.Id, NotificationQueue.Count);
    }

    /// <summary>
    /// Gets pending notifications from the queue for processing.
    /// Used by background workers to actually send notifications.
    /// </summary>
    public static IEnumerable<NotificationItem> GetPendingNotifications(int maxCount = 10)
    {
        var pending = new List<NotificationItem>();

        lock (NotificationQueue)
        {
            for (int i = 0; i < maxCount && NotificationQueue.Count > 0; i++)
            {
                if (NotificationQueue.TryDequeue(out var notification))
                {
                    pending.Add(notification);
                }
            }
        }

        return pending;
    }
}

/// <summary>
/// Represents a queued notification.
/// </summary>
public class NotificationItem
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public NotificationStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Types of notifications.
/// </summary>
public enum NotificationType
{
    Email,
    Sms,
    Push
}

/// <summary>
/// Status of a notification in the queue.
/// </summary>
public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Cancelled
}
