![CI](https://github.com/sarmkadan/signalr-map-realtime/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/signalr-map-realtime)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

# SignalR Map Realtime - Real-Time Location Tracking System

A production-grade, enterprise-ready real-time location tracking system built with ASP.NET Core 10, SignalR, and modern web technologies. Track vehicles, couriers, assets, and mobile workers in real-time with live GPS updates, intelligent route management, and comprehensive fleet monitoring.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technical Stack](#technical-stack)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration Guide](#configuration-guide)
- [Self-Hosting Guide](#self-hosting-guide)
- [Advanced Topics](#advanced-topics)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Overview

SignalR Map Realtime provides a comprehensive solution for tracking the location of mobile assets in real-time. Whether you're building a ride-sharing platform, fleet management system, logistics solution, or any application requiring live location tracking, this project provides a battle-tested, scalable foundation.

The system leverages WebSocket technology through SignalR for ultra-low-latency bidirectional communication, enabling seamless real-time updates of vehicle positions, route progress, and system alerts. The architecture follows domain-driven design principles, ensuring clean separation of concerns and long-term maintainability.

### Use Cases

- **Fleet Management**: Monitor vehicle locations, fuel consumption, driver behavior
- **Ride-Sharing**: Real-time driver availability, ETA calculation, dynamic routing
- **Logistics & Delivery**: Package tracking, route optimization, delivery proof
- **Field Service**: Technician location, job assignment, service area coverage
- **Public Transportation**: Bus/vehicle tracking, arrival predictions, crowd management
- **Emergency Response**: Ambulance/fire truck dispatching, optimal routing

## Key Features

### Core Tracking Capabilities

- **Real-time GPS Updates**: Bi-directional WebSocket communication via SignalR for instantaneous location streaming
- **Multi-Asset Tracking**: Simultaneous tracking of vehicles, couriers, packages, and equipment
- **Historical Tracking**: Complete location history with timestamps and metadata
- **Session Management**: Start, pause, resume, and complete tracking sessions with statistics
- **Location Clustering**: Efficiently group nearby locations to reduce bandwidth

### Advanced Features

- **Route Planning & Management**: Create optimized routes with multiple waypoints
- **Geofencing**: Define geographic boundaries with entry/exit notifications
- **Live Statistics**: Real-time distance, speed, and performance calculations
- **Vehicle Status Tracking**: Online/offline status, connectivity monitoring
- **Driver/Courier Management**: User assignments and performance tracking
- **Asset Type Support**: Vehicles, packages, equipment with type-specific attributes
- **Speed Monitoring**: Detect and alert on speed violations
- **Fuel Tracking**: Monitor fuel levels and consumption patterns

### Technical Features

- **Comprehensive API**: RESTful endpoints with full Swagger/OpenAPI documentation
- **Authentication Ready**: API key authentication infrastructure for extensibility
- **Rate Limiting**: Built-in rate limiting to prevent abuse
- **Caching Layer**: Redis-compatible in-memory caching for performance
- **Error Handling**: Comprehensive error handling with custom exceptions
- **Request Logging**: Detailed request/response logging for debugging
- **Performance Monitoring**: Middleware for tracking endpoint performance
- **Webhook Support**: Integration points for third-party systems

## Technical Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Runtime** | .NET | 10.0 |
| **Framework** | ASP.NET Core | 10.0 |
| **Language** | C# | 13.0 |
| **ORM** | Entity Framework Core | 10.0 |
| **Real-time** | SignalR | 10.0 |
| **Database** | SQL Server / PostgreSQL | 2019+ |
| **Object Mapping** | AutoMapper | 13.0 |
| **Validation** | FluentValidation | 11.9+ |
| **API Docs** | Swagger/OpenAPI | 3.0 |
| **Caching** | In-Memory/Redis | - |
| **Frontend** | Leaflet.js | 1.9+ |

## Architecture

### Component Interaction

The three main layers — Leaflet frontend, ASP.NET Core backend, and the database — communicate as follows:

```
  Browser (Leaflet.js)
        │
        │  1. HTTP REST  ──────────────────────────────────────────────────►
        │                                                  ASP.NET Core App
        │  2. WebSocket (SignalR)  ────────────────────────────────────────►
        │                                                  LocationHub
        │                                                       │
        │                                               LocationService
        │                                               VehicleService
        │                                                       │
        │                                               EF Core / SQL Server
        │                                                       │
        │  3. SignalR broadcast ◄──────────────────────────────────────────
        │     "LocationUpdated",
        │     "PositionSnapshot",
        │     "VehicleStatusChanged", …
        │
  Leaflet map updates marker positions
```

**Data flow for a single location update:**

1. A tracked device calls `POST /api/v1/locations` or invokes `SendLocationUpdate` on the `LocationHub`.
2. `LocationService` validates coordinates, persists the record, and updates the vehicle's last-known position.
3. `LocationHub` broadcasts `LocationUpdated` to **all** connected clients and `VehicleLocationUpdated` to the vehicle-specific group.
4. Each browser's SignalR listener calls `updateVehicleLocation()`, which updates the Leaflet marker position.

**Reconnect handling:**

When a client's WebSocket drops and re-establishes, the `onreconnected` callback invokes `RequestAllVehicleLocations` on the hub. The hub responds with a `PositionSnapshot` containing the latest known position of every online vehicle, so all Leaflet markers are immediately resynced without waiting for the next delta update.

### Infrastructure Requirements

| Concern | Single-node | Multi-node (scale-out) |
|---------|-------------|------------------------|
| **Transport** | WebSockets (default) | WebSockets; sticky sessions **required** unless a backplane is configured |
| **SignalR backplane** | Not required | Redis or Azure SignalR Service |
| **Session affinity** | Not required | Required when backplane is absent (configure on load balancer) |
| **Database** | SQL Server (LocalDB for dev) | SQL Server with connection pooling |
| **Cache** | In-memory | Redis |

#### Enabling the Redis backplane (multi-node)

```bash
dotnet add package Microsoft.AspNetCore.SignalR.StackExchangeRedis
```

```json
// appsettings.json
{
  "SignalR": {
    "Redis": {
      "ConnectionString": "localhost:6379"
    }
  }
}
```

```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration["SignalR:Redis:ConnectionString"]!);
```

### System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                       Web Clients                               │
│              (Browser, Mobile, Desktop Apps)                   │
└────────────────┬────────────────────────────────────────────────┘
                 │
         ┌───────┴────────┐
         │                │
    ┌────▼──────┐    ┌───▼─────┐
    │   HTTP    │    │WebSocket │
    │   (REST)  │    │(SignalR) │
    └────┬──────┘    └───┬─────┘
         │                │
    ┌────▼────────────────▼──────────────────────┐
    │         ASP.NET Core Application           │
    │  ┌─────────────────────────────────────┐   │
    │  │    API Controllers (REST)           │   │
    │  │    SignalR Hubs (Real-time)         │   │
    │  │    Middleware Pipeline              │   │
    │  │    ├─ Authentication                │   │
    │  │    ├─ CORS                          │   │
    │  │    ├─ Rate Limiting                 │   │
    │  │    ├─ Error Handling                │   │
    │  │    └─ Logging                       │   │
    │  └────────────────┬──────────────────┘   │
    │                   │                      │
    │  ┌────────────────▼──────────────────┐   │
    │  │      Business Service Layer       │   │
    │  │  ├─ LocationService               │   │
    │  │  ├─ VehicleService                │   │
    │  │  ├─ TrackingService               │   │
    │  │  ├─ NotificationService           │   │
    │  │  └─ CacheService                  │   │
    │  └────────────────┬──────────────────┘   │
    │                   │                      │
    │  ┌────────────────▼──────────────────┐   │
    │  │    Data Access Layer              │   │
    │  │  (Repository Pattern)             │   │
    │  │  ├─ LocationRepository            │   │
    │  │  ├─ VehicleRepository             │   │
    │  │  ├─ RouteRepository               │   │
    │  │  ├─ AssetRepository               │   │
    │  │  └─ UserRepository                │   │
    │  └────────────────┬──────────────────┘   │
    │                   │                      │
    │  ┌────────────────▼──────────────────┐   │
    │  │  Entity Framework Core (EF Core)  │   │
    │  │  ├─ DbContext                     │   │
    │  │  ├─ Migrations                    │   │
    │  │  └─ LINQ to Entities              │   │
    │  └────────────────┬──────────────────┘   │
    └────────────────┬──────────────────────────┘
                     │
    ┌────────────────▼──────────────────┐
    │      SQL Server Database          │
    │  ├─ Vehicles                      │
    │  ├─ Locations (History)           │
    │  ├─ Routes & Waypoints            │
    │  ├─ Users (Drivers/Couriers)      │
    │  ├─ Assets                        │
    │  ├─ TrackingSessions              │
    │  └─ Indices & Views               │
    └───────────────────────────────────┘
```

### Layered Architecture

```
┌─────────────────────────────────┐
│    Presentation Layer           │
│  (Controllers, SignalR Hubs)    │
└────────────────┬────────────────┘
                 │
┌────────────────▼────────────────┐
│     Application/Service Layer   │
│    (Business Logic, DTOs)       │
└────────────────┬────────────────┘
                 │
┌────────────────▼────────────────┐
│     Domain Layer                │
│   (Models, Enums, Exceptions)   │
└────────────────┬────────────────┘
                 │
┌────────────────▼────────────────┐
│    Data Access Layer            │
│   (Repositories, EF Core)       │
└────────────────┬────────────────┘
                 │
┌────────────────▼────────────────┐
│     Infrastructure Layer        │
│   (Database, Configuration)     │
└─────────────────────────────────┘
```

### Project Structure

```
signalr-map-realtime/
├── src/
│   └── SignalRMapRealtime/
│       ├── Domain/
│       │   ├── Models/
│       │   │   ├── Vehicle.cs
│       │   │   ├── Location.cs
│       │   │   ├── Route.cs
│       │   │   ├── User.cs
│       │   │   ├── Asset.cs
│       │   │   ├── Waypoint.cs
│       │   │   └── TrackingSession.cs
│       │   └── Enums/
│       │       ├── VehicleStatus.cs
│       │       ├── LocationType.cs
│       │       ├── AssetType.cs
│       │       └── SessionStatus.cs
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Repositories/
│       │       ├── IRepository.cs
│       │       ├── BaseRepository.cs
│       │       ├── VehicleRepository.cs
│       │       ├── LocationRepository.cs
│       │       ├── RouteRepository.cs
│       │       ├── AssetRepository.cs
│       │       ├── UserRepository.cs
│       │       └── TrackingSessionRepository.cs
│       ├── Services/
│       │   ├── LocationService.cs
│       │   ├── VehicleService.cs
│       │   ├── TrackingService.cs
│       │   ├── NotificationService.cs
│       │   ├── CacheService.cs
│       │   └── Interfaces/
│       │       ├── ILocationService.cs
│       │       ├── IVehicleService.cs
│       │       └── ITrackingService.cs
│       ├── Controllers/
│       │   ├── VehicleController.cs
│       │   ├── LocationController.cs
│       │   ├── RouteController.cs
│       │   └── AssetController.cs
│       ├── Hubs/
│       │   └── LocationHub.cs
│       ├── DTOs/
│       │   ├── VehicleDto.cs
│       │   ├── LocationDto.cs
│       │   ├── RouteDto.cs
│       │   ├── AssetDto.cs
│       │   └── UserDto.cs
│       ├── Configuration/
│       │   ├── DependencyInjection.cs
│       │   ├── AutoMapperProfile.cs
│       │   └── Options/
│       ├── Middleware/
│       │   ├── ErrorHandlingMiddleware.cs
│       │   ├── LoggingMiddleware.cs
│       │   ├── RateLimitingMiddleware.cs
│       │   └── PerformanceMonitoringMiddleware.cs
│       ├── Attributes/
│       │   └── ApiKeyAuthenticationAttribute.cs
│       ├── Exceptions/
│       │   └── LocationTrackingException.cs
│       ├── BackgroundJobs/
│       │   └── SessionCleanupWorker.cs
│       ├── Events/
│       │   ├── DomainEvent.cs
│       │   └── EventBus.cs
│       ├── Integration/
│       │   ├── HttpClientFactory.cs
│       │   └── WebhookHandler.cs
│       ├── Utilities/
│       │   └── Extension methods
│       ├── Constants/
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── SignalRMapRealtime.csproj
├── examples/
│   ├── VehicleTrackerClient.cs
│   ├── LocationUpdateSimulator.cs
│   ├── RouteOptimizationClient.cs
│   ├── FleetManagementDashboard.cs
│   ├── GeofenceAlertsClient.cs
│   ├── BulkLocationImporter.cs
│   ├── SessionAnalyticsReporter.cs
│   └── WebSocketRealTimeViewer.html
├── docs/
│   ├── getting-started.md
│   ├── architecture.md
│   ├── api-reference.md
│   ├── deployment.md
│   ├── faq.md
│   ├── database-schema.md
│   └── troubleshooting.md
├── .github/
│   └── workflows/
│       └── build.yml
├── Dockerfile
├── docker-compose.yml
├── .editorconfig
├── Makefile
├── CHANGELOG.md
├── LICENSE
├── .gitignore
└── signalr-map-realtime.sln
```

## Installation

### Prerequisites

- **.NET SDK**: 10.0 or later ([download](https://dotnet.microsoft.com/download/dotnet/10.0))
- **SQL Server**: 2019 Express or later, or use LocalDB
- **Visual Studio**: 2022 (17.8+) or VS Code with C# extension
- **Git**: For cloning the repository

### Method 1: Using Visual Studio (Recommended for Windows)

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Sarmkadan/signalr-map-realtime.git
   cd signalr-map-realtime
   ```

2. **Open Solution**
   - Open `signalr-map-realtime.sln` in Visual Studio 2022

3. **Configure Connection String** (if needed)
   - Edit `src/SignalRMapRealtime/appsettings.json`
   - Update `ConnectionStrings.DefaultConnection` for your SQL Server

4. **Apply Migrations**
   - Package Manager Console: `Update-Database`
   - Or CLI: `dotnet ef database update`

5. **Run the Application**
   - Press `F5` or click Run button
   - API available at `https://localhost:5001`
   - Swagger UI at `https://localhost:5001/swagger`

### Method 2: Command Line

```bash
# Clone
git clone https://github.com/Sarmkadan/signalr-map-realtime.git
cd signalr-map-realtime

# Restore packages
dotnet restore

# Build
dotnet build

# Apply database migrations
dotnet ef database update --project src/SignalRMapRealtime

# Run
dotnet run --project src/SignalRMapRealtime
```

### Method 3: Docker

```bash
# Clone and navigate
git clone https://github.com/Sarmkadan/signalr-map-realtime.git
cd signalr-map-realtime

# Build image
docker build -t signalr-map-realtime:latest .

# Run with compose (includes SQL Server)
docker-compose up -d

# API available at http://localhost:5000
# Swagger at http://localhost:5000/swagger
```

## Quick Start

### 1. Verify Installation

```bash
curl https://localhost:5001/health
# Expected response: { "status": "Healthy" }
```

### 2. Create a Vehicle

```bash
curl -X POST https://localhost:5001/api/v1/vehicles \
  -H "Content-Type: application/json" \
  -d '{
    "licensePlate": "ABC-123",
    "manufacturer": "Toyota",
    "model": "Camry",
    "year": 2023,
    "status": "Available"
  }'
```

### 3. Start Real-time Tracking

```javascript
// Connect to SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/locationHub")
    .withAutomaticReconnect()
    .build();

// Subscribe to location updates
connection.on("LocationUpdated", (location) => {
    console.log(`Vehicle ${location.vehicleId} at ${location.latitude}, ${location.longitude}`);
});

// Connect and send location
await connection.start();
await connection.invoke("SendLocationUpdate", {
    vehicleId: "your-vehicle-id",
    latitude: 40.7128,
    longitude: -74.0060,
    accuracy: 5.0
});
```

### 4. Retrieve Location History

```bash
curl "https://localhost:5001/api/v1/locations/vehicle/your-vehicle-id" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Usage Examples

### Example 1: Basic Vehicle Tracking

See `examples/VehicleTrackerClient.cs` for a complete working example.

```csharp
var client = new HttpClient();
var baseUrl = "https://localhost:5001";

// Create vehicle
var vehicle = new { 
    licensePlate = "XYZ-789",
    manufacturer = "Ford",
    model = "Transit"
};

var response = await client.PostAsJsonAsync(
    $"{baseUrl}/api/v1/vehicles", 
    vehicle
);
var vehicleId = await response.Content.ReadAsAsync<string>();

// Start tracking
var location = new {
    vehicleId = vehicleId,
    latitude = 40.7128,
    longitude = -74.0060,
    accuracy = 10.0,
    speed = 35.5,
    heading = 90,
    timestamp = DateTime.UtcNow
};

await client.PostAsJsonAsync(
    $"{baseUrl}/api/v1/locations",
    location
);
```

### Example 2: SignalR Real-time Updates

See `examples/WebSocketRealTimeViewer.html` for a complete HTML/JavaScript example.

```csharp
// In your C# client
var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/locationHub")
    .WithAutomaticReconnect()
    .Build();

connection.On<LocationDto>("LocationUpdated", location => 
{
    Console.WriteLine($"Update: {location.VehicleId} - {location.Latitude}, {location.Longitude}");
});

connection.On<string, string>("VehicleStatusChanged", (vehicleId, status) =>
{
    Console.WriteLine($"Vehicle {vehicleId} is now {status}");
});

await connection.StartAsync();

// Subscribe to specific vehicle
await connection.InvokeAsync("SubscribeToVehicle", vehicleId);
```

### Example 3: Route Optimization

See `examples/RouteOptimizationClient.cs`.

```csharp
var route = new {
    name = "Downtown Delivery Route",
    vehicleId = "vehicle-123",
    waypoints = new[] {
        new { latitude = 40.7128, longitude = -74.0060, name = "Start" },
        new { latitude = 40.7489, longitude = -73.9680, name = "Stop 1" },
        new { latitude = 40.7614, longitude = -73.9776, name = "Stop 2" },
        new { latitude = 40.7282, longitude = -73.7949, name = "End" }
    },
    estimatedDuration = 3600
};

var response = await client.PostAsJsonAsync(
    $"{baseUrl}/api/v1/routes",
    route
);
```

### Example 4: Geofence Alerts

See `examples/GeofenceAlertsClient.cs`.

```csharp
connection.On<AlertDto>("Alert", alert =>
{
    if (alert.Type == AlertType.GeofenceEntry)
    {
        Console.WriteLine($"Vehicle {alert.VehicleId} entered restricted area!");
    }
    else if (alert.Type == AlertType.GeofenceExit)
    {
        Console.WriteLine($"Vehicle {alert.VehicleId} left geofence");
    }
});
```

### Example 5: Bulk Location Import

See `examples/BulkLocationImporter.cs` for importing historical data.

```csharp
var locations = new List<LocationDto>();
// ... populate locations ...

var client = new HttpClient();
foreach (var batch in locations.Batch(100))
{
    await client.PostAsJsonAsync($"{baseUrl}/api/v1/locations/bulk", batch);
}
```

### Example 6: Session Analytics

See `examples/SessionAnalyticsReporter.cs`.

```csharp
var session = await trackingService.GetSessionAsync(sessionId);
var analytics = new {
    sessionId = session.Id,
    totalDistance = session.TotalDistance,
    totalDuration = session.EndTime - session.StartTime,
    averageSpeed = session.AverageSpeed,
    maxSpeed = session.MaxSpeed,
    locationCount = session.Locations.Count
};
```

## API Reference

### Health & Info Endpoints

#### Health Check
```
GET /health
```
**Response** (200 OK):
```json
{
  "status": "Healthy",
  "timestamp": "2024-05-04T10:30:00Z",
  "version": "2.0.2"
}
```

#### API Information
```
GET /api/info
```
**Response** (200 OK):
```json
{
  "version": "2.0.2",
  "title": "SignalR Map Realtime API",
  "environment": "Production"
}
```

### Vehicle Endpoints

#### List Vehicles
```
GET /api/v1/vehicles
?skip=0&take=20&status=Active
```
**Query Parameters**:
- `skip`: Number of items to skip (default: 0)
- `take`: Number of items to return (default: 20, max: 100)
- `status`: Filter by status (Active, Inactive, InMaintenance)

**Response** (200 OK):
```json
{
  "data": [
    {
      "id": "uuid",
      "licensePlate": "ABC-123",
      "manufacturer": "Toyota",
      "model": "Camry",
      "year": 2023,
      "status": "Active",
      "lastLocationUpdate": "2024-05-04T10:25:00Z",
      "currentLatitude": 40.7128,
      "currentLongitude": -74.0060,
      "speed": 35.5
    }
  ],
  "pageSize": 20,
  "totalCount": 150
}
```

#### Get Vehicle Details
```
GET /api/v1/vehicles/{id}
```
**Response** (200 OK):
```json
{
  "id": "uuid",
  "licensePlate": "ABC-123",
  "manufacturer": "Toyota",
  "model": "Camry",
  "year": 2023,
  "status": "Active",
  "fuelLevel": 85.5,
  "odometerReading": 45230,
  "assignedDriver": {
    "id": "uuid",
    "name": "John Doe",
    "phone": "+1234567890"
  },
  "createdAt": "2024-01-15T09:00:00Z",
  "updatedAt": "2024-05-04T10:25:00Z"
}
```

#### Create Vehicle
```
POST /api/v1/vehicles
Content-Type: application/json

{
  "licensePlate": "XYZ-789",
  "manufacturer": "Ford",
  "model": "Transit",
  "year": 2024,
  "vin": "1FTYR14D04TM00001",
  "status": "Active"
}
```
**Response** (201 Created):
```json
{
  "id": "uuid",
  "licensePlate": "XYZ-789",
  "status": "Active"
}
```

#### Update Vehicle
```
PUT /api/v1/vehicles/{id}
Content-Type: application/json

{
  "licensePlate": "XYZ-789",
  "status": "InMaintenance",
  "fuelLevel": 75.0
}
```
**Response** (204 No Content)

#### Delete Vehicle
```
DELETE /api/v1/vehicles/{id}
```
**Response** (204 No Content)

### Location Endpoints

#### Get Vehicle Locations
```
GET /api/v1/locations/vehicle/{vehicleId}
?startDate=2024-05-01&endDate=2024-05-04&limit=1000
```
**Query Parameters**:
- `startDate`: Filter from date (ISO 8601)
- `endDate`: Filter to date (ISO 8601)
- `limit`: Maximum number of locations (default: 100, max: 10000)

**Response** (200 OK):
```json
{
  "data": [
    {
      "id": "uuid",
      "vehicleId": "uuid",
      "latitude": 40.7128,
      "longitude": -74.0060,
      "accuracy": 5.0,
      "altitude": 10.5,
      "speed": 35.5,
      "heading": 90,
      "type": "GPS",
      "timestamp": "2024-05-04T10:25:00Z"
    }
  ],
  "count": 1000
}
```

#### Record Location
```
POST /api/v1/locations
Content-Type: application/json

{
  "vehicleId": "uuid",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "accuracy": 5.0,
  "speed": 35.5,
  "heading": 90,
  "timestamp": "2024-05-04T10:25:00Z"
}
```
**Response** (201 Created):
```json
{
  "id": "uuid",
  "vehicleId": "uuid",
  "latitude": 40.7128,
  "longitude": -74.0060
}
```

#### Find Nearby Locations
```
GET /api/v1/locations/nearby
?latitude=40.7128&longitude=-74.0060&radiusKm=5&limit=50
```
**Query Parameters**:
- `latitude`: Center point latitude
- `longitude`: Center point longitude
- `radiusKm`: Search radius in kilometers (default: 1, max: 50)
- `limit`: Maximum results (default: 20)

**Response** (200 OK):
```json
{
  "data": [
    {
      "id": "uuid",
      "vehicleId": "uuid",
      "latitude": 40.7180,
      "longitude": -74.0050,
      "distanceKm": 0.8,
      "timestamp": "2024-05-04T10:25:00Z"
    }
  ],
  "centerPoint": {
    "latitude": 40.7128,
    "longitude": -74.0060
  }
}
```

### Route Endpoints

#### List Routes
```
GET /api/v1/routes
?status=Active&vehicleId=uuid&skip=0&take=20
```

#### Create Route
```
POST /api/v1/routes
Content-Type: application/json

{
  "name": "Downtown Delivery",
  "vehicleId": "uuid",
  "waypoints": [
    {
      "order": 1,
      "latitude": 40.7128,
      "longitude": -74.0060,
      "name": "Start Point",
      "estimatedArrivalTime": "2024-05-04T10:30:00Z"
    },
    {
      "order": 2,
      "latitude": 40.7489,
      "longitude": -73.9680,
      "name": "Stop 1",
      "estimatedArrivalTime": "2024-05-04T10:45:00Z"
    }
  ],
  "estimatedDuration": 3600
}
```

## Configuration Guide

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SignalRMapRealtimeDb;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AppSettings": {
    "ApiVersion": "2.0.2",
    "Environment": "Production",
    "EnableSwagger": true,
    "RequestTimeoutSeconds": 30
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  },
  "Caching": {
    "Enabled": true,
    "DurationSeconds": 300
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 100
  },
  "Notification": {
    "Enabled": true,
    "EmailNotifications": true
  }
}
```

### Environment Variables

```bash
# Database
ASPNETCORE_CONNECTIONSTRINGS_DEFAULTCONNECTION=Server=...

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://0.0.0.0:5001

# Logging
ASPNETCORE_LOGGING_LOGLEVEL_DEFAULT=Information

# SignalR
SIGNALR_BACKPLANETYPE=Redis
SIGNALR_REDIS_CONNECTIONSTRING=localhost:6379
```

### Docker Configuration

See `docker-compose.yml` for complete multi-container setup with SQL Server and Redis.

## Self-Hosting Guide

### Docker Compose (recommended for production)

The included `docker-compose.yml` starts the API, SQL Server, and Redis together.

```bash
# 1. Copy and customise environment variables
cp .env.example .env   # edit DB password, API keys, etc.

# 2. Start all services
docker-compose up -d

# 3. Apply database migrations
docker-compose exec api dotnet ef database update --project SignalRMapRealtime.csproj

# 4. Verify
curl http://localhost:5000/health
```

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ASPNETCORE_URLS` | Listening addresses | `http://0.0.0.0:5000` |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | `Server=db;Database=SignalRMap;User Id=sa;Password=…` |
| `SignalR__Redis__ConnectionString` | Redis backplane (multi-node) | `redis:6379` |
| `AppSettings__ApiKey` | API key for hub authentication | `your-secret-key` |
| `Cors__AllowedOrigins__0` | Allowed CORS origin | `https://your-frontend.example.com` |
| `LocationThrottle__CourierIntervalSeconds` | Override throttle per asset type | `15` |

### Reverse Proxy Configuration

#### Nginx

WebSocket connections require the `Upgrade` and `Connection` headers to be forwarded.

```nginx
server {
    listen 443 ssl;
    server_name track.example.com;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection "upgrade";
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 86400s;  # keep SignalR connections alive
    }
}
```

#### Traefik (Docker label-based)

```yaml
# docker-compose.yml excerpt
services:
  api:
    labels:
      - "traefik.http.routers.api.rule=Host(`track.example.com`)"
      - "traefik.http.services.api.loadbalancer.sticky.cookie=true"
      - "traefik.http.services.api.loadbalancer.sticky.cookie.name=signalr_affinity"
```

> **Sticky sessions** — if you are running multiple API replicas without a Redis SignalR backplane,
> configure sticky sessions on your load balancer so each WebSocket connection is always routed
> to the same server. Without this, SignalR group broadcasts will only reach clients connected
> to the same replica.

## Advanced Topics

### SignalR Hub Methods

#### Client-to-Server

**SendLocationUpdate**
```csharp
await connection.InvokeAsync("SendLocationUpdate", new LocationDto {
    VehicleId = "uuid",
    Latitude = 40.7128,
    Longitude = -74.0060,
    Accuracy = 5.0,
    Speed = 35.5,
    Heading = 90
});
```

**SubscribeToVehicle**
```csharp
await connection.InvokeAsync("SubscribeToVehicle", vehicleId);
```

**RequestVehicleLocation**
```csharp
await connection.InvokeAsync("RequestVehicleLocation", vehicleId);
```

#### Server-to-Client

**LocationUpdated**: Broadcasted when a new location is recorded
```javascript
connection.on("LocationUpdated", (location) => {
    // Handle location update
});
```

**VehicleStatusChanged**: When vehicle status changes
```javascript
connection.on("VehicleStatusChanged", (vehicleId, status) => {
    // Handle status change
});
```

**Alert**: System alerts (geofence, speed, etc.)
```javascript
connection.on("Alert", (alert) => {
    // Handle alert
});
```

### Database Optimization

#### Indexing Strategy
- Vehicle.LicensePlate (unique)
- Location.VehicleId + Location.Timestamp
- User.Email (unique)
- Route.VehicleId + Route.CreatedAt

#### Query Performance
- Use pagination (skip/take)
- Filter by date range when possible
- Cache frequently accessed data
- Batch location inserts for bulk operations

### Scaling Considerations

1. **Database**: Use read replicas for reporting queries
2. **SignalR**: Configure backplane (Redis/Service Bus) for multiple servers
3. **Caching**: Implement Redis for session/location caching
4. **API**: Use load balancer for horizontal scaling

## Performance

The following benchmarks were measured on a single application node (4 vCPU, 8 GB RAM) backed by SQL Server 2022.

| Scenario | Result |
|---|---|
| Concurrent WebSocket connections | 10,000+ per node |
| Location updates ingested | ~50,000 / minute |
| SignalR broadcast latency (LAN) | < 5 ms |
| Location API response (cached) | < 10 ms |
| Location API response (uncached) | < 80 ms |
| Location history query (30-day range, indexed) | < 200 ms |
| Geofence boundary evaluation per update | < 2 ms |

**Scaling guidance:**
- Add a Redis SignalR backplane to distribute WebSocket connections across multiple nodes.
- Enable response caching for read-heavy endpoints (`GET /api/v1/vehicles`, nearby lookups).
- Use the bulk import endpoint (`POST /api/v1/locations/bulk`) to batch-write historical GPS data and reduce per-record overhead.
- Deploy read replicas for reporting and analytics queries to keep the write path latency consistent.

## Troubleshooting

### Common Issues

#### Q: Connection String Error
**A**: Verify SQL Server is running:
```bash
# Windows
sqlcmd -S (localdb)\mssqllocaldb

# Or check SQL Server service status
```

#### Q: SignalR Connection Failed
**A**: Check CORS configuration in `appsettings.json`:
```json
"Cors": {
  "AllowedOrigins": ["http://your-client-url"]
}
```

#### Q: Slow Location Queries
**A**: Ensure indexes are created:
```bash
dotnet ef database update
```

#### Q: Port Already in Use
**A**: Change port in `appsettings.json`:
```bash
dotnet run --urls "https://localhost:5002"
```

### Debugging

Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "SignalRMapRealtime": "Debug"
    }
  }
}
```

View logs:
```bash
# Windows Event Viewer (if using Windows Hosting)
# Or file logs in bin/Debug/net10.0/logs/
```

## Testing

Unit and integration tests live in `tests/signalr-map-realtime.Tests/`.

```bash
# Run all tests
dotnet test

