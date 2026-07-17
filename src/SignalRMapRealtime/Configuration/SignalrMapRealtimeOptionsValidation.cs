#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace SignalRMapRealtime.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="SignalrMapRealtimeOptions"/> configuration.
/// </summary>
internal static class SignalrMapRealtimeOptionsValidation
{
    /// <summary>
    /// Validates the SignalR Map Realtime configuration options.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate AppInfo
        if (value.AppInfo is null)
        {
            problems.Add("AppInfo section is required but was null.");
        }
        else
        {
            problems.AddRange(value.AppInfo.Validate());
        }

        // Validate HealthChecks
        if (value.HealthChecks is null)
        {
            problems.Add("HealthChecks section is required but was null.");
        }
        else
        {
            problems.AddRange(value.HealthChecks.Validate());
        }

        // Validate ApiKeyAuthentication
        if (value.ApiKeyAuthentication is null)
        {
            problems.Add("ApiKeyAuthentication section is required but was null.");
        }
        else
        {
            problems.AddRange(value.ApiKeyAuthentication.Validate());
        }

        // Validate Performance
        if (value.Performance is null)
        {
            problems.Add("Performance section is required but was null.");
        }
        else
        {
            problems.AddRange(value.Performance.Validate());
        }

        // Validate SignalRHubs
        if (value.SignalRHubs is null)
        {
            problems.Add("SignalRHubs section is required but was null.");
        }
        else
        {
            problems.AddRange(value.SignalRHubs.Validate());
        }

        // Validate WebSockets
        if (value.WebSockets is null)
        {
            problems.Add("WebSockets section is required but was null.");
        }
        else
        {
            problems.AddRange(value.WebSockets.Validate());
        }

        // Validate BackgroundJobs
        if (value.BackgroundJobs is null)
        {
            problems.Add("BackgroundJobs section is required but was null.");
        }
        else
        {
            problems.AddRange(value.BackgroundJobs.Validate());
        }

