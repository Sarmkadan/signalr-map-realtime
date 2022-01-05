// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Data.Repositories;
using SignalRMapRealtime.Services;

/// <summary>
/// Extension methods for registering services and dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application services and repositories.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Database context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, dbOpts =>
            {
                dbOpts.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelaySeconds: 5, errorNumbersToAdd: null);
            })
        );

        // Repositories
        services.AddScoped<IRepository<global::SignalRMapRealtime.Domain.Models.Vehicle>, BaseRepository<global::SignalRMapRealtime.Domain.Models.Vehicle>>();
        services.AddScoped<IRepository<global::SignalRMapRealtime.Domain.Models.Location>, BaseRepository<global::SignalRMapRealtime.Domain.Models.Location>>();
        services.AddScoped<IRepository<global::SignalRMapRealtime.Domain.Models.User>, BaseRepository<global::SignalRMapRealtime.Domain.Models.User>>();
        services.AddScoped<IRepository<global::SignalRMapRealtime.Domain.Models.Asset>, BaseRepository<global::SignalRMapRealtime.Domain.Models.Asset>>();
        services.AddScoped<IRepository<global::SignalRMapRealtime.Domain.Models.Route>, BaseRepository<global::SignalRMapRealtime.Domain.Models.Route>>();
        services.AddScoped<IRepository<global::SignalRMapRealtime.Domain.Models.Waypoint>, BaseRepository<global::SignalRMapRealtime.Domain.Models.Waypoint>>();
        services.AddScoped<IRepository<global::SignalRMapRealtime.Domain.Models.TrackingSession>, BaseRepository<global::SignalRMapRealtime.Domain.Models.TrackingSession>>();

        // Specialized repositories
        services.AddScoped<VehicleRepository>();
        services.AddScoped<LocationRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<AssetRepository>();
        services.AddScoped<RouteRepository>();
        services.AddScoped<TrackingSessionRepository>();

        // Business services
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ITrackingService, TrackingService>();

        // AutoMapper
        services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

        return services;
    }

    /// <summary>
    /// Registers SignalR services.
    /// </summary>
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSignalR(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.DisableImplicitFromServicesParameters = false;
        });

        return services;
    }

    /// <summary>
    /// Registers Swagger/OpenAPI documentation.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = Constants.ApiConstants.ApiTitle,
                Version = Constants.ApiConstants.ApiVersion,
                Description = Constants.ApiConstants.ApiDescription
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
