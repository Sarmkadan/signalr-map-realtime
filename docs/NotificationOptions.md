# NotificationOptions

Configuration class that controls how real-time notifications are generated and delivered in the `signalr-map-realtime` project. It aggregates settings for email, SMS, and push notifications, along with global delivery parameters such as retry behavior and asynchronous processing.

## API

### Properties

#### `Enabled`
- **Purpose**: Determines whether notification delivery is active for the application.
- **Type**: `bool`
- **Default**: `true`

#### `Email`
- **Purpose**: Contains SMTP and sender configuration for email notifications.
- **Type**: `EmailNotificationOptions`
- **Default**: New instance of `EmailNotificationOptions`

#### `Sms`
- **Purpose**: Contains API and provider settings for SMS notifications.
- **Type**: `SmsNotificationOptions`
- **Default**: New instance of `SmsNotificationOptions`

#### `Push`
- **Purpose**: Contains settings for push notification delivery via platform services.
- **Type**: `PushNotificationOptions`
- **Default**: New instance of `PushNotificationOptions`

#### `DefaultSender`
- **Purpose**: Specifies the default sender identifier used when no explicit sender is provided in a notification.
- **Type**: `string`
- **Default**: `null`

#### `UseAsyncProcessing`
- **Purpose**: Indicates whether notifications should be processed asynchronously via a background queue.
- **Type**: `bool`
- **Default**: `true`

#### `MaxRetries`
- **Purpose**: Maximum number of retry attempts for failed notification deliveries.
- **Type**: `int`
- **Default**: `3`
- **Constraints**: Must be non-negative.

#### `RetryDelaySeconds`
- **Purpose**: Delay in seconds between retry attempts for failed notifications.
- **Type**: `int`
- **Default**: `5`
- **Constraints**: Must be non-negative.

#### `SmtpHost`
- **Purpose**: Hostname or IP address of the SMTP server used for sending email notifications.
- **Type**: `string`
- **Default**: `null`

#### `SmtpPort`
- **Purpose**: Port number for the SMTP server.
- **Type**: `int`
- **Default**: `587`

#### `SmtpUsername`
- **Purpose**: Username for authenticating with the SMTP server.
- **Type**: `string`
- **Default**: `null`

#### `SmtpPassword`
- **Purpose**: Password for authenticating with the SMTP server.
- **Type**: `string`
- **Default**: `null`

#### `UsesTls`
- **Purpose**: Indicates whether TLS encryption should be used when connecting to the SMTP server.
- **Type**: `bool`
- **Default**: `true`

#### `FromAddress`
- **Purpose**: Email address used as the "From" field in sent emails.
- **Type**: `string`
- **Default**: `null`

#### `FromDisplayName`
- **Purpose**: Display name shown in the "From" field of sent emails.
- **Type**: `string`
- **Default**: `null`

#### `TemplateDirectory`
- **Purpose**: Filesystem path to the directory containing notification email templates.
- **Type**: `string`
- **Default**: `"./Templates/Email"`

#### `ApiKey`
- **Purpose**: API key for authenticating with the SMS provider.
- **Type**: `string`
- **Default**: `null`

#### `ApiSecret`
- **Purpose**: API secret for authenticating with the SMS provider.
- **Type**: `string`
- **Default**: `null`

## Usage

### Example 1: Basic Configuration
