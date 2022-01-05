# SignalR Map Realtime

A real-time location tracking system using ASP.NET Core SignalR and Leaflet maps. Track vehicles, couriers, and assets in real-time with live location updates, route management, and fleet monitoring capabilities.

## Features

- **Real-time Location Tracking**: Live GPS coordinates streaming via SignalR
- **Vehicle Management**: Full CRUD operations for vehicles with status tracking
- **Route Planning**: Create and manage routes with waypoints
- **Asset Tracking**: Track multiple assets with location history
- **User Management**: Driver and courier management with assignment tracking
- **Live Statistics**: Distance calculations, speed monitoring, fuel tracking
- **Session Management**: Start, pause, resume, and complete tracking sessions
- **Geofencing**: Define geographic boundaries and receive alerts
- **SignalR Hub**: Bi-directional real-time communication
- **Comprehensive API**: RESTful API with Swagger/OpenAPI documentation

## Technology Stack

- **.NET 10** - Latest .NET framework
- **Entity Framework Core 10** - ORM for database operations
- **SQL Server** - Database
- **SignalR** - Real-time communication
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Data validation

## Project Structure

```
signalr-map-realtime/
├── src/
│   └── SignalRMapRealtime/
│       ├── Domain/
│       │   ├── Models/           # Entity models
│       │   └── Enums/            # Status and type enumerations
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Repositories/     # Data access layer
│       ├── Services/             # Business logic layer
│       ├── DTOs/                 # Data transfer objects
│       ├── Hubs/                 # SignalR hubs
│       ├── Exceptions/           # Custom exceptions
│       ├── Constants/            # Application constants
│       ├── Configuration/        # Dependency injection & configuration
│       ├── Program.cs            # Application entry point
│       └── appsettings*.json    # Configuration files
├── LICENSE
├── README.md
└── .gitignore
```

## Getting Started

### Prerequisites

- .NET 10 SDK or later
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone https://github.com/vladyslavzaiets/signalr-map-realtime.git
cd signalr-map-realtime
```

2. Update database connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=SignalRMapRealtimeDb;Trusted_Connection=true;"
}
```

3. Apply database migrations:
```bash
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` and Swagger documentation at `https://localhost:5001/swagger`

## API Endpoints

### Health Check
- `GET /health` - Application health status
- `GET /api/info` - API information

### Vehicles (Future Implementation)
- `GET /api/v1/vehicles` - List all vehicles
- `GET /api/v1/vehicles/{id}` - Get vehicle details
- `POST /api/v1/vehicles` - Create vehicle
- `PUT /api/v1/vehicles/{id}` - Update vehicle
- `DELETE /api/v1/vehicles/{id}` - Delete vehicle

### Locations (Future Implementation)
- `GET /api/v1/locations/vehicle/{vehicleId}` - Get vehicle locations
- `POST /api/v1/locations` - Record new location
- `GET /api/v1/locations/nearby` - Find locations nearby

### Routes (Future Implementation)
- `GET /api/v1/routes` - List routes
- `POST /api/v1/routes` - Create route
- `PUT /api/v1/routes/{id}` - Update route

## SignalR Hub Methods

### Client -> Server
- `SendLocationUpdate(locationDto)` - Send GPS location update
- `SubscribeToVehicle(vehicleId)` - Subscribe to vehicle updates
- `UnsubscribeFromVehicle(vehicleId)` - Unsubscribe from vehicle
- `RequestVehicleLocation(vehicleId)` - Request current location
- `RequestOnlineVehicleCount()` - Request fleet status

### Server -> Client
- `LocationUpdated(locationDto)` - Location update broadcast
- `VehicleStatusChanged(vehicleId, status)` - Status change notification
- `RouteProgressUpdated(routeId, percentage, status)` - Route progress update
- `Alert(vehicleId, alertType, message)` - Alert notification
- `FleetStatusUpdated(fleetName, onlineCount, totalCount)` - Fleet status
- `Pong()` - Heartbeat response

## Core Services

### LocationService
- Records and retrieves location points
- Calculates distances and statistics
- Manages location history
- Provides geofencing capabilities

### VehicleService
- Manages vehicle lifecycle
- Tracks vehicle status and connectivity
- Monitors fuel levels and speed violations
- Handles driver assignments

### TrackingService
- Manages tracking sessions
- Handles session state transitions
- Calculates session statistics
- Generates performance reports

## Database Models

- **Vehicle** - Tracked vehicles with status and metrics
- **Location** - GPS coordinates with metadata
- **User** - Drivers, couriers, and system users
- **Asset** - Equipment and packages being tracked
- **Route** - Planned routes with waypoints
- **Waypoint** - Individual stops on a route
- **TrackingSession** - Continuous tracking periods with statistics

## Domain-Driven Design

The project implements domain-driven design principles with:
- Rich domain models with business logic
- Repository pattern for data access
- Service layer for orchestration
- Custom exceptions for error handling
- Value objects and aggregates

## Configuration

### Connection Strings
Update in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "connection_string_here"
}
```

### CORS
Configure allowed origins:
```json
"Cors": {
  "AllowedOrigins": ["http://localhost:3000"]
}
```

### Logging
Set log levels in `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "SignalRMapRealtime": "Debug"
  }
}
```

## Development

### Applying Migrations
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Running Tests (Phase 2)
```bash
dotnet test
```

### Building for Production
```bash
dotnet publish -c Release
```

## Architecture Patterns

- **Repository Pattern** - Data access abstraction
- **Service Layer** - Business logic encapsulation
- **Dependency Injection** - Loose coupling
- **DTO Pattern** - API contract definition
- **SignalR Hub Pattern** - Real-time communication
- **Entity Framework Core** - ORM with migrations

## Error Handling

Custom exception hierarchy:
- `LocationTrackingException` - Base exception
- `VehicleNotFoundException` - Vehicle not found
- `InvalidLocationException` - Invalid coordinates
- `AssetNotFoundException` - Asset not found
- `TrackingSessionNotFoundException` - Session not found

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Author

**Vladyslav Zaiets**
- Website: https://sarmkadan.com
- Email: rutova2@gmail.com
- GitHub: https://github.com/vladyslavzaiets

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## Roadmap

### Phase 1 (Current)
- ✅ Core domain models
- ✅ Database schema and migrations
- ✅ Repository pattern implementation
- ✅ Business services
- ✅ SignalR hub setup
- ✅ Configuration and DI
- ✅ Project structure

### Phase 2 (Planned)
- API endpoints and controllers
- Unit tests and integration tests
- Authentication and authorization
- Validation and error handling
- Performance optimization
- Frontend integration

### Phase 3 (Planned)
- Advanced analytics and reporting
- Machine learning for route optimization
- Mobile app support
- Advanced geofencing
- Offline support
- Push notifications

## Support

For issues and questions, please open an issue on GitHub or contact the author.

---

**SignalR Map Realtime** - Real-time location tracking made simple and scalable.
