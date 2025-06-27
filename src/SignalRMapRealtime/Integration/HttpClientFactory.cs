// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Integration;

using System.Net.Http.Headers;

/// <summary>
/// Factory for creating and configuring HttpClient instances for external API calls.
/// Manages connection pooling, timeouts, retry policies, and headers.
/// Provides a centralized place for HTTP client configuration.
/// </summary>
public interface IHttpClientFactory
{
    /// <summary>
    /// Gets or creates an HTTP client for a specific service.
    /// </summary>
    HttpClient GetClient(string serviceName);

    /// <summary>
    /// Creates a new HTTP client with custom configuration.
    /// </summary>
    HttpClient CreateClient(string serviceName, TimeSpan timeout, Dictionary<string, string>? defaultHeaders = null);
}

/// <summary>
/// Implementation of HTTP client factory with built-in service clients.
/// </summary>
public class ExternalHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalHttpClientFactory> _logger;
    private readonly Dictionary<string, HttpClient> _clients = new();

    public ExternalHttpClientFactory(HttpClient httpClient, ILogger<ExternalHttpClientFactory> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        InitializeDefaultClients();
    }

    /// <summary>
    /// Gets an HTTP client for a specific external service.
    /// </summary>
    public HttpClient GetClient(string serviceName)
    {
        if (_clients.TryGetValue(serviceName, out var client))
            return client;

        _logger.LogWarning("No configured client for service {ServiceName}. Using default client.", serviceName);
        return _httpClient;
    }

    /// <summary>
    /// Creates a new HTTP client with custom timeout and headers.
    /// </summary>
    public HttpClient CreateClient(string serviceName, TimeSpan timeout, Dictionary<string, string>? defaultHeaders = null)
    {
        var client = new HttpClient
        {
            Timeout = timeout
        };

        // Add default headers
        client.DefaultRequestHeaders.Add("User-Agent", "SignalRMapRealtime/1.0");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        if (defaultHeaders != null)
        {
            foreach (var header in defaultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        _clients[serviceName] = client;
        _logger.LogInformation("HTTP client created for service {ServiceName}", serviceName);

        return client;
    }

    /// <summary>
    /// Initializes default HTTP clients for known external services.
    /// </summary>
    private void InitializeDefaultClients()
    {
        // Google Maps API client
        CreateClient(
            "GoogleMaps",
            TimeSpan.FromSeconds(10),
            new Dictionary<string, string>
            {
                { "Accept", "application/json" }
            });

        // OpenStreetMap API client
        CreateClient(
            "OpenStreetMap",
            TimeSpan.FromSeconds(10),
            new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "User-Agent", "SignalRMapRealtime/1.0 (+https://sarmkadan.com)" }
            });

        // Webhook dispatch client for pushing to external services
        CreateClient(
            "WebhookDispatcher",
            TimeSpan.FromSeconds(30),
            new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            });

        // Notification service client
        CreateClient(
            "NotificationService",
            TimeSpan.FromSeconds(15),
            new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            });

        _logger.LogInformation("HTTP clients initialized for default external services");
    }
}

/// <summary>
/// Extension methods for registering the HTTP client factory.
/// </summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>
    /// Adds the HTTP client factory to the service collection.
    /// </summary>
    public static IServiceCollection AddExternalHttpClientFactory(this IServiceCollection services)
    {
        services.AddHttpClient<ExternalHttpClientFactory>();
        services.AddScoped<IHttpClientFactory>(sp => sp.GetRequiredService<ExternalHttpClientFactory>());
        return services;
    }
}
