# IHttpClientFactory

Provides a factory abstraction for creating and managing `HttpClient` instances with preconfigured settings for external service communication in the SignalR map realtime application. It centralizes HTTP client lifecycle management, ensuring proper disposal, connection pooling, and consistent configuration across the application.

## API

### ExternalHttpClientFactory

```csharp
public ExternalHttpClientFactory(IServiceProvider serviceProvider, IOptions<ExternalHttpClientOptions> options)
```

Initializes a new instance of the factory with the specified service provider and configuration options.

**Parameters**
- `serviceProvider`: The application service provider for resolving scoped dependencies.
- `options`: Configuration options containing base addresses, timeouts, and default headers for external HTTP clients.

**Exceptions**
- `ArgumentNullException`: Thrown if `serviceProvider` or `options` is null.

---

### GetClient

```csharp
public HttpClient GetClient(string name)
```

Retrieves a named `HttpClient` instance configured for a specific external service.

**Parameters**
- `name`: The logical name of the client configuration (e.g., "geocoding", "tiles", "routing").

**Returns**
- An `HttpClient` instance preconfigured with the base address, timeout, and default headers associated with the given name.

**Exceptions**
- `ArgumentException`: Thrown if `name` is null, empty, or not registered in the configuration.
- `InvalidOperationException`: Thrown if the underlying `IHttpClientFactory` cannot create the client.

---

### CreateClient

```csharp
public HttpClient CreateClient(Action<HttpClient> configureClient = null)
```

Creates a new `HttpClient` instance with optional additional configuration.

**Parameters**
- `configureClient`: An optional delegate to further customize the client (e.g., adding request headers, setting timeout).

**Returns**
- A new `HttpClient` instance with the factory's default configuration applied, plus any customizations from `configureClient`.

**Exceptions**
- `InvalidOperationException`: Thrown if the default client configuration is missing or invalid.

---

### AddExternalHttpClientFactory

```csharp
public static IServiceCollection AddExternalHttpClientFactory(this IServiceCollection services, Action<ExternalHttpClientOptions> configureOptions)
```

Registers the `ExternalHttpClientFactory` and its dependencies in the dependency injection container.

**Parameters**
- `services`: The service collection to add the registration to.
- `configureOptions`: A delegate to configure `ExternalHttpClientOptions` with named client settings.

**Returns**
- The same `IServiceCollection` instance for chaining.

**Exceptions**
- `ArgumentNullException`: Thrown if `services` or `configureOptions` is null.

## Usage

### Registering and configuring the factory at startup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddExternalHttpClientFactory(options =>
    {
        options.Clients["geocoding"] = new ExternalHttpClientSettings
        {
            BaseAddress = new Uri("https://api.geocoding.example.com/"),
            Timeout = TimeSpan.FromSeconds(10),
            DefaultHeaders = { { "User-Agent", "SignalRMap/1.0" } }
        };
        options.Clients["tiles"] = new ExternalHttpClientSettings
        {
            BaseAddress = new Uri("https://tiles.example.com/"),
            Timeout = TimeSpan.FromSeconds(5)
        };
    });

    services.AddControllers();
}
```

### Injecting and using the factory in a service

```csharp
public class MapTileService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MapTileService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<byte[]> GetTileAsync(int x, int y, int zoom, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.GetClient("tiles");
        var response = await client.GetAsync($"{zoom}/{x}/{y}.png", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<GeocodeResult> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        using var client = _httpClientFactory.CreateClient(c =>
        {
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        var response = await client.GetAsync($"search?q={Uri.EscapeDataString(address)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GeocodeResult>(cancellationToken: cancellationToken);
    }
}
```

## Notes

- **Thread safety**: The factory itself is thread-safe. `HttpClient` instances returned by `GetClient` and `CreateClient` are safe for concurrent use, but callers should not dispose clients obtained via `GetClient` as their lifetime is managed by the factory. Clients from `CreateClient` should be disposed by the caller.
- **Connection pooling**: `GetClient` reuses pooled connections for the same named client. `CreateClient` creates a new client instance each call; prefer `GetClient` for repeated calls to the same service.
- **Configuration validation**: `AddExternalHttpClientFactory` validates that at least one named client is configured. An empty configuration throws at service build time.
- **Scoped dependencies**: The factory captures the root `IServiceProvider` at construction. If scoped services are required inside `configureClient` delegate, resolve them via `IServiceScopeFactory` manually.
- **Disposal**: The factory implements `IDisposable` and disposes all internally managed `HttpClient` instances. Ensure the factory is registered as a singleton to avoid premature disposal.
