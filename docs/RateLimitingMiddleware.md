# RateLimitingMiddleware

The `RateLimitingMiddleware` component provides request throttling capabilities for the `signalr-map-realtime` application by intercepting incoming HTTP requests within the ASP.NET Core pipeline. It evaluates request frequency against configured constraints and either allows the request to proceed to the next delegate or short-circuits the pipeline with a rate-limit response. This middleware is essential for protecting real-time map resources from excessive polling or connection attempts, ensuring system stability under high load.

## API

### `public RateLimitingMiddleware`
Initializes a new instance of the `RateLimitingMiddleware` class. This constructor is invoked by the dependency injection container or the middleware extension method to instantiate the component before it begins processing requests. It typically accepts required services such as logging, configuration, or rate-limit policy providers via injection, though specific parameters are resolved internally by the framework's activation system.

### `public async Task InvokeAsync`
Executes the middleware logic for an individual HTTP request. This method inspects the current context to determine if the client has exceeded their allocated request quota.
*   **Parameters**: Accepts an `HttpContext` instance representing the current request and response, and a `RequestDelegate` representing the next middleware in the pipeline.
*   **Return Value**: Returns a `Task` that completes when the request processing is finished. If the request is allowed, it awaits the next delegate; if rate-limited, it completes after writing the appropriate error response.
*   **Exceptions**: Throws `ArgumentNullException` if the context or next delegate is null. May throw exceptions propagated from downstream middleware or specific rate-limiting policy violations depending on the configured strategy.

### `public static IApplicationBuilder UseRateLimiting`
Extension method used to register the `RateLimitingMiddleware` into the ASP.NET Core request pipeline.
*   **Parameters**: Accepts an `IApplicationBuilder` instance on which the middleware is being configured. Optional parameters may include specific rate-limiting options or policies.
*   **Return Value**: Returns the same `IApplicationBuilder` instance to allow for fluent chaining of further middleware configurations.
*   **Exceptions**: Throws `ArgumentNullException` if the provided application builder is null.

### `public static long ToUnixTimestamp`
Utility method that converts a `DateTime` or `DateTimeOffset` value into a Unix timestamp representation.
*   **Parameters**: Accepts a `DateTime` or `DateTimeOffset` struct representing the point in time to convert.
*   **Return Value**: Returns a `long` integer representing the number of seconds elapsed since the Unix epoch (January 1, 1970, 00:00:00 UTC).
*   **Exceptions**: Throws `ArgumentOutOfRangeException` if the provided date value is prior to the Unix epoch or exceeds the maximum representable Unix timestamp.

## Usage

### Registering the Middleware
The middleware is typically added in the `Configure` method of the `Startup` class or within the `Program.cs` file of a minimal API setup. It should be placed early in the pipeline, usually before authentication and endpoint mapping, to ensure unauthorized or excessive traffic is filtered efficiently.

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Register rate limiting before mapping endpoints
app.UseRateLimiting();

app.MapHub<MapHub>("/hubs/map");
app.Run();
```

### Customizing Pipeline Order
In scenarios where rate limiting needs to apply only to specific branches of the pipeline or requires custom configuration options passed during registration, the extension method can be chained with other middleware components explicitly.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    // Apply rate limiting specifically to the realtime map paths
    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/map"),
        appBuilder =>
        {
            appBuilder.UseRateLimiting();
        }
    );

    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```

## Notes

*   **Thread Safety**: The `InvokeAsync` method is designed to be thread-safe and stateless regarding the middleware instance itself, allowing the ASP.NET Core runtime to process concurrent requests against a single singleton instance. However, any stateful services injected into the constructor (such as distributed caches or counters) must themselves be thread-safe.
*   **Timestamp Precision**: The `ToUnixTimestamp` method returns values in seconds. Systems requiring millisecond precision for fine-grained rate limiting windows should account for this truncation when calculating time deltas.
*   **Pipeline Ordering**: Placing `UseRateLimiting` after `UseAuthentication` may result in unnecessary resource consumption for anonymous flood attacks. Conversely, placing it before essential security headers or logging middleware might obscure audit trails for rejected requests; careful positioning relative to `UseRouting` is recommended to ensure path-based policies resolve correctly.
*   **Edge Cases**: If the system clock on the host machine drifts significantly, the Unix timestamp calculations may yield inconsistent rate-limit windows. Reliance on synchronized time sources (NTP) is critical for distributed deployments where multiple instances share a rate-limit store.
