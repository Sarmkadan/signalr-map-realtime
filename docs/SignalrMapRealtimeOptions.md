# SignalrMapRealtimeOptions

`SignalrMapRealtimeOptions` serves as the central configuration object for the `signalr-map-realtime` service. It aggregates all tunable settings—ranging from authentication, health checks, and SignalR hub behavior to performance thresholds and environmental metadata—into a single validated options class. An instance of this type is typically bound from application configuration and injected into the service pipeline to control runtime behavior.

## API

### AppInfo
`public AppInfoOptions AppInfo`

Gets or sets the application metadata options, such as name, version, and build information. This object is used to populate diagnostic and info endpoints.

### HealthChecks
`public HealthCheckOptions HealthChecks`

Gets or sets the configuration for health-check endpoints. Controls which probes are enabled, their paths, and timeout policies.

### ApiKeyAuthentication
`public ApiKeyAuthenticationOptions ApiKeyAuthentication`

Gets or sets the API key authentication settings. Defines header names, expected keys, and validation rules for securing endpoints.

### Performance
`public PerformanceOptions Performance`

Gets or sets performance-related thresholds, such as maximum concurrent connections, throttling limits, and buffer sizes.

### SignalRHubs
`public SignalRHubOptions SignalRHubs`

Gets or sets the SignalR hub configuration, including hub route patterns, protocol settings, and keep-alive intervals.

### WebSockets
`public WebSocketOptions WebSockets`

Gets or sets WebSocket transport options, such as allowed origins, receive buffer sizes, and keep-alive frequency.

### BackgroundJobs
`public BackgroundJobsOptions BackgroundJobs`

Gets or sets the background job processing configuration. Controls job queues, retry policies, and scheduling intervals.

### Security
`public SecurityOptions Security`

Gets or sets security constraints, including CORS origins, CSP headers, and TLS enforcement flags.

### Validate
`public bool Validate`

When `true`, instructs the options framework to perform strict validation on all nested option objects at startup. Validation failures will prevent the application from starting.

### ApiVersion
`public string ApiVersion`

Gets or sets the API version string reported in response headers and Swagger documents. Must conform to a standard version format (e.g., `"1.0"`).

### ApiTitle
`public string ApiTitle`

Gets or sets the human-readable API title displayed in Swagger UI and OpenAPI metadata.

### Environment
`public string Environment`

Gets or sets the deployment environment name (e.g., `"Development"`, `"Staging"`, `"Production"`). Used to toggle environment-specific behaviors.

### EnableSwagger
`public bool EnableSwagger`

When `true`, Swagger UI and the OpenAPI endpoint are registered in the middleware pipeline.

### EnableCors
`public bool EnableCors`

When `true`, Cross-Origin Resource Sharing middleware is activated using the rules defined in `Security` and `WebSockets`.

### RequestTimeoutSeconds
`public int RequestTimeoutSeconds`

Gets or sets the HTTP request timeout in seconds. Applies to incoming API calls. A value of `0` indicates no explicit timeout.

### LocationUpdateIntervalSeconds
`public int LocationUpdateIntervalSeconds`

Gets or sets the minimum interval in seconds between location update broadcasts to connected SignalR clients. Lower values increase message frequency.

### MaxPayloadSizeKb
`public int MaxPayloadSizeKb`

Gets or sets the maximum allowed payload size in kilobytes for incoming SignalR messages and API requests. Messages exceeding this limit are rejected.

### Enabled
`public bool Enabled`

Master toggle for the service. When `false`, core processing loops and hub connections are disabled, though health checks may still respond.

### TimeoutSeconds
`public int TimeoutSeconds`

Gets or sets a general-purpose timeout in seconds used by internal operations such as connection establishment and background job execution.

### MinimumStatus
`public string MinimumStatus`

Gets or sets the minimum operational status required for the service to report itself as healthy. Typical values are `"Healthy"`, `"Degraded"`, or `"Unhealthy"`.

## Usage

### Example 1: Binding from Configuration and Starting the Service

```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

// Bind the entire SignalrMapRealtimeOptions section from appsettings.json
builder.Services
    .AddOptions<SignalrMapRealtimeOptions>()
    .Bind(builder.Configuration.GetSection("SignalrMapRealtime"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Read back the configured options to conditionally enable Swagger
var options = app.Services.GetRequiredService<IOptions<SignalrMapRealtimeOptions>>().Value;
if (options.EnableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.DocumentTitle = options.ApiTitle);
}

app.MapSignalRHubs(options.SignalRHubs);
app.Run();
```

### Example 2: Programmatic Construction for Testing

```csharp
// Constructing options in a unit test or integration test harness
var testOptions = new SignalrMapRealtimeOptions
{
    Enabled = true,
    Environment = "Testing",
    ApiVersion = "2.0-preview",
    ApiTitle = "SignalR Map Realtime Test API",
    EnableSwagger = false,
    EnableCors = false,
    RequestTimeoutSeconds = 30,
    LocationUpdateIntervalSeconds = 5,
    MaxPayloadSizeKb = 256,
    Validate = true,
    ApiKeyAuthentication = new ApiKeyAuthenticationOptions
    {
        HeaderName = "X-Api-Key",
        Keys = new[] { "test-key-123" }
    },
    HealthChecks = new HealthCheckOptions
    {
        Enabled = true,
        Endpoint = "/healthz"
    },
    Security = new SecurityOptions
    {
        AllowedOrigins = new[] { "https://localhost" }
    }
};

// Validate manually before injecting into the service
if (testOptions.Validate)
{
    var validationContext = new ValidationContext(testOptions);
    Validator.ValidateObject(testOptions, validationContext, validateAllProperties: true);
}

var service = new MapRealtimeService(testOptions);
await service.StartAsync(CancellationToken.None);
```

## Notes

- **Validation ordering**: When `Validate` is `true`, validation runs on the root object and recursively on all nested option types (`AppInfoOptions`, `HealthCheckOptions`, etc.). Any `DataAnnotations` attributes on those nested types are evaluated. A validation failure throws an `OptionsValidationException` during startup, preventing the application from entering a misconfigured state.
- **Thread safety**: `SignalrMapRealtimeOptions` is designed as a configuration snapshot. Once bound and validated at startup, it is treated as immutable during the lifetime of the application. Reading properties from multiple threads is safe; modifying properties after the service has started is not supported and may lead to inconsistent behavior.
- **Timeout interactions**: `RequestTimeoutSeconds` and `TimeoutSeconds` serve different scopes. The former applies exclusively to HTTP request pipelines, while the latter is a fallback for internal operations. Setting both to `0` disables explicit timeouts, which may cause indefinite hangs if external dependencies are unresponsive.
- **`MinimumStatus` parsing**: The `MinimumStatus` string is compared ordinally against known health-check statuses. An unrecognized value defaults to `"Healthy"`, meaning the service will report healthy even if a stricter status was intended. Ensure the value matches one of the standard status literals.
- **Payload size enforcement**: `MaxPayloadSizeKb` is enforced at the transport level for SignalR messages and at the middleware level for API requests. Messages that exceed the limit are rejected with a `413 Payload Too Large` response or a hub-level disconnect, depending on the protocol.
- **`Enabled` flag scope**: Setting `Enabled` to `false` stops location processing loops and prevents new SignalR connections from being established. Existing connections are drained gracefully. Health-check endpoints remain active unless explicitly disabled in `HealthChecks`.
