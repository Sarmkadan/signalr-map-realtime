// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Troubleshooting Guide

This guide provides solutions for common issues encountered when developing, deploying, or running SignalR Map Realtime.

## Setup & Installation Issues

### Issue: "The target framework 'net10.0' is not installed"

**Cause**: .NET 10 SDK not installed on your machine.

**Solution**:
```bash
# Check current .NET version
dotnet --version

# Download and install .NET 10
# Visit https://dotnet.microsoft.com/download/dotnet/10.0
# Or use a package manager:

# Windows (Chocolatey)
choco install dotnet-sdk-10.0

# macOS (Homebrew)
brew install dotnet-sdk

# Linux (Ubuntu/Debian)
sudo apt-get update
sudo apt-get install dotnet-sdk-10.0
```

After installation, verify:
```bash
dotnet --version  # Should show 10.0.x
dotnet --list-sdks
```

---

### Issue: "LocalDB Server does not exist" or "Cannot connect to SQL Server"

**Cause**: SQL Server LocalDB is not installed or not running.

**Solution - Windows**:

1. Install LocalDB via Visual Studio:
   - Open Visual Studio Installer
   - Click "Modify"
   - Under "Desktop development with C#", check "LocalDB"
   - Click "Modify" to install

2. Or install SQL Server Express:
   - Download from: https://www.microsoft.com/sql-server/sql-server-downloads
   - Run installer
   - Enable "Local DB" option

3. Verify LocalDB is running:
   ```bash
   # List LocalDB instances
   sqllocaldb info
   
   # Start LocalDB
   sqllocaldb start mssqllocaldb
   
   # Delete and recreate if corrupted
   sqllocaldb delete mssqllocaldb
   sqllocaldb create mssqllocaldb
   ```

**Solution - Linux/macOS**:

Use Docker instead:
```bash
# Run SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourPassword123" \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2019-latest

# Update connection string in appsettings.json
"DefaultConnection": "Server=localhost,1433;Database=SignalRMapRealtimeDb;User Id=sa;Password=YourPassword123;Encrypt=false;"
```

---

### Issue: "Port 5001 is already in use"

**Cause**: Another process is using the port.

**Solution - Windows**:
```bash
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or use different port
dotnet run --urls "https://localhost:5002"
```

**Solution - Linux/macOS**:
```bash
# Find process using port 5001
lsof -i :5001

# Kill process
kill -9 <PID>

# Or use different port
dotnet run --urls "https://localhost:5002"
```

---

### Issue: "Unable to resolve service for type 'IVehicleService'"

**Cause**: Dependency injection not configured correctly.

**Solution**: Check `Program.cs` has all service registrations:

```csharp
// In Program.cs, ensure all services are registered:
services.AddScoped<IVehicleService, VehicleService>();
services.AddScoped<ILocationService, LocationService>();
services.AddScoped<ITrackingService, TrackingService>();
services.AddScoped<IRepository<Vehicle>, VehicleRepository>();
// ... etc for all services
```

---

## Database Issues

### Issue: "Migration 'InitialCreate' has not been applied to the database"

**Cause**: Database migrations not applied.

**Solution**:
```bash
# Apply all pending migrations
dotnet ef database update

# Or specify target migration
dotnet ef database update InitialCreate

# Check migration status
dotnet ef migrations list
```

If database is corrupted:
```bash
# Drop and recreate database
dotnet ef database drop --force
dotnet ef database update
```

---

### Issue: "Timeout expired" or "Connection timeout" errors

**Cause**: Database connection issues, slow queries, or network problems.

**Solution**:

1. Increase timeout in connection string:
   ```json
   "DefaultConnection": "Server=...;Connection Timeout=60;"
   ```

2. Check database server is running:
   ```bash
   # SQL Server
   SELECT 1  -- Test connection
   
   # From sqlcmd
   sqlcmd -S localhost -U sa -P password
   ```

3. Check network connectivity:
   ```bash
   # Test connection
   ping sql-server-host
   
   # Test port
   telnet sql-server-host 1433
   ```

4. Review slow queries:
   - Use SQL Profiler
   - Check indexes are created: `dotnet ef database update`
   - Review query execution plans

---

### Issue: "Unique constraint violation" when creating vehicle

**Cause**: License plate already exists in database.

**Solution**:
```csharp
// Check existing vehicle before creating
var existing = await _vehicleService.GetByLicensePlateAsync(licensePlate);
if (existing != null)
{
    throw new InvalidOperationException("Vehicle with this license plate already exists");
}
```

Or if testing with same data:
```bash
# Clear test data
DELETE FROM Vehicles WHERE LicensePlate = 'TEST-001'

# Or create database from scratch
dotnet ef database drop --force
dotnet ef database update
```

---

## API & Web Issues