        // Validate Security
        if (value.Security is null)
        {
            problems.Add("Security section is required but was null.");
        }
        else
        {
            problems.AddRange(value.Security.Validate());
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the SignalR Map Realtime configuration options are valid.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>True if all validation rules pass; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SignalrMapRealtimeOptions value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Validates and ensures the SignalR Map Realtime configuration options are valid.
    /// Throws an <see cref="ArgumentException"/> with detailed validation messages if any problems are found.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing detailed error messages.</exception>
    public static void EnsureValid(this SignalrMapRealtimeOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SignalR Map Realtime configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates the AppInfoOptions configuration section.
    /// </summary>
    /// <param name="value">The AppInfoOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.AppInfoOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ApiVersion))
        {
            problems.Add("AppInfo.ApiVersion is required and cannot be empty or whitespace.");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(value.ApiVersion, @"^\d+\.\d+\.\d+$"))
        {
            problems.Add("AppInfo.ApiVersion must follow semantic versioning format (MAJOR.MINOR.PATCH).");
        }

        if (string.IsNullOrWhiteSpace(value.ApiTitle))
        {
            problems.Add("AppInfo.ApiTitle is required and cannot be empty or whitespace.");
        }
        else if (value.ApiTitle.Length < 3)
        {
            problems.Add("AppInfo.ApiTitle must be at least 3 characters long.");
        }
        else if (value.ApiTitle.Length > 100)
        {
            problems.Add("AppInfo.ApiTitle must be 100 characters or less.");
        }

        if (string.IsNullOrWhiteSpace(value.Environment))
        {
            problems.Add("AppInfo.Environment is required and cannot be empty or whitespace.");
        }
        else if (value.Environment != "Development" && value.Environment != "Staging" && value.Environment != "Production")
        {
            problems.Add("AppInfo.Environment must be Development, Staging, or Production.");
        }

        if (value.RequestTimeoutSeconds < 5 || value.RequestTimeoutSeconds > 300)
        {
            problems.Add("AppInfo.RequestTimeoutSeconds must be between 5 and 300 seconds.");
        }

        if (value.LocationUpdateIntervalSeconds < 1 || value.LocationUpdateIntervalSeconds > 600)
        {
            problems.Add("AppInfo.LocationUpdateIntervalSeconds must be between 1 and 600 seconds.");
        }

        if (value.MaxPayloadSizeKb < 1 || value.MaxPayloadSizeKb > 10240)
        {
            problems.Add("AppInfo.MaxPayloadSizeKb must be between 1 and 10240 KB.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the HealthCheckOptions configuration section.
    /// </summary>
    /// <param name="value">The HealthCheckOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.HealthCheckOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.TimeoutSeconds < 1 || value.TimeoutSeconds > 60)
        {
            problems.Add("HealthChecks.TimeoutSeconds must be between 1 and 60 seconds.");
        }

        if (string.IsNullOrWhiteSpace(value.MinimumStatus))
        {
            problems.Add("HealthChecks.MinimumStatus is required and cannot be empty or whitespace.");
        }
        else if (value.MinimumStatus != "Healthy" && value.MinimumStatus != "Degraded" && value.MinimumStatus != "Unhealthy")
        {
            problems.Add("HealthChecks.MinimumStatus must be Healthy, Degraded, or Unhealthy.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the ApiKeyAuthenticationOptions configuration section.
    /// </summary>
    /// <param name="value">The ApiKeyAuthenticationOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.HeaderName))
        {
            problems.Add("ApiKeyAuthentication.HeaderName is required and cannot be empty or whitespace.");
        }
        else if (value.HeaderName.Length < 3)
        {
            problems.Add("ApiKeyAuthentication.HeaderName must be at least 3 characters long.");
        }
        else if (value.HeaderName.Length > 50)
        {
            problems.Add("ApiKeyAuthentication.HeaderName must be 50 characters or less.");
        }

        if (value.ExemptedEndpoints is null)
        {
            problems.Add("ApiKeyAuthentication.ExemptedEndpoints is required but was null.");
        }
        else if (value.ExemptedEndpoints.Any(string.IsNullOrWhiteSpace))
        {
            problems.Add("ApiKeyAuthentication.ExemptedEndpoints contains empty or whitespace entries.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the PerformanceOptions configuration section.
    /// </summary>
    /// <param name="value">The PerformanceOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.PerformanceOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxConcurrentConnections < 100 || value.MaxConcurrentConnections > 100000)
        {
            problems.Add("Performance.MaxConcurrentConnections must be between 100 and 100000.");
        }

        if (value.RequestQueueLimit < 10 || value.RequestQueueLimit > 10000)
        {
            problems.Add("Performance.RequestQueueLimit must be between 10 and 10000.");
        }

        if (value.MaxProcessingTimeSeconds < 5 || value.MaxProcessingTimeSeconds > 300)
        {
            problems.Add("Performance.MaxProcessingTimeSeconds must be between 5 and 300 seconds.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the SignalRHubOptions configuration section.
    /// </summary>
    /// <param name="value">The SignalRHubOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.SignalRHubOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxConnectionsPerHub < 100 || value.MaxConnectionsPerHub > 50000)
        {
            problems.Add("SignalRHubs.MaxConnectionsPerHub must be between 100 and 50000.");
        }

        if (value.UseBackplane && string.IsNullOrWhiteSpace(value.BackplaneConnectionString))
        {
            problems.Add("SignalRHubs.BackplaneConnectionString is required when UseBackplane is true.");
        }
        else if (value.UseBackplane && value.BackplaneConnectionString?.Length > 500)
        {
            problems.Add("SignalRHubs.BackplaneConnectionString must be 500 characters or less.");
        }

        if (value.ReconnectTimeoutSeconds < 5 || value.ReconnectTimeoutSeconds > 120)
        {
            problems.Add("SignalRHubs.ReconnectTimeoutSeconds must be between 5 and 120 seconds.");
        }

        if (value.MaxMessageSizeKb < 1 || value.MaxMessageSizeKb > 1024)
        {
            problems.Add("SignalRHubs.MaxMessageSizeKb must be between 1 and 1024 KB.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the WebSocketOptions configuration section.
    /// </summary>
    /// <param name="value">The WebSocketOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.WebSocketOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.KeepAliveIntervalSeconds < 10 || value.KeepAliveIntervalSeconds > 300)
        {
            problems.Add("WebSockets.KeepAliveIntervalSeconds must be between 10 and 300 seconds.");
        }

        if (value.MaxMessageSizeKb < 1 || value.MaxMessageSizeKb > 1024)
        {
            problems.Add("WebSockets.MaxMessageSizeKb must be between 1 and 1024 KB.");
        }

        if (value.EnablePingPong && (value.PingIntervalSeconds < 15 || value.PingIntervalSeconds > 120))
        {
            problems.Add("WebSockets.PingIntervalSeconds must be between 15 and 120 seconds when EnablePingPong is true.");
        }

        if (value.MaxConnectionDurationHours < 1 || value.MaxConnectionDurationHours > 24)
        {
            problems.Add("WebSockets.MaxConnectionDurationHours must be between 1 and 24 hours.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the BackgroundJobsOptions configuration section.
    /// </summary>
    /// <param name="value">The BackgroundJobsOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.BackgroundJobsOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxConcurrentWorkers < 1 || value.MaxConcurrentWorkers > 20)
        {
            problems.Add("BackgroundJobs.MaxConcurrentWorkers must be between 1 and 20.");
        }

        if (value.SessionCleanupIntervalMinutes < 1 || value.SessionCleanupIntervalMinutes > 1440)
        {
            problems.Add("BackgroundJobs.SessionCleanupIntervalMinutes must be between 1 and 1440 minutes.");
        }

        if (value.CacheCleanupIntervalMinutes < 5 || value.CacheCleanupIntervalMinutes > 1440)
        {
            problems.Add("BackgroundJobs.CacheCleanupIntervalMinutes must be between 5 and 1440 minutes.");
        }

        if (value.DatabaseMaintenanceIntervalHours < 1 || value.DatabaseMaintenanceIntervalHours > 168)
        {
            problems.Add("BackgroundJobs.DatabaseMaintenanceIntervalHours must be between 1 and 168 hours.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the SecurityOptions configuration section.
    /// </summary>
    /// <param name="value">The SecurityOptions to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    private static IReadOnlyList<string> Validate(this SignalrMapRealtimeOptions.SecurityOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.ContentSecurityPolicy?.Length > 1000)
        {
            problems.Add("Security.ContentSecurityPolicy must be 1000 characters or less.");
        }

        return problems.AsReadOnly();
    }
}