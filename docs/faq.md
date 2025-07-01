// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Frequently Asked Questions

## General

**Q: What is SignalR Map Realtime?**

A: It's an open-source, production-ready real-time location tracking system built with ASP.NET Core 10 and SignalR. It enables tracking of vehicles, couriers, assets, and field workers with live GPS updates, route management, and comprehensive fleet monitoring.

**Q: What license is this project under?**

A: MIT License - free for commercial and personal use. See LICENSE file for details.

**Q: Who maintains this project?**

A: Vladyslav Zaiets - CTO & Software Architect. See [Portfolio](https://sarmkadan.com) for more information.

**Q: Can I use this in production?**

A: Yes, absolutely. The project follows enterprise-grade patterns and includes comprehensive documentation for deployment to AWS, Azure, Google Cloud, Kubernetes, Docker, and traditional servers.

**Q: What's included in the project?**

A: Complete backend API with SignalR hubs, database models, services, repositories, example clients, comprehensive documentation, Docker support, Kubernetes manifests, and CI/CD workflows.

## Installation & Setup

**Q: Do I need SQL Server? Can I use PostgreSQL?**

A: The project ships configured for SQL Server, but Entity Framework Core supports PostgreSQL, MySQL, SQLite, and others. See [Architecture Guide](architecture.md#database-options) for switching databases.

**Q: I get "LocalDB doesn't exist" error. How do I fix it?**

A: Install SQL Server LocalDB via Visual Studio Installer:
- Run installer, click "Modify"
- Under "Desktop development with C#", check "LocalDB"
- Install

Or use Docker SQL Server instead:
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password123!" -p 1433:1433 mssql/server:2019-latest
```

**Q: What .NET version do I need?**

A: .NET 10.0 or later. [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)

**Q: Can I run this on Linux/Mac?**

A: Yes! .NET 10 runs on Windows, Linux, and macOS. Use Docker for SQL Server on Linux/Mac.

**Q: How long does setup take?**

A: 5-10 minutes following the [Getting Started Guide](getting-started.md).

## Development

**Q: How do I add new fields to the Vehicle model?**

A: 
1. Edit `src/SignalRMapRealtime/Domain/Models/Vehicle.cs`
2. Add property: `public string VinNumber { get; set; }`
3. Create migration: `dotnet ef migrations add AddVinNumber`
4. Update database: `dotnet ef database update`
5. Update `VehicleDto` in DTOs/
6. Update AutoMapper profile in Configuration/

**Q: How do I create a new API endpoint?**

A: 
1. Create controller: `Controllers/MyController.cs`
2. Inject service via constructor
3. Create action methods with [HttpGet], [HttpPost] attributes
4. DTOs handle request/response serialization
5. Service layer contains business logic

Example:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class CustomController : ControllerBase
{
    private readonly IMyService _service;
    
    public CustomController(IMyService service)
    {
        _service = service;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<MyDto>> Get(Guid id)
    {
        var result = await _service.GetAsync(id);
        return Ok(result);
    }
}
```

**Q: How do I test my changes?**

A: See [Testing](#testing) section below.

**Q: Can I change the port?**

A: Yes:
```bash
dotnet run --urls "https://localhost:5002"
```

Or in `appsettings.json`:
```json
"AppSettings": {
  "Port": 5002
}
```

**Q: How do I enable detailed logging?**

A: Edit `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "SignalRMapRealtime": "Debug"
    }
  }
}
```

## Real-time Communication

**Q: How do I connect to the SignalR hub from my client?**

A: 
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/locationHub")
    .withAutomaticReconnect()
    .build();

await connection.start();
```

**Q: Why is my SignalR connection failing?**

A: Common causes:
1. **CORS not configured**: Update `appsettings.json` with client URL
2. **Wrong hub URL**: Should be `/locationHub`
3. **HTTPS certificate invalid**: Use `-k` flag with curl or ignore in browser
4. **Firewall blocking WebSocket**: Open port 5001

**Q: Can I use SignalR over HTTP instead of HTTPS?**

A: Yes, for development:
```bash
dotnet run --urls "http://localhost:5000"
```

For production, HTTPS is required.

**Q: How many concurrent connections can the hub handle?**

A: A single server with default settings handles ~10,000 concurrent connections. For more, use SignalR backplane with Redis and load balancer.

**Q: Can I customize the hub methods?**

A: Yes, edit `Hubs/LocationHub.cs`. Remember to update client code to call new methods.

## Database & Performance

**Q: How do I optimize location queries?**

A: 
1. Use pagination: `?skip=0&take=20`
2. Filter by date range
3. Cache frequently accessed data
4. Create indexes on VehicleId, Timestamp
5. Archive old locations to separate table

**Q: How do I back up my database?**

A: 
```bash
# Full backup
BACKUP DATABASE SignalRMapRealtimeDb 
TO DISK = 'C:\Backups\db.bak';
```

Or use automated backups in cloud provider (AWS RDS, Azure SQL, etc.)

**Q: Can I migrate my existing data?**

A: Yes, write a migration script:
```csharp
var vehicles = GetVehiclesFromLegacyDb();
foreach (var v in vehicles)
{
    var newVehicle = new Vehicle { ... };
    _context.Vehicles.Add(newVehicle);
}
await _context.SaveChangesAsync();
```

**Q: How do I handle large numbers of location updates?**

A: 
1. Batch inserts: Send 100+ locations per request
2. Use bulk operations
3. Archive old locations
4. Configure proper indexes
5. Use connection pooling

**Q: Is there a way to delete old data?**

A: 
```csharp
var oldLocations = await _context.Locations
    .Where(l => l.Timestamp < DateTime.UtcNow.AddMonths(-3))
    .ToListAsync();

_context.Locations.RemoveRange(oldLocations);
await _context.SaveChangesAsync();
```

Or configure retention policy in database.

## Deployment & Scaling

**Q: How do I deploy to production?**

A: See [Deployment Guide](deployment.md) for:
- Docker deployment
- Azure App Service
- AWS Elastic Beanstalk
- Google Cloud Run
- Kubernetes

**Q: How do I scale the application?**

A:
1. **Horizontal**: Deploy multiple instances behind load balancer
2. **Vertical**: Increase server resources
3. **Database**: Use replication and read replicas
4. **Cache**: Use Redis for distributed caching
5. **SignalR Backplane**: Use Redis/Service Bus for multi-server

**Q: Can I run this on a shared hosting?**

A: Not recommended. Shared hosting typically doesn't support:
- Long-running WebSocket connections
- Background jobs
- Database migrations

Use cloud providers (AWS, Azure, Google Cloud) for better support.

**Q: How do I monitor the application?**

A: Use:
- Application Insights (Azure)
- New Relic
- DataDog
- ELK Stack
- Prometheus + Grafana

**Q: What are the system requirements for production?**

A: Minimum:
- 2 GB RAM
- 2 CPU cores
- 10 GB disk space
- SQL Server 2019+
- .NET 10 runtime

Recommended:
- 8 GB+ RAM
- 4+ CPU cores
- 50 GB+ SSD
- SQL Server 2019+ (Standard or Enterprise)
- Redis for caching
- Load balancer
- CDN for static content

## Security

**Q: How do I secure my API?**

A: 
1. Enable HTTPS/TLS
2. Use API key authentication
3. Implement rate limiting
4. Validate all inputs
5. Use CORS appropriately
6. Keep dependencies updated
7. Enable audit logging
8. Run security scans (OWASP ZAP, Snyk)

**Q: How do I add authentication?**

A: The project includes API key infrastructure. Enable in `appsettings.json`:
```json
"Authentication": {
  "Enabled": true,
  "ApiKeyHeader": "X-API-Key"
}
```

For OAuth/OpenID Connect, integrate:
- Azure AD
- Auth0
- Okta

**Q: Can I use this without HTTPS?**

A: Only for local development. Production MUST use HTTPS.

**Q: How do I handle sensitive data (passwords, tokens)?**

A: 
1. Never commit secrets to git
2. Use environment variables or secrets management
3. Use `dotnet user-secrets` for development
4. Azure Key Vault or AWS Secrets Manager for production
5. Encrypt sensitive data at rest

**Q: Is the API vulnerable to SQL injection?**

A: No, Entity Framework Core parameterizes all queries automatically.

## Testing

**Q: How do I write unit tests?**

A: Create test project with xUnit:
```csharp
public class LocationServiceTests
{
    [Fact]
    public async Task GetLocation_WithValidId_ReturnsLocation()
    {
        // Arrange
        var mockRepo = new Mock<ILocationRepository>();
        var service = new LocationService(mockRepo.Object);
        
        // Act
        var result = await service.GetAsync(Guid.NewGuid());
        
        // Assert
        Assert.NotNull(result);
    }
}
```

**Q: How do I test SignalR hubs?**

A: Use TestServer for integration testing:
```csharp
var server = new TestServer(new WebHostBuilder()
    .UseStartup<Startup>());
var client = new HubConnection(new Uri("http://localhost/locationHub"));
await client.StartAsync();
```

**Q: How do I test database operations?**

A: Use real database for integration tests:
```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: "Test")
    .Options;
    
using var context = new ApplicationDbContext(options);
```

## Contributing

**Q: How do I contribute?**

A: 
1. Fork the repository
2. Create feature branch
3. Make changes
4. Write tests
5. Submit pull request

See [Contributing Guidelines](../README.md#contributing) for details.

**Q: What's the code style?**

A: Follow Microsoft C# Coding Conventions:
- camelCase for local variables
- PascalCase for public members
- Use meaningful names
- Keep methods small and focused

**Q: How do I report a bug?**

A: Open an issue on GitHub with:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment (OS, .NET version, etc.)
- Logs/screenshots

**Q: Can I request a feature?**

A: Yes! Open an issue with:
- Use case description
- Proposed implementation
- Examples of similar features
- Why it's important

## Troubleshooting

**Q: Application crashes on startup.**

A: Check:
1. .NET SDK installed: `dotnet --version`
2. Database connection: Test connection string
3. Ports in use: `netstat -ano | findstr :5001`
4. Logs: Check console output for errors

**Q: Swagger UI not loading.**

A: Ensure `EnableSwagger` is true in `appsettings.json`:
```json
"AppSettings": {
  "EnableSwagger": true
}
```

**Q: Migrations failing.**

A: 
```bash
# Remove last migration
dotnet ef migrations remove

# Recreate
dotnet ef migrations add [name]
dotnet ef database update
```

**Q: Performance issues.**

A: Check:
1. Database query performance (use SQL Profiler)
2. Missing indexes
3. N+1 query problems
4. Memory usage
5. CPU usage

See [Troubleshooting Guide](troubleshooting.md) for detailed solutions.

## Getting Help

- **Documentation**: See [README.md](../README.md)
- **Issues**: [GitHub Issues](https://github.com/Sarmkadan/signalr-map-realtime/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Sarmkadan/signalr-map-realtime/discussions)
- **Email**: Contact via website [sarmkadan.com](https://sarmkadan.com)

## Additional Resources

- [Architecture Guide](architecture.md)
- [Getting Started](getting-started.md)
- [Deployment Guide](deployment.md)
- [API Reference](api-reference.md)
- [Database Schema](database-schema.md)
- [Troubleshooting](troubleshooting.md)