### Issue: Swagger UI not loading (404 error)

**Cause**: Swagger not enabled or CORS issue.

**Solution**:

1. Verify Swagger is enabled in `appsettings.json`:
   ```json
   "AppSettings": {
     "EnableSwagger": true
   }
   ```

2. Check middleware order in `Program.cs`:
   ```csharp
   // Swagger must be before UseRouting
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI();
   }
   
   app.UseRouting();
   app.UseEndpoints(...);
   ```

3. Clear browser cache: `Ctrl+Shift+Delete`

---

### Issue: CORS error when accessing from browser

**Cause**: Client origin not in allowed CORS origins.

**Solution**: Update `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173",
    "https://your-domain.com"
  ]
}
```

And verify middleware:
```csharp
// In Program.cs
app.UseCors("AllowedOrigins");
```

---

### Issue: "401 Unauthorized" on API calls

**Cause**: Missing or invalid API key/authentication.

**Solution**:

1. Check if authentication is enabled:
   ```json
   "Authentication": {
     "Enabled": true,
     "ApiKeyHeader": "X-API-Key"
   }
   ```

2. Include API key in request:
   ```bash
   curl -H "X-API-Key: your-api-key" https://localhost:5001/api/v1/vehicles
   ```

3. If using bearer tokens, include:
   ```bash
   curl -H "Authorization: Bearer your-token" https://localhost:5001/api/v1/vehicles
   ```

---

### Issue: "400 Bad Request" on POST/PUT requests

**Cause**: Invalid request body, validation errors, or missing required fields.

**Solution**:

1. Check request JSON is valid:
   ```bash
   # Use jq to validate
   echo '{"name": "test"}' | jq .
   ```

2. Check required fields are included:
   ```json
   {
     "licensePlate": "ABC-123",
     "manufacturer": "Toyota",
     "model": "Camry",
     "year": 2024
   }
   ```

3. View validation errors from response:
   ```bash
   curl -X POST https://localhost:5001/api/v1/vehicles \
     -H "Content-Type: application/json" \
     -d '{invalid json}'
   # Will show validation error details
   ```

---

## SignalR & Real-time Issues

### Issue: "Failed to connect to SignalR hub"

**Cause**: Hub not running, CORS not configured, or wrong URL.

**Solution**:

1. Verify hub is registered in `Program.cs`:
   ```csharp
   app.MapHub<LocationHub>("/locationHub");
   ```

2. Check CORS allows WebSocket:
   ```csharp
   services.AddCors(options =>
   {
       options.AddPolicy("Default", policy =>
       {
           policy.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials(); // Important for SignalR
       });
   });
   ```

3. Verify hub URL is correct:
   ```javascript
   // Correct URL format
   const connection = new signalR.HubConnectionBuilder()
       .withUrl("https://localhost:5001/locationHub")
       .build();
   ```

4. Check firewall/proxy allows WebSocket connections

---

### Issue: SignalR connection drops frequently

**Cause**: Network issues, timeout, or server problems.

**Solution**:

1. Enable automatic reconnect:
   ```javascript
   const connection = new signalR.HubConnectionBuilder()
       .withUrl("https://localhost:5001/locationHub")
       .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000])
       .build();
   ```

2. Increase server timeout in `appsettings.json`:
   ```json
   "SignalR": {
     "KeepAliveInterval": 15,
     "ClientTimeoutInterval": 30,
     "HandshakeTimeout": 15
   }
   ```

3. Check server logs for errors:
   ```bash
   # View real-time logs
   dotnet run | grep -i signalr
   ```

---

### Issue: Real-time updates not received

**Cause**: Not subscribed to group, wrong event name, or client disconnected.

**Solution**:

1. Verify subscription:
   ```javascript
   // Must subscribe to vehicle first
   await connection.invoke("SubscribeToVehicle", vehicleId);
   ```

2. Listen for correct event:
   ```javascript
   // Check event name matches hub method
   connection.on("LocationUpdated", (location) => {
       console.log("Received:", location);
   });
   ```

3. Verify hub is broadcasting:
   ```csharp
   // In LocationHub.cs
   await Clients.Group($"vehicle-{vehicleId}")
       .SendAsync("LocationUpdated", location);
   ```

---

## Performance Issues

### Issue: Slow location queries

**Cause**: Missing indexes, large result sets, or inefficient queries.

**Solution**:

1. Ensure indexes exist:
   ```bash
   dotnet ef database update
   ```

2. Use pagination:
   ```bash
   curl "https://localhost:5001/api/v1/locations/vehicle/id?skip=0&take=100"
   ```

3. Filter by date range:
   ```bash
   curl "https://localhost:5001/api/v1/locations/vehicle/id?startDate=2024-05-01&endDate=2024-05-04"
   ```

