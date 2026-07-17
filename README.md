// ... (rest of the file remains unchanged)

## NotificationOptions

The `NotificationOptions` class represents configuration settings for various notification channels, including email, SMS, and push notifications. It allows centralized management of notification preferences and behaviors.

### Usage Example

```csharp
using SignalRMapRealtime.Configuration;

// Access and configure notification options
var notificationOptions = new NotificationOptions
{
    Enabled = true,
    DefaultSender = "noreply@example.com",
    UseAsyncProcessing = true,
    MaxRetries = 3,
    RetryDelaySeconds = 5,
    Email = new EmailNotificationOptions
    {
        Enabled = true,
        SmtpHost = "smtp.example.com",
        SmtpPort = 587,
        SmtpUsername = "sender@example.com",
        SmtpPassword = "password123",
        UsesTls = true,
        FromAddress = "noreply@example.com",
        FromDisplayName = "Example Service",
        TemplateDirectory = "/templates/email"
    },
    Sms = new SmsNotificationOptions
    {
        Enabled = true,
        ApiKey = "sms_api_key",
        ApiSecret = "sms_api_secret",
        SenderId = "+1234567890"
    },
    Push = new PushNotificationOptions
    {
        Enabled = true,
        Provider = "Firebase",
        FirebaseProjectId = "firebase_project_id",
        FirebaseCredentialsPath = "/path/to/firebase/credentials.json"
    }
};
```

// ... (rest of the file remains unchanged)
