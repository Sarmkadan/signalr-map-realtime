// ... (rest of the file remains unchanged)

## SignalrMapRealtimeOptions

The `SignalrMapRealtimeOptions` class serves as the root configuration options for the SignalR Map Realtime application, consolidating all individual option classes into a single root configuration object. This allows for centralized management of application settings.

### Usage Example

```csharp
using SignalRMapRealtime.Configuration;

// Access and configure SignalrMapRealtimeOptions
var signalrMapRealtimeOptions = new SignalrMapRealtimeOptions
{
    AppInfo = new AppInfoOptions
    {
        ApiVersion = "2.0.0",
        ApiTitle = "SignalR Map Realtime API",
        Environment = "Production",
        EnableSwagger = true,
        EnableCors = true,
        RequestTimeoutSeconds = 30,
        LocationUpdateIntervalSeconds = 30,
        MaxPayloadSizeKb = 1024,
    },
    HealthChecks = new HealthCheckOptions
    {
        Enabled = true,
        TimeoutSeconds = 5,
        MinimumStatus = "Healthy"
    },
    ApiKeyAuthentication = new ApiKeyAuthenticationOptions
    {
        Enabled = true,
        HeaderName = "X-API-Key",
        Required = true
    },
    Performance = new PerformanceOptions
    {
        EnableDetailedMetrics = true,
        MaxConcurrentConnections = 10000,
        RequestQueueLimit = 1000
    },
    SignalRHubs = new SignalRHubOptions
    {
        Enabled = true,
        MaxConnectionsPerHub = 10000,
    },
    WebSockets = new WebSocketOptions
    {
        Enabled = true,
        KeepAliveIntervalSeconds = 30,
    },
    BackgroundJobs = new BackgroundJobsOptions
    {
        Enabled = true,
        MaxConcurrentWorkers = 5,
    },
    Security = new SecurityOptions
    {
        EnableHttpsRedirection = true,
        EnableRequestLogging = true,
    }
};

// Validate the configuration
if (!signalrMapRealtimeOptions.Validate(out var validationResults))
{
    // Handle validation errors
    foreach (var result in validationResults)
    {
        Console.WriteLine(result.ErrorMessage);
    }
}
```

// ... (rest of the file remains unchanged)
