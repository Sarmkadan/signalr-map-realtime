using System;
using Xunit;
using SignalRMapRealtime.Utilities;

namespace SignalRMapRealtime.Tests;

public class ValidationExtensionsTests
{
    [Fact]
    public void IsValidEmail_ValidEmail_ReturnsTrue()
    {
        var email = "test@example.com";
        Assert.True(email.IsValidEmail());
    }

    [Fact]
    public void IsValidEmail_InvalidEmail_ReturnsFalse()
    {
        var email = "invalid-email";
        Assert.False(email.IsValidEmail());
    }

    [Fact]
    public void IsValidEmail_NullEmail_ThrowsArgumentNullException()
    {
        string? email = null;
        Assert.Throws<ArgumentNullException>(() => email.IsValidEmail());
    }

    [Fact]
    public void IsValidPhoneNumber_ValidPhoneNumber_ReturnsTrue()
    {
        var phoneNumber = "+1234567890";
        Assert.True(phoneNumber.IsValidPhoneNumber());
    }

    [Fact]
    public void IsValidPhoneNumber_InvalidPhoneNumber_ReturnsFalse()
    {
        var phoneNumber = "invalid-phone-number";
        Assert.False(phoneNumber.IsValidPhoneNumber());
    }

    [Fact]
    public void IsValidUrl_ValidUrl_ReturnsTrue()
    {
        var url = "https://example.com";
        Assert.True(url.IsValidUrl());
    }

    [Fact]
    public void IsValidUrl_InvalidUrl_ReturnsFalse()
    {
        var url = "invalid-url";
        Assert.False(url.IsValidUrl());
    }

    [Fact]
    public void IsAlphanumeric_ValidString_ReturnsTrue()
    {
        var str = "HelloWorld123";
        Assert.True(str.IsAlphanumeric());
    }

    [Fact]
    public void IsAlphanumeric_InvalidString_ReturnsFalse()
    {
        var str = "Hello World!";
        Assert.False(str.IsAlphanumeric());
    }

    [Fact]
    public void IsStrongPassword_ValidPassword_ReturnsTrue()
    {
        var password = "P@ssw0rd";
        Assert.True(password.IsStrongPassword());
    }

    [Fact]
    public void IsStrongPassword_InvalidPassword_ReturnsFalse()
    {
        var password = "weak";
        Assert.False(password.IsStrongPassword());
    }

    [Fact]
    public void IsInRange_ValidRange_ReturnsTrue()
    {
        var value = 5;
        Assert.True(value.IsInRange(1, 10));
    }

    [Fact]
    public void IsInRange_InvalidRange_ReturnsFalse()
    {
        var value = 15;
        Assert.False(value.IsInRange(1, 10));
    }

    [Fact]
    public void IsLengthInRange_ValidLength_ReturnsTrue()
    {
        var str = "Hello";
        Assert.True(str.IsLengthInRange(1, 10));
    }

    [Fact]
    public void IsLengthInRange_InvalidLength_ReturnsFalse()
    {
        var str = new string('a', 20);
        Assert.False(str.IsLengthInRange(1, 10));
    }

    [Fact]
    public void HasElements_ValidCollection_ReturnsTrue()
    {
        var collection = new[] { 1, 2, 3 };
        Assert.True(collection.HasElements());
    }

    [Fact]
    public void HasElements_InvalidCollection_ReturnsFalse()
    {
        var collection = new int[0];
        Assert.False(collection.HasElements());
    }
}
