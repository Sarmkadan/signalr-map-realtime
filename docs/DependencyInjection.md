# DependencyInjection

`DependencyInjection` is a static class of extension methods on `IServiceCollection`. Each method groups one area of service registration so that `Program.cs` can call it in one line. The class holds no state.

Source: `src/SignalRMapRealtime/Configuration/DependencyInjection.cs`

## Methods

| Method | Parameters | Registers |
|---|---|---|
| `AddApplicationServices` | `IServiceCollection services`, `IConfiguration configuration` | EF Core DbContext, repositories, business services, clustering, geofencing, route playback, location throttler, AutoMapper |
| `AddSignalRServices` | `IServiceCollection services` | SignalR with a 30-second client timeout |
| `AddSwaggerDocumentation` | `IServiceCollection services` | Swagger generator for the `v1` document, with XML comments when the file exists |

All three methods:

- call `ArgumentNullException.ThrowIfNull` on `services` and return the same `services` instance, so calls can be chained,
- do not register hubs, middleware, or endpoints. Those belong to the host (`Program.cs`) and are outside this file.

`AddApplicationServices` also checks `configuration`. The other two methods check only `services`.

## AddApplicationServices

### Database context

The connection string is read from `ConnectionStrings:DefaultConnection`. If it is missing or whitespace, the method throws `InvalidOperationException` with the message `Connection string 'DefaultConnection' not found.` The check happens at registration time, so a misconfigured connection string fails at startup.

`ApplicationDbContext` is registered with `AddDbContext` and `UseSqlServer`, with SQL Server retry on failure:

| Setting | Value |
|---|---|
| `maxRetryCount` | 3 |
| `maxRetryDelay` | 5 seconds |
| `errorNumbersToAdd` | `null` (no extra SQL error numbers added) |

The file does not pass a lifetime to `AddDbContext`, so the EF Core default applies.

### Repositories

Two layers are registered. All are scoped.

Generic repositories: `IRepository<T>` resolves to `BaseRepository<T>` for each of seven entities:

| Entity |
|---|
| `Vehicle` |
| `Location` |
| `User` |
| `Asset` |
| `Route` |
| `Waypoint` |
| `TrackingSession` |

Specialized repositories are registered by concrete type only. There is no interface and no mapping to a base type:

| Type | Lifetime |
|---|---|
| `VehicleRepository` | Scoped |
| `LocationRepository` | Scoped |
| `UserRepository` | Scoped |
| `AssetRepository` | Scoped |
| `RouteRepository` | Scoped |
| `TrackingSessionRepository` | Scoped |

There is no specialized repository for `Waypoint`. Only the generic `IRepository<Waypoint>` is registered for it.

### Business services

| Interface | Implementation | Lifetime |
|---|---|---|
| `ILocationService` | `LocationService` | Scoped |
| `IVehicleService` | `VehicleService` | Scoped |
| `ITrackingService` | `TrackingService` | Scoped |

### Clustering and geofencing

- `services.AddClustering()` registers location clustering and heatmap support.
- `services.AddGeofencing()` registers geofencing. The comment in the file says it is a singleton, so that zone configuration and vehicle presence history persist. The lifetime is set inside `AddGeofencing`, which is defined outside this file. This file does not show it.

Both extension methods are defined outside `DependencyInjection.cs`.

### Route playback

- `PlaybackOptions` is bound to the configuration section `PlaybackOptions.SectionName` with `Configure`.
- `IRoutePlaybackService` resolves to `RoutePlaybackService` as a **singleton**. The comment says this keeps active playback session state across requests.

### Location update throttler

- `ThrottleOptions` is bound to the configuration section `ThrottleOptions.SectionName` with `Configure`.
- `LocationUpdateThrottler` is registered as a **singleton** using a factory delegate. The factory resolves:
  - `IOptions<ThrottleOptions>`,
  - `ILogger<LocationUpdateThrottler>`,
  - `IHubContext<LocationHub>`,

  and passes them to the constructor. The comment says the singleton shares state across hub instances.

### AutoMapper

`services.AddAutoMapper(typeof(AutoMapperProfile).Assembly)` scans the assembly that contains `AutoMapperProfile` for AutoMapper profiles. The profile itself is documented in `AutoMapperProfile.md`.

## AddSignalRServices

Calls `AddSignalR` with these options:

| Option | Value | Meaning |
|---|---|---|
| `ClientTimeoutInterval` | 30 seconds | The server treats a client as disconnected after 30 seconds without a message |
| `DisableImplicitFromServicesParameters` | `false` | Hub method parameters may be resolved from DI without `[FromServices]`. The value is set explicitly, but `false` is the default. |

## AddSwaggerDocumentation

Calls `AddSwaggerGen` and defines one document, `v1`, using these constants from `SignalRMapRealtime.Constants.ApiConstants`:

- `ApiTitle` → title
- `ApiVersion` → version
- `ApiDescription` → description

XML documentation is added only if the file `<AssemblyName>.xml` exists in `AppContext.BaseDirectory`. The assembly name comes from the executing assembly. If the file is absent, the method still succeeds and Swagger is generated without XML comments.

This file does not add the Swagger UI middleware. That is outside this file.

## Behavior not defined in this file

The file does not show:

- the order in which `Program.cs` calls these methods,
- whether options are validated (for example with `ValidateDataAnnotations()`),
- the lifetime of `AddClustering` and `AddGeofencing`,
- which hubs are mapped to endpoints.

These are determined by code outside this file and are not covered here.

## Related

- `src/SignalRMapRealtime/Program.cs` - calls these extension methods.
- `src/SignalRMapRealtime/Data/ApplicationDbContext.cs` and `Data/Repositories/` - types registered in `AddApplicationServices`.
- `src/SignalRMapRealtime/Services/` - `I*Service` interfaces and implementations.
- `src/SignalRMapRealtime/Hubs/LocationHub.cs` - hub used by the throttler's `IHubContext`.
- `AutoMapperProfile.md` - the profile registered by `AddAutoMapper`.