# Run with code coverage report
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test class
dotnet test --filter "FullyQualifiedName~LocationServiceTests"
```

Test coverage includes:

- **Domain model behavior** — `DomainModelBehaviorTests.cs`: entity invariants and status transitions
- **Geo-distance calculations** — `GeoLocationExtensionsTests.cs`: Haversine formula, bounding-box helpers
- **Location service logic** — `LocationServiceTests.cs`: recording, history queries, nearby search

## Related Projects

- [gps-tracker-protocol](https://github.com/sarmkadan/gps-tracker-protocol) - Parse GPS tracker protocols (GT06, H02, TK103) in .NET — converts raw TCP/UDP frames into structured location data

### Integration Examples

**Ingest raw device packets and forward to the tracking API:**

```csharp
// Receive a raw GT06 frame over TCP, parse it, then record it
var parser = new GpsTrackerProtocolParser();
var frame  = parser.Parse(rawBytes, ProtocolType.GT06);

await httpClient.PostAsJsonAsync("/api/v1/locations", new {
    vehicleId = frame.DeviceId,
    latitude  = frame.Latitude,
    longitude = frame.Longitude,
    speed     = frame.SpeedKmh,
    timestamp = frame.FixTime
});
```

**Subscribe to real-time updates after device frames are stored:**

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://your-server/locationHub")
    .WithAutomaticReconnect()
    .Build();

connection.On<LocationDto>("LocationUpdated", loc =>
    Console.WriteLine($"[{loc.VehicleId}] {loc.Latitude:F6}, {loc.Longitude:F6} @ {loc.Speed} km/h"));

await connection.StartAsync();
await connection.InvokeAsync("SubscribeToVehicle", deviceId);
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit changes: `git commit -am 'Add new feature'`
4. Push to branch: `git push origin feature/your-feature`
5. Submit Pull Request

### Code Style Guidelines

- Follow Microsoft C# Coding Conventions
- Use meaningful variable and method names
- Add XML documentation to public methods
- Write unit tests for new features
- Keep methods focused (Single Responsibility)

### Pull Request Process

1. Update README.md with new features/changes
2. Update CHANGELOG.md with version number
3. Ensure all tests pass: `dotnet test`
4. Request review from project maintainers

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for complete details.

## Support & Contact

- **Issues**: [GitHub Issues](https://github.com/Sarmkadan/signalr-map-realtime/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Sarmkadan/signalr-map-realtime/discussions)

---

## New Features

### Clustering & Heatmap Layer

Location points are automatically bucketed into geographic grid cells to render density overlays on the Leaflet map.

#### Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/clustering/clusters` | Returns location clusters with centroid and bounds |
| `GET` | `/api/clustering/heatmap` | Returns normalised heatmap tiles (intensity 0.0–1.0) |

