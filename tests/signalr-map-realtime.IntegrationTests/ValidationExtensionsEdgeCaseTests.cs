#nullable enable
using FluentAssertions;
using SignalRMapRealtime.Utilities;
using Xunit;

namespace SignalRMapRealtime.IntegrationTests;

public sealed class ValidationExtensionsEdgeCaseTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("test@example.com", true)]
    [InlineData("user@domain.co", true)]
    [InlineData("no-at-sign", false)]
    [InlineData("@no-local-part.com", false)]
    [InlineData("no-domain@", false)]
    public void IsValidEmail_VariousInputs(string? email, bool expected) =>
        email.IsValidEmail().Should().Be(expected);

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("+1234567890", true)]
    [InlineData("1234567890", true)]
    [InlineData("(123) 456-7890", true)]
    [InlineData("123", false)]  // Too short
    [InlineData("abc", false)]
    public void IsValidPhoneNumber_VariousInputs(string? phone, bool expected) =>
        phone.IsValidPhoneNumber().Should().Be(expected);

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("not-a-url", false)]
    [InlineData("https://example.com", true)]
    [InlineData("http://localhost:5000", true)]
    [InlineData("ftp://files.com", false)]
    public void IsValidUrl_VariousInputs(string? url, bool expected) =>
        url.IsValidUrl().Should().Be(expected);

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("192.168.1.1", true)]
    [InlineData("999.999.999.999", false)]
    [InlineData("::1", true)]
    [InlineData("abc.def.ghi.jkl", false)]
    public void IsValidIpAddress_VariousInputs(string? ip, bool expected) =>
        ip.IsValidIpAddress().Should().Be(expected);
}
