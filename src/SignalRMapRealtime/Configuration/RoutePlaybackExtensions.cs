// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

using SignalRMapRealtime.Hubs;
using SignalRMapRealtime.Services;

/// <summary>
/// Dependency injection and endpoint registration extension methods for the v2 route playback feature.
/// Call <see cref="AddRoutePlayback"/> during service registration and
/// <see cref="MapRoutePlaybackHub"/> when configuring the endpoint pipeline.
/// </summary>
public static class RoutePlaybackExtensions
{
    /// <summary>
    /// Registers all services required by the real-time route playback and historical
    /// timeline engine, including the singleton <see cref="IRoutePlaybackService"/> and
    /// the typed <see cref="PlaybackOptions"/> configuration binding.
    /// </summary>
    /// <remarks>
    /// This method must be called before <c>builder.Build()</c> in <c>Program.cs</c>.
    /// Ensure that <see cref="DependencyInjection.AddSignalRServices"/> has also been called
    /// so that SignalR infrastructure is available for <see cref="RoutePlaybackHub"/>.
    /// </remarks>
    /// <param name="services">The application service collection to register into.</param>
    /// <param name="configuration">
    /// Application configuration used to bind the <see cref="PlaybackOptions.SectionName"/> section.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddApplicationServices(builder.Configuration)
    ///     .AddSignalRServices()
    ///     .AddRoutePlayback(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddRoutePlayback(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<PlaybackOptions>(configuration.GetSection(PlaybackOptions.SectionName));
        services.AddSingleton<IRoutePlaybackService, RoutePlaybackService>();

        return services;
    }

    /// <summary>
    /// Maps the <see cref="RoutePlaybackHub"/> SignalR endpoint at the conventional path
    /// <c>/hubs/playback</c>.
    /// </summary>
    /// <remarks>
    /// Call this method within the <c>app.MapXxx</c> endpoint pipeline, after
    /// <c>app.UseRouting()</c> and <c>app.UseAuthorization()</c>.
    /// JavaScript clients should connect using:
    /// <code>
    /// const connection = new signalR.HubConnectionBuilder()
    ///     .withUrl("/hubs/playback")
    ///     .withAutomaticReconnect()
    ///     .build();
    /// </code>
    /// </remarks>
    /// <param name="endpoints">The endpoint route builder from <c>app.UseEndpoints</c> or <c>app.MapXxx</c>.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> for fluent chaining.</returns>
    /// <example>
    /// <code>
    /// app.MapHub&lt;LocationHub&gt;("/hubs/location");
    /// app.MapRoutePlaybackHub();
    /// </code>
    /// </example>
    public static IEndpointRouteBuilder MapRoutePlaybackHub(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapHub<RoutePlaybackHub>("/hubs/playback");
        return endpoints;
    }
}