4. Check database indexes:
   ```sql
   -- View index fragmentation
   SELECT * FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED')
   WHERE avg_fragmentation_in_percent > 10
   ORDER BY avg_fragmentation_in_percent DESC;
   
   -- Rebuild fragmented indexes
   ALTER INDEX ALL ON Locations REBUILD;
   ```

---

### Issue: High memory usage

**Cause**: Large objects in memory, memory leaks, or excessive caching.

**Solution**:

1. Disable unnecessary caching:
   ```json
   "Caching": {
     "Enabled": false
   }
   ```

2. Reduce cache duration:
   ```json
   "Caching": {
     "DurationSeconds": 60
   }
   ```

3. Use streaming for large result sets:
   ```csharp
   // Instead of loading all locations:
   var locations = await _context.Locations.ToListAsync();
   
   // Use pagination:
   var locations = await _context.Locations
       .Where(l => l.VehicleId == vehicleId)
       .OrderByDescending(l => l.Timestamp)
       .Skip(skip)
       .Take(take)
       .ToListAsync();
   ```

4. Monitor memory:
   ```bash
   # View process memory (Windows)
   Get-Process dotnet | Select-Object WorkingSet
   
   # Linux
   ps aux | grep dotnet
   ```

---

### Issue: High CPU usage

**Cause**: Inefficient queries, tight loops, or excessive logging.

**Solution**:

1. Reduce logging level in production:
   ```json
   "Logging": {
     "LogLevel": {
       "Default": "Warning"
     }
   }
   ```

2. Disable debug features:
   ```json
   "AppSettings": {
     "Environment": "Production",
     "EnableSwagger": false
   }
   ```

3. Optimize queries (use projection):
   ```csharp
   // Bad: Load entire entity
   var vehicles = await _context.Vehicles.ToListAsync();
   
   // Good: Load only needed fields
   var vehicles = await _context.Vehicles
       .Select(v => new { v.Id, v.LicensePlate, v.Status })
       .ToListAsync();
   ```

4. Use query plan analyzer:
   ```sql
   -- Enable actual execution plan
   SET STATISTICS IO ON;
   SET STATISTICS TIME ON;
   
   SELECT * FROM Vehicles WHERE LicensePlate = 'ABC-123';
   
   -- Review output for expensive operations
   ```

---

## Logging & Debugging

### Enable Detailed Logging

Edit `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Debug",
      "SignalRMapRealtime": "Debug"
    },
    "Console": {
      "IncludeScopes": true,
      "Format": "json"
    }
  }
}
```

### View Entity Framework SQL Queries

```csharp
// In Program.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .LogTo(Console.WriteLine) // Log SQL to console
           .EnableSensitiveDataLogging() // Include parameter values
);
```

### Debug SignalR Connections

```javascript
// In client code
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/locationHub")
    .configureLogging(signalR.LogLevel.Debug) // Enable debug logging
    .build();

connection.onreconnected(() => console.log("Reconnected"));
connection.onreconnecting(err => console.log("Reconnecting:", err));
connection.onclose(err => console.log("Connection closed:", err));
```

---

## Common Error Messages & Solutions

| Error | Cause | Solution |
|-------|-------|----------|
| "An error occurred while using the transaction" | Transaction exception | Check inner exception; rollback transaction |
| "Entity type 'X' has no key defined" | Missing [Key] attribute | Add `[Key]` or `Id` property to entity |
| "DbUpdateException: ...duplicate key" | Unique constraint violation | Check unique constraints; use different values |
| "System.NullReferenceException" | Null object accessed | Add null checks; verify object is initialized |
| "Index was out of range" | Array/List bounds | Check collection size before accessing |
| "Method not found" | Version mismatch | Ensure package versions match .NET version |

---

## Getting Help

If you can't find a solution:

1. **Check logs**: Look for detailed error messages
2. **Search issues**: GitHub Issues for similar problems
3. **FAQ**: See [FAQ.md](faq.md) for common questions
4. **Documentation**: Check [Getting Started](getting-started.md)
5. **Stack Overflow**: Tag with `signalr` and `.net`
6. **GitHub Issues**: Open new issue with:
   - Steps to reproduce
   - Expected vs actual behavior
   - Error messages/logs
   - Environment details (.NET version, OS, etc.)

---

## Performance Checklist

Before deploying to production, verify:

- [ ] Database indexes created
- [ ] Query performance acceptable (< 500ms)
- [ ] Memory usage stable
- [ ] CPU usage normal
- [ ] SignalR connections stable
- [ ] CORS configured correctly
- [ ] HTTPS enabled
- [ ] Logging configured appropriately
- [ ] Error handling working
- [ ] Backups scheduled
- [ ] Monitoring alerts set up
- [ ] Load testing completed
- [ ] Security scan passed
- [ ] Documentation current
