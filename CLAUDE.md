# CLAUDE.md

ASP.NET Core 8 web API + SignalR hubs for real-time vehicle/asset location tracking on a map (EF Core + SQL Server, geofences, route playback, clustering).

## Build

```bash
dotnet restore
dotnet build                                   # or: make build
dotnet run --project src/SignalRMapRealtime    # or: make run / make watch
dotnet publish -c Release src/SignalRMapRealtime
docker compose up -d                           # docker-compose.yml / docker-compose.dev.yml
```

EF Core: `make db-update`, `make db-migrate`, `make db-drop` (all target `src/SignalRMapRealtime`). `Migrations/` is gitignored.
Config: copy `src/SignalRMapRealtime/appsettings.example.json` -> `appsettings.Development.json` (gitignored). SQL Server connection string in `ConnectionStrings:DefaultConnection`.

## Test

```bash
dotnet test                                                   # all projects in the .sln
dotnet test tests/signalr-map-realtime.Tests                  # unit tests
dotnet test tests/signalr-map-realtime.IntegrationTests       # WebApplicationFactory + EF InMemory
dotnet test --filter "FullyQualifiedName~AssetTests"          # single class
dotnet run -c Release --project tests/signalr-map-realtime.Benchmarks   # BenchmarkDotNet (net10.0)
```

Stack: xUnit 2.9, FluentAssertions 7, NSubstitute 5. CI (`.github/workflows/ci.yml`, `build.yml`) runs restore/build/test in Release on .NET 8.0.x and 10.0.x.

## Lint / format

```bash
dotnet format                                        # make format
dotnet build /p:EnforceCodeStyleInBuild=true         # make lint
```

Style from `.editorconfig`: 4-space indent (2 for csproj/json/yml), LF, UTF-8, final newline. `Nullable` and `ImplicitUsings` enabled.

## Key directories

- `signalr-map-realtime.sln` - only projects listed here are built by `dotnet build`/`dotnet test`: main app, `tests/signalr-map-realtime.Tests`, `tests/signalr-map-realtime.IntegrationTests`, `tests/signalr-map-realtime.Benchmarks`.
- `src/SignalRMapRealtime/Program.cs` - entry point; explicit `Program` class, options bound with `ValidateDataAnnotations()`, services wired via extension methods (`AddApplicationServices`, `AddSignalRServices`, `AddEventBus`, `AddApiKeyAuthentication`, ...).
- `src/SignalRMapRealtime/Hubs/` - `LocationHub`, `RoutePlaybackHub` (SignalR).
- `src/SignalRMapRealtime/Controllers/` - REST API: Asset, Vehicle, Location, Route, Geofence, Playback, Clustering, Webhooks.
- `src/SignalRMapRealtime/Services/` - business logic behind `I*Service` interfaces (Location, Tracking, Vehicle, Geofence, Clustering, RoutePlayback, Notification, Cache, `LocationUpdateThrottler`, `VehicleStaleDetectionService`).
- `src/SignalRMapRealtime/Data/` - `ApplicationDbContext`, `Repositories/` (`IRepository<T>` -> `BaseRepository<T>` -> per-entity repos).
- `src/SignalRMapRealtime/Domain/Models|Enums/` - entities (Asset, Vehicle, Location, Route, Waypoint, Geofence, TrackingSession, User).
- `DTOs/`, `Configuration/` (`*Options` with `SectionName` const), `Events/` (domain events + event bus), `Middleware/` (error handling, rate limiting), `Authentication/` (API key), `BackgroundJobs/`, `Integration/` (webhooks), `Formatters/`, `Utilities/`, `Constants/`, `Attributes/`.
- `tests/` - unit tests mirror source folders (`Controllers/`, `Configuration/`, `Formatters/`, ...).
- `docs/` - per-class markdown docs plus `architecture.md`, `database-schema.md`, `deployment.md`, `docker-guide.md`, `troubleshooting.md`.
- `examples/` - standalone client samples (not part of the solution).

Orphan/legacy locations not referenced by the .sln: root `Controllers/`, `Services/`, `src/SignalRMapRealtime.Tests/`, `src/tests/`, `tests/SignalRMapRealtime.Domain.Models.Tests/`, `tests/SignalRMapRealtime.Hubs.Tests/`. Do not add new code there.

## Conventions

- Namespaces follow folders: `SignalRMapRealtime.<Folder>`. Files start with `#nullable enable` and the author header comment block.
- One public type per file; interface `IFoo` next to `Foo` in the same folder.
- Companion files by suffix: `FooExtensions.cs` (static extension methods), `FooJsonExtensions.cs` (JSON helpers), `FooValidation.cs` (validation/guards). Keep the pattern when extending a type.
- Options classes: `XxxOptions` with `public const string SectionName`, data-annotation validated at startup.
- API responses wrapped in `ApiResponse<T>` / `PaginatedResponse<T>`; errors in `ErrorResponse`; custom exceptions in `Exceptions/`.
- Tests: `<TypeUnderTest>Tests.cs`, `[Fact]`/`[Theory]`, FluentAssertions `Should()`, NSubstitute for mocks; `<TypeUnderTest>ValidationTests.cs` / `JsonExtensionsTests.cs` for companion files.
- Commits: conventional prefixes (`feat:`, `fix:`, `docs:`, `chore:`), PRs target `main`.
