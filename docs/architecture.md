// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture Guide

This document describes the system architecture, design patterns, and architectural decisions in SignalR Map Realtime.

## Architecture Overview

SignalR Map Realtime uses a **layered hexagonal (ports & adapters) architecture** combined with **domain-driven design** principles.

```
┌─────────────────────────────────────────────────────┐
│                 API Layer                           │
│  (Controllers, SignalR Hubs, DTOs)                 │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│              Application Layer                      │
│  (Services, Business Logic, Orchestration)         │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│               Domain Layer                          │
│  (Entities, Value Objects, Business Rules)         │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│           Infrastructure Layer                      │
│  (Database, External Services, Frameworks)         │
└─────────────────────────────────────────────────────┘
```

## Layered Architecture Detailed

### 1. Presentation/API Layer

Handles HTTP requests and SignalR connections.

**Components:**
- **Controllers**: REST API endpoints
  - `VehicleController` - Vehicle CRUD and status
  - `LocationController` - Location tracking endpoints
  - `RouteController` - Route management
  - `AssetController` - Asset tracking

- **SignalR Hubs**: Real-time bidirectional communication
  - `LocationHub` - Vehicle location streaming and subscriptions

- **DTOs**: Data Transfer Objects for API contracts
  - Serialize/deserialize domain models
  - Decouple API contracts from domain models
  - Version API contracts independently

- **Middleware**: Cross-cutting concerns
  - `ErrorHandlingMiddleware` - Centralized exception handling
  - `LoggingMiddleware` - Request/response logging
  - `RateLimitingMiddleware` - API rate limiting
  - `PerformanceMonitoringMiddleware` - Request timing

- **Attributes**: Custom attributes for authorization/validation
  - `ApiKeyAuthenticationAttribute` - API key validation

### 2. Application/Service Layer

Business logic and orchestration of domain models.

**Core Services:**

**LocationService**
- Records new locations
- Retrieves location history
- Calculates distance between points
- Implements geofencing logic
- Manages location caching

**VehicleService**
- CRUD operations for vehicles
- Status management (Active, Inactive, InMaintenance)
- Driver assignment
- Vehicle availability tracking
- Fuel level monitoring

**TrackingService**
- Session lifecycle management
- Session statistics calculation
- Distance and duration tracking
- Performance metrics aggregation
- Session state transitions

**NotificationService**
- Broadcasts events to connected clients
- Generates system alerts
- Manages subscriptions
- Handles SignalR messaging

**CacheService**
- In-memory caching for frequently accessed data
- Cache invalidation strategies
- Performance optimization

**Design Patterns:**
- **Dependency Injection**: All services injected via constructor
- **Async/Await**: All I/O operations are async
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Implicit via EF Core DbContext

### 3. Domain Layer

Core business logic and domain models.

**Entities** (Aggregates):

```csharp
// Vehicle aggregate
- Id (PK)
- LicensePlate (unique)
- Manufacturer, Model, Year
- Status (enum)
- AssignedUser (FK)
- Locations (navigation)
- Routes (navigation)
```

```csharp
// Location aggregate
- Id (PK)
- VehicleId (FK)
- Latitude, Longitude
- Accuracy, Altitude
- Speed, Heading
- Type (enum)
- Timestamp
```

**Enumerations:**
- `VehicleStatus`: Active, Inactive, InMaintenance, Retired
- `LocationType`: GPS, CellularTriangulation, WiFi, Manual
- `AssetType`: Vehicle, Package, Equipment, Person
- `SessionStatus`: Active, Paused, Completed, Cancelled

**Value Objects:**
- Coordinates (latitude, longitude as single value)
- Distance measurements
- Timestamps and durations

**Domain Events:**
- VehicleCreatedEvent
- LocationRecordedEvent
- GeofenceEnteredEvent
- RouteCompletedEvent

**Custom Exceptions:**
```csharp
- LocationTrackingException (base)
  - VehicleNotFoundException
  - InvalidLocationException
  - AssetNotFoundException
  - TrackingSessionNotFoundException
```

