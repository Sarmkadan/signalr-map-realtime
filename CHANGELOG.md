# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-04-10

### Added
- Comprehensive documentation suite (getting-started, architecture, deployment, FAQ, troubleshooting)
- Example clients: VehicleTrackerClient, LocationUpdateSimulator, RouteOptimizationClient, GeofenceAlertsClient, BulkLocationImporter, SessionAnalyticsReporter
- Real-time HTML5 web viewer with Leaflet map integration
- xUnit test suite covering domain model behavior, geo-distance calculations, and location service logic
- Docker and Docker Compose support for complete stack deployment
- Makefile for common development tasks
- EditorConfig for consistent code formatting
- NuGet packaging configuration with README and license
- Security scanning via CodeQL workflow
- Dependabot configuration for automated dependency updates
- CI/CD workflow for automated builds and testing

### Changed
- Updated to .NET 10.0
- Improved database schema documentation
- Enhanced API reference with detailed endpoint examples
- Refined error handling and logging configuration

### Fixed
- SignalR connection stability improvements
- Database migration scripts now idempotent
- CORS configuration more flexible

## [0.4.0] - 2025-03-06

### Added
- FluentValidation integration for request validation
- Rate limiting middleware for API protection
- Performance monitoring middleware with timing metrics
- Comprehensive Swagger/OpenAPI documentation
- Background job for session cleanup (`SessionCleanupWorker`)
- Event bus for domain events (`DomainEvent`, `EventBus`)
- Webhook handler for third-party integrations
- Health check endpoint
- Extended logging with structured JSON output
- GeoJSON serializer for location data
- HTTP client factory for integration layer

### Changed
- Refactored dependency injection configuration into `DependencyInjection.cs`
- Improved service layer error handling
- Enhanced database context configuration with full entity mappings
- Optimized entity mappings with AutoMapper profiles

### Fixed
- Fixed N+1 query problems in location retrievals
- Improved pagination handling in repository base class
- Fixed null reference exceptions in edge cases across service layer

## [0.3.0] - 2025-02-17

### Added
- REST API controllers: VehicleController, LocationController, RouteController, AssetController
- SignalR `LocationHub` with client/server method contracts
- Data Transfer Objects (DTOs) for all API contracts
- AutoMapper profile for entity-to-DTO mapping
- `NotificationService` for real-time alert delivery
- `CacheService` with in-memory/Redis-compatible interface
- `GeofenceService` for boundary entry/exit detection
- API key authentication attribute
- Error handling middleware with structured JSON responses
- Request/response logging middleware
- Rate limiting middleware scaffold
- CORS configuration
- Swagger/OpenAPI setup in `Program.cs`
- Configuration options: `CachingOptions`, `RateLimitingOptions`, `NotificationOptions`
- Extension methods: `GeoLocationExtensions`, `DateTimeExtensions`, `StringExtensions`, `PaginationExtensions`
- Constants for API versioning and location validation (`ApiConstants`, `LocationConstants`)

### Fixed
- Route waypoint ordering during creation
- Asset type deserialization from JSON

## [0.2.0] - 2025-01-30

### Added
- Repository pattern with generic `BaseRepository<T>` and `IRepository<T>`
- Concrete repositories: VehicleRepository, LocationRepository, RouteRepository, AssetRepository, UserRepository, TrackingSessionRepository
- Service layer: `LocationService`, `VehicleService`, `TrackingService`
- Service interfaces: `ILocationService`, `IVehicleService`, `ITrackingService`
- `ApplicationDbContext` with EF Core configuration for all entity types
- Paginated response models (`PagedRequest`, `PaginatedResponse<T>`)
- `ApiResponse<T>` and `ErrorResponse` envelope models
- Custom exception hierarchy (`LocationTrackingException`)
- `ClaimsExtensions` and `HttpContextExtensions` utilities
- `CollectionExtensions` and `ValidationExtensions` helpers

### Changed
- Separated domain models into dedicated `Domain/Models` and `Domain/Enums` folders

## [0.1.0] - 2025-01-08

### Added
- Initial project setup targeting .NET 10.0
- Solution file and project structure
- Core domain models: `Vehicle`, `Location`, `Route`, `User`, `Asset`, `Waypoint`, `TrackingSession`, `Geofence`
- Domain enums: `VehicleStatus`, `LocationType`, `AssetType`, `SessionStatus`
- Git repository initialization with `.gitignore`
- `README.md` with project overview
- MIT `LICENSE`
- Basic `Program.cs` application entry point
- `appsettings.json` and `appsettings.Development.json` configuration files

### Security
- API key authentication infrastructure
- SQL injection prevention via EF Core parameterized queries
- Input validation foundation
- HTTPS support

---

## Version Support

| Version | Status | .NET Target | Release Date |
|---------|--------|------------|--------------|
| 1.0.0 | Current | net10.0 | 2025-04-10 |
| 0.4.0 | Legacy | net10.0 | 2025-03-06 |
| 0.3.0 | Legacy | net10.0 | 2025-02-17 |
| 0.2.0 | End of Life | net10.0 | 2025-01-30 |
| 0.1.0 | End of Life | net10.0 | 2025-01-08 |

---

## Contributing

When adding new features, please update this file in the "Unreleased" section with:
- What was added (features, improvements)
- What changed (breaking changes, modifications)
- What was fixed (bug fixes)
- Security updates

## Contact

- **Author**: Vladyslav Zaiets
- **Website**: https://sarmkadan.com
- **GitHub**: https://github.com/Sarmkadan

---

**Note**: This changelog is maintained for all released versions. For unreleased changes, refer to the main branch.
