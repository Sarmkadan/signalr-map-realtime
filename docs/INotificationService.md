# INotificationService

Interface and related types used to send notifications across multiple channels (email, SMS, push) with retry logic and status tracking. Designed for real-time applications where notifications must be delivered reliably and tracked for debugging or auditing.

## API

### `InMemoryNotificationService`

Concrete in-memory implementation of `INotificationService`. Stores notifications in a thread-safe collection and processes them asynchronously. Notifications are retained until successfully delivered or permanently failed after retries.

### `Task SendEmailAsync()`

Sends a notification via email channel.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `InvalidOperationException` if the notification is in an invalid state (e.g., `Status` is `Failed` and `RetryCount` exceeds limit).

### `Task SendSmsAsync()`

Sends a notification via SMS channel.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `InvalidOperationException` if the notification is in an invalid state (e.g., `Status` is `Failed` and `RetryCount` exceeds limit).

### `Task SendPushAsync()`

Sends a notification via push notification channel.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `InvalidOperationException` if the notification is in an invalid state (e.g., `Status` is `Failed` and `RetryCount` exceeds limit).

### `Task SendMultiChannelAsync()`

Attempts to send the notification via all supported channels (email, SMS, push) in parallel. Marks the notification as `Delivered` only if all channels succeed; otherwise, marks as `Failed` and increments `RetryCount`.

- **Parameters**: None.
- **Return value**: `Task` representing the asynchronous operation.
- **Exceptions**: Throws `InvalidOperationException` if the notification is in an invalid state (e.g., `Status` is `Failed` and `RetryCount` exceeds limit).

### `static IEnumerable<NotificationItem> GetPendingNotifications()`

Retrieves all notifications that are pending delivery (i.e., `Status` is `Pending` or `Retrying`).

- **Parameters**: None.
- **Return value**: `IEnumerable<NotificationItem>` of pending notifications.
- **Exceptions**: None.

### `Guid Id`

Unique identifier for the notification. Assigned at creation and immutable.

### `NotificationType Type`

Type of notification (e.g., `Alert`, `Reminder`, `System`). Used to determine routing and formatting.

### `string Recipient`

Destination address or identifier for the recipient (e.g., email address, phone number, push token).

### `string? Subject`

Optional subject line for email or title for push notifications.

### `string Body`

Content of the notification. May be plain text or HTML depending on channel.

### `DateTime CreatedAt`

Timestamp when the notification was created. Set once and immutable.

### `NotificationStatus Status`

Current state of the notification (`Pending`, `Sending`, `Delivered`, `Failed`, `Retrying`).

### `int RetryCount`

Number of times delivery has been attempted. Incremented on each failure.

### `string? ErrorMessage`

Optional error message from the most recent delivery attempt. Cleared on success.

## Usage

### Example 1: Sending a multi-channel notification