**Business Rules Enforced:**
- Vehicle must have unique license plate
- Location coordinates must be valid (±90 lat, ±180 lon)
- Location accuracy must be positive
- Route waypoints must be ordered
- Session can only be deleted in specific states

### 4. Infrastructure Layer

Data persistence and external integrations.

**Entity Framework Core Setup:**
- Database first migrations
- DbContext: `ApplicationDbContext`
- Connection string configuration
- Lazy loading with explicit includes
- SQL Server optimized

**Repositories:**
- Base repository pattern with generics
- LINQ expression trees for querying
- Entity tracking management

```csharp
public interface IRepository<T> where T : Entity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<PaginatedResult<T>> GetPagedAsync(int skip, int take);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();
}
```

**Data Access Pattern:**
- Repositories abstract database queries
- Services use repositories
- Controllers use services
- No direct DbContext access outside data layer

## Design Patterns

### Repository Pattern

Abstracts data access behind a contract:

```csharp
public class VehicleRepository : BaseRepository<Vehicle>
{
    public async Task<Vehicle> GetByLicensePlateAsync(string plate)
    {
        return await _dbContext.Vehicles
            .Include(v => v.Locations)
            .FirstOrDefaultAsync(v => v.LicensePlate == plate);
    }
}
```

### Dependency Injection

Configured in `Program.cs`:

```csharp
services.AddScoped<IVehicleService, VehicleService>();
services.AddScoped<ILocationService, LocationService>();
services.AddScoped<IRepository<Vehicle>, VehicleRepository>();
```

**Lifetime Strategy:**
- `Transient`: Stateless utilities (created each time)
- `Scoped`: HTTP request scope (DbContext, services)
- `Singleton`: Application lifetime (configuration, logging)

### DTO Pattern

Decouples API from domain:

```csharp
// Domain Model
public class Vehicle : Entity
{
    public string LicensePlate { get; set; }
    public string Manufacturer { get; set; }
    // ... business logic methods
}

// DTO
public class VehicleDto
{
    public string Id { get; set; }
    public string LicensePlate { get; set; }
    public string Manufacturer { get; set; }
}

// Mapping
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Vehicle, VehicleDto>().ReverseMap();
    }
}
```

### Service Locator Alternative - Factory Pattern

For complex object creation:

```csharp
public interface IServiceFactory
{
    IVehicleService CreateVehicleService();
    ILocationService CreateLocationService();
}
```

### Specification Pattern (Optional)

For complex queries:

```csharp
public class ActiveVehiclesSpecification : Specification<Vehicle>
{
    public ActiveVehiclesSpecification()
    {
        AddCriteria(v => v.Status == VehicleStatus.Active);
        AddInclude(v => v.Locations);
    }
}

var vehicles = await _repository.GetAsync(new ActiveVehiclesSpecification());
```

## Async/Await Strategy

All I/O operations are asynchronous:

```csharp
// Service method
public async Task<VehicleDto> GetVehicleAsync(Guid id)
{
    var vehicle = await _vehicleRepository.GetByIdAsync(id);
    return _mapper.Map<VehicleDto>(vehicle);
}

// Controller action
[HttpGet("{id}")]
public async Task<ActionResult<VehicleDto>> GetVehicle(Guid id)
{
    var vehicle = await _vehicleService.GetVehicleAsync(id);
    return Ok(vehicle);
}
```

**Benefits:**
- Scalability: Thread pool not blocked on I/O
- Responsiveness: UI threads not blocked
- Database connection pooling improved

## SignalR Hub Architecture

Real-time communication via WebSockets:

```csharp
public class LocationHub : Hub
{
    // Client -> Server methods
    public async Task SendLocationUpdate(LocationDto location) { ... }
    public async Task SubscribeToVehicle(string vehicleId) { ... }
    
    // Server -> Client methods
    public async Task LocationUpdated(LocationDto location) { ... }
    public async Task VehicleStatusChanged(string vehicleId, string status) { ... }
}
```