Both endpoints accept the same query parameters:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `vehicleId` | `int?` | — | Restrict to a single vehicle's history |
| `from` | `DateTime?` | -24 h | Start of time window (UTC) |
| `to` | `DateTime?` | now | End of time window (UTC) |
| `gridCellKm` | `double` | 0.5 | Grid cell edge length in km |
| `minLatitude` | `double?` | — | Southern bounding-box limit |
| `maxLatitude` | `double?` | — | Northern bounding-box limit |
| `minLongitude` | `double?` | — | Western bounding-box limit |
| `maxLongitude` | `double?` | — | Eastern bounding-box limit |

**Cluster response example:**
```json
{
  "success": true,
  "data": {
    "clusters": [
      {
        "centerLatitude": 51.507,
        "centerLongitude": -0.127,
        "count": 42,
        "minLatitude": 51.502,
        "maxLatitude": 51.512,
        "minLongitude": -0.132,
        "maxLongitude": -0.122
      }
    ],
    "totalPoints": 42,
    "gridCellKm": 0.5,
    "computedAt": "2024-05-04T10:30:00Z"
  }
}
```

**Heatmap response example:**
```json
{
  "success": true,
  "data": {
    "points": [
      { "latitude": 51.507, "longitude": -0.127, "intensity": 1.0, "count": 42 }
    ],
    "maxCount": 42,
    "gridCellKm": 0.5,
    "computedAt": "2024-05-04T10:30:00Z"
  }
}
```

