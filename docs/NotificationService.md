# NotificationService

Documentation for `SignalRMapRealtime.Services.NotificationService` - notification service for sending alerts across multiple channels.

## INotificationService

Interface defining the notification contract. Supports email, SMS, and push notifications with a unified interface.

### Methods

#### `Task SendEmailAsync(string to, string subject, string body)`

Sends an email notification.

- **to** - recipient email address
- **subject** - email subject line
- **body** - email body content

#### `Task SendSmsAsync(string phoneNumber, string message)`

Sends an SMS notification.

- **phoneNumber** - recipient phone number
- **message** - SMS message text

#### `Task SendPushAsync(string userId, string title, string message)`

Sends a push notification.

- **userId** - target user identifier
- **title** - notification title
- **message** - notification message body

#### `Task SendMultiChannelAsync(string recipient, string subject, string body, bool includeEmail = true, bool includePush = false)`

Sends a multi-channel notification (email and optionally push). If the recipient contains `@`, email is sent when `includeEmail` is true. Push is sent when `includePush` is true.

- **recipient** - recipient identifier (email address or user ID)
- **subject** - notification subject
- **body** - notification body
- **includeEmail** - whether to include email channel (default: true)
- **includePush** - whether to include push channel (default: false)

## InMemoryNotificationService

In-memory implementation of `INotificationService`. Queues notifications for processing and logs them. Intended for development and testing; production use requires implementing actual email/SMS/push providers.

### Constructor

```csharp
public InMemoryNotificationService(
    ILogger<InMemoryNotificationService> logger,
    IOptions<NotificationOptions> options)
```

- **logger** - logger instance for diagnostic output
- **options** - notification configuration options controlling which channels are enabled

### Behavior

Each `Send*Async` method checks whether the global `Enabled` flag and the channel-specific `Enabled` flag are both true in `NotificationOptions`. If either is disabled, the call is a no-op (logged at Debug level).

When enabled, a `NotificationItem` is created with a new GUID, the appropriate `NotificationType`, status `Pending`, and the current UTC timestamp, then enqueued in a static thread-safe queue.

### Static Methods

#### `static IEnumerable<NotificationItem> GetPendingNotifications(int maxCount = 10)`

Dequeues up to `maxCount` pending notifications from the shared queue. Intended for use by background workers that process and send notifications.

- **maxCount** - maximum number of notifications to dequeue (default: 10)
- **Returns** - list of dequeued `NotificationItem` instances

## NotificationItem

Represents a queued notification.

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Unique notification identifier |
| `Type` | `NotificationType` | Channel type (Email, Sms, Push) |
| `Recipient` | `string` | Recipient address or identifier |
| `Subject` | `string?` | Notification subject (null for SMS) |
| `Body` | `string` | Notification body content |
| `CreatedAt` | `DateTime` | UTC timestamp when the notification was created |
| `Status` | `NotificationStatus` | Current processing status |
| `RetryCount` | `int` | Number of delivery retry attempts |
| `ErrorMessage` | `string?` | Error details if delivery failed |

## NotificationType

Enum defining supported notification channels.

- `Email` - email notification
- `Sms` - SMS notification
- `Push` - push notification

## NotificationStatus

Enum defining notification processing states.

- `Pending` - queued, not yet sent
- `Sent` - successfully delivered
- `Failed` - delivery failed
- `Cancelled` - cancelled before delivery
