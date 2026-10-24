#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension methods for adding API Key authentication to an IServiceCollection.
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Adds API Key authentication to the specified IServiceCollection.
    /// The configured keys are validated at startup, so a malformed hash stops the application
    /// instead of rejecting every request at runtime.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">An action to configure the <see cref="ApiKeyAuthenticationOptions"/>.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static AuthenticationBuilder AddApiKeyAuthentication(
        this IServiceCollection services,
        Action<ApiKeyAuthenticationOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IValidateOptions<ApiKeyAuthenticationOptions>, ApiKeyAuthenticationOptionsValidation>();
        services.AddOptions<ApiKeyAuthenticationOptions>(ApiKeyAuthenticationOptions.DefaultScheme)
            .ValidateOnStart();

        return services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                configureOptions);
    }
}