**Group Management:**
- Subscriptions per vehicle
- Broadcast to subscribed clients only
- Automatic cleanup on disconnect

```csharp
// Subscribe to vehicle group
await Groups.AddToGroupAsync(Context.ConnectionId, $"vehicle-{vehicleId}");

// Broadcast to group
await Clients.Group($"vehicle-{vehicleId}")
    .SendAsync("LocationUpdated", location);
```

## Configuration Management

### Dependency Injection Configuration

```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
services.AddControllers();
services.AddSignalR();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
);
services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints => {
    endpoints.MapControllers();
    endpoints.MapHub<LocationHub>("/locationHub");
});
```

### Configuration Options

```csharp
// options/CachingOptions.cs
public class CachingOptions
{
    public bool Enabled { get; set; }
    public int DurationSeconds { get; set; }
}

// In appsettings.json
"Caching": {
    "Enabled": true,
    "DurationSeconds": 300
}

// Usage
var cachingOptions = configuration.GetSection("Caching").Get<CachingOptions>();
```

## Error Handling Strategy

Centralized in middleware:

```csharp
public class ErrorHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log
        // Format error response
        // Set HTTP status code
    }
}
```

**Exception Hierarchy:**
```
Exception
├── LocationTrackingException (custom base)
│   ├── VehicleNotFoundException
│   ├── InvalidLocationException
│   ├── AssetNotFoundException
│   └── TrackingSessionNotFoundException
└── Framework exceptions (re-mapped)
```

## Caching Strategy

Multi-level caching for performance:

```
┌─────────────────────┐
│   HTTP Cache        │ (Response headers)
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   L1 Cache          │ (In-Memory)
│   (LocationService) │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   L2 Cache          │ (Redis - optional)
│   (Distributed)     │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   Database          │
│   (SQL Server)      │
└─────────────────────┘
```

## Scaling Considerations

### Horizontal Scaling

For multiple servers, use:

1. **SignalR Backplane** (Redis or Service Bus):
   ```csharp
   services.AddSignalR()
       .AddStackExchangeRedis(options =>
           options.ConnectionFactory = async writer =>
           {
               return await ConnectionMultiplexer.ConnectAsync("localhost:6379");
           });
   ```

2. **Load Balancer** (sticky sessions for SignalR)

3. **Shared Cache** (Redis for distributed caching)

### Database Scaling

1. **Replication** for read-heavy queries
2. **Sharding** by vehicle ID if needed
3. **Archiving** old location data
4. **Partitioning** tables by date

### Performance Optimization

1. **Batch Operations**: Insert multiple locations in single transaction
2. **Indexes**: On frequently queried columns
3. **Query Optimization**: Use projections with `.Select()`
4. **Connection Pooling**: EF Core handles automatically
5. **Lazy Loading**: Disable and use explicit `Include()`

## Security Considerations

1. **Authentication**: API key in headers
2. **Authorization**: Role-based access control
3. **HTTPS/TLS**: All connections encrypted
4. **SQL Injection**: Protected via EF Core parameterization
5. **XSS**: DTOs serialize safely
6. **CORS**: Restricted to allowed origins
7. **Rate Limiting**: Per-IP limits
8. **Input Validation**: FluentValidation rules

## Testing Architecture

```
Unit Tests (xUnit)
├── Service tests (mocked repos)
├── DTO mapping tests
└── Utility function tests

Integration Tests
├── Database tests
├── Repository tests
└── Service tests (real DB)

E2E Tests (TestServer)
├── API endpoint tests
├── SignalR hub tests
└── Workflow tests
```

## Summary

SignalR Map Realtime combines:
- **Clean Architecture** for maintainability
- **Domain-Driven Design** for business focus
- **Dependency Injection** for testability
- **Async/Await** for scalability
- **Repository Pattern** for data abstraction
- **DTO Pattern** for API contracts
- **Middleware** for cross-cutting concerns
- **SignalR** for real-time communication
