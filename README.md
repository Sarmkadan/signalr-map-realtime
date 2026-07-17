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

## ThrottleOptions

`ThrottleOptions` configures per‑asset‑type throttling for location updates, allowing you to limit how frequently each type of asset can broadcast its position. This helps reduce unnecessary SignalR traffic for slow‑moving or stationary assets while still keeping fast‑moving assets responsive.

### Usage Example

```csharp
using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Domain.Enums;

// Create a throttle configuration with custom intervals
var throttleOptions = new ThrottleOptions
{
    Enabled = true,
    DeliveryVanIntervalSeconds = 2,
    CourierIntervalSeconds = 12,
    BicycleIntervalSeconds = 20,
    MotorcycleIntervalSeconds = 5,
    PortableIntervalSeconds = 30,
    FixedAssetIntervalSeconds = 300,
    DroneIntervalSeconds = 1
};

// Get the minimum interval for a specific asset type
TimeSpan courierInterval = throttleOptions.GetIntervalForAssetType(AssetType.Courier);
Console.WriteLine($"Courier updates are throttled to one every {courierInterval.TotalSeconds} seconds.");
```

// ... (rest of the file remains unchanged)
