#nullable enable
using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Xunit;
using SignalRMapRealtime.Utilities;

namespace SignalRMapRealtime.Tests;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetClientIpAddress_ReturnsForwardedForHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.5, 198.51.100.2";

        var ip = context.GetClientIpAddress();

        Assert.Equal("203.0.113.5", ip);
    }

    [Fact]
    public void GetClientIpAddress_FallbackToRemoteIpWhenNoHeaders()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.1");

        var ip = context.GetClientIpAddress();

        Assert.Equal("192.0.2.1", ip);
    }

    [Fact]
    public void GetUserAgent_ReturnsHeader_AndThrowsOnNullContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["User-Agent"] = "UnitTestAgent/1.0";

        var agent = context.GetUserAgent();

        Assert.Equal("UnitTestClient/1.0", agent);

        // Verify exception for null context
        Assert.Throws<ArgumentNullException>(() => ((HttpContext)null!).GetUserAgent());
    }

    [Fact]
    public void GetFullUrlAndBaseUrl_ReturnCorrectValues()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = "/api/values";
        context.Request.QueryString = new QueryString("?id=5");

        var fullUrl = context.GetFullUrl();
        var baseUrl = context.GetBaseUrl();

        Assert.Equal("https://example.com/api/values?id=5", fullUrl);
        Assert.Equal("https://example.com", baseUrl);
    }

    [Fact]
    public void GetRefererAndOrigin_ReturnsHeadersOrNull()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Referer"] = "https://referrer.example.com/page";
        context.Request.Headers["Origin"] = "https://origin.example.com";

        var referer = context.GetReferer();
        var origin = context.GetOrigin();

        Assert.Equal("https://referrer.example.com/page", referer);
        Assert.Equal("https://origin.example.com", origin);

        // When headers are missing they should return null
        var emptyContext = new DefaultHttpContext();
        Assert.Null(emptyContext.GetReferer());
        Assert.Null(emptyContext.GetOrigin());
    }

    [Fact]
    public void IsSecure_ReturnsTrueWhenHttps()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.IsHttps = true;

        Assert.True(context.IsSecure());

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        httpContext.Request.IsHttps = false;

        Assert.False(httpContext.IsSecure());
    }

    [Fact]
    public void HasHeaderAndGetHeader_BehaveCorrectly()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Custom"] = "custom-value";

        Assert.True(context.HasHeader("X-Custom"));
        Assert.False(context.HasHeader("Missing-Header"));

        var value = context.GetHeader("X-Custom");
        var missing = context.GetHeader("Missing-Header");

        Assert.Equal("custom-value", value);
        Assert.Equal(string.Empty, missing);
    }

    [Fact]
    public void IsAjaxRequest_ReturnsTrueWhenHeaderSet()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        Assert.True(context.IsAjaxRequest());

        var nonAjaxContext = new DefaultHttpContext();
        Assert.False(nonAjaxContext.IsAjaxRequest());
    }
}