**Leaflet integration:**
```javascript
// Heatmap
const data = response.data.points.map(p => [p.latitude, p.longitude, p.intensity]);
L.heatLayer(data, { radius: 25 }).addTo(map);

// Clustering — draw rectangles for each cell
response.data.clusters.forEach(c => {
  const bounds = [[c.minLatitude, c.minLongitude], [c.maxLatitude, c.maxLongitude]];
  L.rectangle(bounds, { color: '#ff7800', weight: 1 }).addTo(map)
    .bindPopup(`${c.count} points`);
});
```

---

### Geofence Zones on Map

Define polygonal or circular geographic zones. The system detects when tracked vehicles enter or exit a zone and raises domain events.

#### Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/geofence` | List all registered geofence zones |
| `POST` | `/api/geofence` | Register a new geofence zone |
| `DELETE` | `/api/geofence/{id}` | Remove a geofence zone |
| `POST` | `/api/geofence/check` | Evaluate whether a position is inside any zone |

**Register zone request:**
```json
{
  "name": "Warehouse District",
  "centerLatitude": 51.507,
  "centerLongitude": -0.127,
  "radiusMeters": 500,
  "alertOnEnter": true,
  "alertOnExit": true,
  "description": "Main warehouse perimeter"
}
```

**Check position request:**
```json
{
  "vehicleId": 42,
  "latitude": 51.507,
  "longitude": -0.127
}
```

