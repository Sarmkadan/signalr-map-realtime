using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using SignalRMapRealtime.Configuration;
using Xunit;

namespace SignalRMapRealtime.Tests.Configuration;

/// <summary>
/// Unit tests for SignalrMapRealtimeOptions configuration validation.
/// </summary>
public class SignalrMapRealtimeOptionsTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.example.json", optional: true)
            .Build();

        var options = new SignalrMapRealtimeOptions();

        // Act
        var isValid = options.Validate(out var validationResults);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validate_WithInvalidApiVersion_ReturnsFalse()
    {
        // Arrange
        var options = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "invalid-version",
                ApiTitle = "Test API",
                Environment = "Production"
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions(),
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions(),
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions(),
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions(),
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions(),
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions(),
            Security = new SignalrMapRealtimeOptions.SecurityOptions()
        };

        // Act
        var isValid = options.Validate(out var validationResults);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage?.Contains("ApiVersion") == true);
    }

    [Fact]
    public void Validate_WithInvalidEnvironment_ReturnsFalse()
    {
        // Arrange
        var options = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "Test API",
                Environment = "InvalidEnvironment"
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions(),
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions(),
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions(),
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions(),
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions(),
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions(),
            Security = new SignalrMapRealtimeOptions.SecurityOptions()
        };

        // Act
        var isValid = options.Validate(out var validationResults);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage?.Contains("Environment") == true);
    }

    [Fact]
    public void Validate_WithOutOfRangeValues_ReturnsFalse()
    {
        // Arrange
        var options = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "Test API",
                Environment = "Production",
                RequestTimeoutSeconds = 2, // Too low
                LocationUpdateIntervalSeconds = 700 // Too high
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions(),
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions(),
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions(),
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions(),
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions(),
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions(),
            Security = new SignalrMapRealtimeOptions.SecurityOptions()
        };

        // Act
        var isValid = options.Validate(out var validationResults);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage?.Contains("RequestTimeoutSeconds") == true);
        Assert.Contains(validationResults, v => v.ErrorMessage?.Contains("LocationUpdateIntervalSeconds") == true);
    }

    [Fact]
    public void AppInfoOptions_WithEmptyApiTitle_ReturnsFalse()
    {
        // Arrange
        var options = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "", // Empty
                Environment = "Production"
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions(),
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions(),
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions(),
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions(),
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions(),
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions(),
            Security = new SignalrMapRealtimeOptions.SecurityOptions()
        };

        // Act
        var isValid = options.Validate(out var validationResults);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage?.Contains("ApiTitle") == true);
    }

    [Fact]
    public void SignalRHubsOptions_WithOutOfRangeMaxConnections_ReturnsFalse()
    {
        // Arrange
        var options = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "Test API",
                Environment = "Production"
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions(),
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions(),
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions(),
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions
            {
                MaxConnectionsPerHub = 50001 // Too high
            },
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions(),
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions(),
            Security = new SignalrMapRealtimeOptions.SecurityOptions()
        };

        // Act
        var isValid = options.Validate(out var validationResults);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage?.Contains("MaxConnectionsPerHub") == true);
    }

    [Fact]
    public void PerformanceOptions_WithOutOfRangeMaxConcurrentConnections_ReturnsFalse()
    {
        // Arrange
        var options = new SignalrMapRealtimeOptions
        {
            AppInfo = new SignalrMapRealtimeOptions.AppInfoOptions
            {
                ApiVersion = "2.0.0",
                ApiTitle = "Test API",
                Environment = "Production"
            },
            HealthChecks = new SignalrMapRealtimeOptions.HealthCheckOptions(),
            ApiKeyAuthentication = new SignalrMapRealtimeOptions.ApiKeyAuthenticationOptions(),
            Performance = new SignalrMapRealtimeOptions.PerformanceOptions
            {
                MaxConcurrentConnections = 99 // Too low
            },
            SignalRHubs = new SignalrMapRealtimeOptions.SignalRHubOptions(),
            WebSockets = new SignalrMapRealtimeOptions.WebSocketOptions(),
            BackgroundJobs = new SignalrMapRealtimeOptions.BackgroundJobsOptions(),
            Security = new SignalrMapRealtimeOptions.SecurityOptions()
        };

        // Act
        var isValid = options.Validate(out var validationResults);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage?.Contains("MaxConcurrentConnections") == true);
    }

    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        // Arrange & Act
        var sectionName = SignalrMapRealtimeOptions.SectionName;

        // Assert
        Assert.Equal("SignalRMapRealtime", sectionName);
    }

    [Fact]
    public void AllNestedOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new SignalrMapRealtimeOptions();

        // Assert
        Assert.NotNull(options.AppInfo);
        Assert.NotNull(options.HealthChecks);
        Assert.NotNull(options.ApiKeyAuthentication);
        Assert.NotNull(options.Performance);
        Assert.NotNull(options.SignalRHubs);
        Assert.NotNull(options.WebSockets);
        Assert.NotNull(options.BackgroundJobs);
        Assert.NotNull(options.Security);

        // Verify default values are set
        Assert.Equal("2.0.0", options.AppInfo.ApiVersion);
        Assert.Equal("Production", options.AppInfo.Environment);
        Assert.True(options.AppInfo.EnableSwagger);
        Assert.True(options.HealthChecks.Enabled);
        Assert.True(options.ApiKeyAuthentication.Enabled);
        Assert.True(options.Performance.EnableDetailedMetrics);
        Assert.True(options.SignalRHubs.Enabled);
        Assert.True(options.WebSockets.Enabled);
        Assert.True(options.BackgroundJobs.Enabled);
    }
}