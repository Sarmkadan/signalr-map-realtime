#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Tests;

using FluentAssertions;
using SignalRMapRealtime.Configuration;
using Xunit;

/// <summary>
/// Tests for the NotificationOptions class and its sub-options.
/// </summary>
public class NotificationOptionsTests
{
    [Fact]
    public void NotificationOptions_ShouldInitializeWithDefaultValues()
    {
        // Act
        var options = new NotificationOptions();

        // Assert
        options.Enabled.Should().BeFalse();
        options.DefaultSender.Should().Be("noreply@signalrmaptracking.com");
        options.UseAsyncProcessing.Should().BeTrue();
        options.MaxRetries.Should().Be(3);
        options.RetryDelaySeconds.Should().Be(5);
        options.Email.Should().NotBeNull();
        options.Sms.Should().NotBeNull();
        options.Push.Should().NotBeNull();
    }

    [Fact]
    public void NotificationOptions_Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new NotificationOptions();

        // Act
        options.Enabled = true;
        options.DefaultSender = "custom@example.com";
        options.UseAsyncProcessing = false;
        options.MaxRetries = 5;
        options.RetryDelaySeconds = 10;

        // Assert
        options.Enabled.Should().BeTrue();
        options.DefaultSender.Should().Be("custom@example.com");
        options.UseAsyncProcessing.Should().BeFalse();
        options.MaxRetries.Should().Be(5);
        options.RetryDelaySeconds.Should().Be(10);
    }

    [Fact]
    public void EmailNotificationOptions_ShouldInitializeWithDefaultValues()
    {
        // Act
        var options = new EmailNotificationOptions();

        // Assert
        options.Enabled.Should().BeFalse();
        options.SmtpHost.Should().Be(string.Empty);
        options.SmtpPort.Should().Be(587);
        options.SmtpUsername.Should().Be(string.Empty);
        options.SmtpPassword.Should().Be(string.Empty);
        options.UsesTls.Should().BeTrue();
    }

    [Fact]
    public void EmailNotificationOptions_Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new EmailNotificationOptions();

        // Act
        options.Enabled = true;
        options.SmtpHost = "smtp.example.com";
        options.SmtpPort = 25;
        options.SmtpUsername = "user";
        options.SmtpPassword = "password";
        options.UsesTls = false;

        // Assert
        options.Enabled.Should().BeTrue();
        options.SmtpHost.Should().Be("smtp.example.com");
        options.SmtpPort.Should().Be(25);
        options.SmtpUsername.Should().Be("user");
        options.SmtpPassword.Should().Be("password");
        options.UsesTls.Should().BeFalse();
    }
}