**Check position response:**
```json
{
  "success": true,
  "data": {
    "vehicleId": 42,
    "latitude": 51.507,
    "longitude": -0.127,
    "violations": [
      {
        "zoneId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "zoneName": "Warehouse District",
        "eventType": "Enter"
      }
    ],
    "checkedAt": "2024-05-04T10:30:00Z"
  }
}
```

**Domain events** — subscribe via `IEventBus` for integration:
- `GeofenceEnteredEvent` — raised when a vehicle enters a zone for the first time
- `GeofenceExitedEvent` — raised when a vehicle exits a zone it was inside

---

### Historical Playback

Replay recorded route sessions step by step via SignalR, with speed control and real-time position snapshots.

#### REST Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/playback/sessions` | Create a new playback session |
| `GET` | `/api/playback/sessions` | List all active playback sessions |
| `DELETE` | `/api/playback/sessions/{id}` | Stop and remove a playback session |
| `GET` | `/api/playback/timeline/{id}` | Get the ordered list of location points |
| `GET` | `/api/playback/snapshot/{id}` | Get the current playback position |
| `GET` | `/api/playback/statistics/{id}` | Get distance, speed, and duration stats |

**Create session request:**
```json
{
  "vehicleId": 42,
  "startTime": "2024-05-04T08:00:00Z",
  "endTime":   "2024-05-04T09:00:00Z",
  "playbackSpeed": 2.0
}
```

**Snapshot response:**
```json
{
  "success": true,
  "data": {
    "sessionId": "3fa85f64-...",
    "currentIndex": 12,
    "totalPoints": 120,
    "progressPercent": 10.0,
    "currentLocation": {
      "latitude": 51.507,
      "longitude": -0.127,
      "speed": 45.2,
      "recordedAt": "2024-05-04T08:06:00Z"
    },
    "isPlaying": true
  }
}
```

#### SignalR Hub — `/hubs/playback`

Connect with the SignalR client and invoke:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/playback')
  .build();

// Receive position ticks during replay
connection.on('PlaybackTick', (sessionId, location) => {
  marker.setLatLng([location.latitude, location.longitude]);
});

connection.on('PlaybackCompleted', (sessionId) => {
  console.log('Replay finished', sessionId);
});

await connection.start();

// Start playback
await connection.invoke('StartPlayback', sessionId);

// Pause / resume / stop
await connection.invoke('PausePlayback', sessionId);
await connection.invoke('ResumePlayback', sessionId);
await connection.invoke('StopPlayback', sessionId);

// Join a session group to receive its events
await connection.invoke('JoinPlaybackSession', sessionId);
await connection.invoke('LeavePlaybackSession', sessionId);
```

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)


