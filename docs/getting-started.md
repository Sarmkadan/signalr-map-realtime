// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with SignalR Map Realtime

This guide will help you set up and run SignalR Map Realtime on your development machine in under 15 minutes.

## Prerequisites

Before starting, ensure you have:

- **.NET SDK 10.0** or later - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **SQL Server** 2019+ Express or LocalDB (included with Visual Studio)
- **Git** - [Download](https://git-scm.com)
- **Text Editor**: Visual Studio 2022, VS Code, or JetBrains Rider

## Step-by-Step Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/Sarmkadan/signalr-map-realtime.git
cd signalr-map-realtime
```

### Step 2: Verify .NET Installation

```bash
dotnet --version
# Expected output: 10.0.x
```

### Step 3: Restore Dependencies

```bash
dotnet restore
```

This downloads all required NuGet packages.

### Step 4: Configure Database Connection

#### Option A: Using LocalDB (Default - Windows)

The default `appsettings.json` already uses LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SignalRMapRealtimeDb;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

If LocalDB isn't installed, install via Visual Studio Installer:
- Select "Modify"
- Under "Desktop development with C#", check "LocalDB"
- Install

#### Option B: Using SQL Server Express

Edit `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SignalRMapRealtimeDb;Trusted_Connection=true;"
}
```

#### Option C: Using Docker

If you have Docker installed:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest

# Update connection string
"DefaultConnection": "Server=localhost,1433;Database=SignalRMapRealtimeDb;User Id=sa;Password=YourPassword123;"
```

### Step 5: Create Database & Apply Migrations

```bash
cd src/SignalRMapRealtime
dotnet ef database update
```

This creates the database and all required tables.

**If using Visual Studio Package Manager Console:**

```powershell
# In Package Manager Console (Tools > NuGet Package Manager > Package Manager Console)
Update-Database
```

### Step 6: Build the Solution

```bash
cd ../..
dotnet build
```

Verify no compilation errors.

### Step 7: Run the Application

```bash
dotnet run --project src/SignalRMapRealtime
```

Expected output:
```
info: Microsoft.AspNetCore.Hosting.Diagnostics
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### Step 8: Verify It's Working

Open your browser and navigate to:

```
https://localhost:5001/swagger
```

You should see the Swagger UI with all API endpoints.

## Your First API Call

### 1. Create a Vehicle

Using curl:

```bash
curl -X POST https://localhost:5001/api/v1/vehicles \
  -H "Content-Type: application/json" \
  -d '{
    "licensePlate": "TEST-001",
    "manufacturer": "Toyota",
    "model": "Camry",
    "year": 2024
  }' \
  -k  # Ignore SSL for localhost
```

Using PowerShell:

```powershell
$body = @{
    licensePlate = "TEST-001"
    manufacturer = "Toyota"
    model = "Camry"
    year = 2024
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/v1/vehicles" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body `
  -SkipCertificateCheck
```

Using Swagger UI (easier):

1. Go to https://localhost:5001/swagger
2. Find "POST /api/v1/vehicles"
3. Click "Try it out"
4. Enter the JSON above
5. Click "Execute"

Save the returned `id` for the next step.

### 2. Record a Location

Replace `{vehicleId}` with the ID from step 1:

```bash
curl -X POST https://localhost:5001/api/v1/locations \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": "YOUR-VEHICLE-ID-HERE",
    "latitude": 40.7128,
    "longitude": -74.0060,
    "accuracy": 5.0,
    "speed": 25.5,
    "heading": 90,
    "timestamp": "'$(date -u +%Y-%m-%dT%H:%M:%SZ)'"
  }' \
  -k
```

### 3. Retrieve Vehicle Locations

```bash
curl https://localhost:5001/api/v1/locations/vehicle/YOUR-VEHICLE-ID-HERE \
  -k
```

You should see the location you just created.

## Next Steps

### Connect via SignalR

Create a `.html` file to test real-time updates:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Location Tracker</title>
    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/signalr.js"></script>
</head>
<body>
    <h1>Real-time Location Tracking</h1>
    <div id="messages"></div>
    
    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:5001/locationHub")
            .withAutomaticReconnect()
            .build();

        connection.on("LocationUpdated", (location) => {
            const msg = `Vehicle ${location.vehicleId}: ${location.latitude}, ${location.longitude}`;
            document.getElementById("messages").innerHTML += msg + "<br>";
        });

        connection.start().catch(err => console.error(err));
    </script>
</body>
</html>
```

Open this file in your browser and send location updates via curl to see real-time updates.

### Explore the Code

Key files to understand:

1. **Program.cs** - Application setup and configuration
2. **Domain/Models/** - Data models (Vehicle, Location, etc.)
3. **Services/** - Business logic
4. **Controllers/** - REST API endpoints
5. **Hubs/LocationHub.cs** - Real-time communication

### Read Documentation

- [Architecture Guide](architecture.md) - System design and patterns
- [API Reference](api-reference.md) - Complete API documentation
- [Database Schema](database-schema.md) - Table structures and relationships
- [Deployment Guide](deployment.md) - Production setup

## Common Tasks

### Change the Port

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Port": 5002
  }
}
```

Or run with environment variable:

```bash
dotnet run --urls "https://localhost:5002"
```

### Enable File Logging

Edit `appsettings.Development.json`:

```json
{
  "Logging": {
    "File": {
      "IncludeScopes": true,
      "LogLevel": {
        "Default": "Debug"
      }
    }
  }
}
```

### Use PostgreSQL Instead of SQL Server

1. Install NuGet package:
   ```bash
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. Update connection string:
   ```json
   "DefaultConnection": "Host=localhost;Database=signalr_map;Username=postgres;Password=password"
   ```

3. Update Startup.cs to use PostgreSQL provider

### Add Authentication

The project includes infrastructure for API key authentication. Enable in `appsettings.json`:

```json
"Authentication": {
  "Enabled": true,
  "ApiKeyHeader": "X-API-Key"
}
```

## Troubleshooting Quick Guide

| Problem | Solution |
|---------|----------|
| "SQL Server doesn't exist" | Verify `appsettings.json` connection string |
| Port 5001 already in use | Use different port: `dotnet run --urls "https://localhost:5002"` |
| Migrations fail | Delete database and run: `dotnet ef database update` |
| Swagger not loading | Check CORS settings in `appsettings.json` |
| SignalR connection fails | Verify hub URL matches your server: `https://localhost:5001/locationHub` |

## Getting Help

- Check [FAQ](faq.md) for common questions
- See [Troubleshooting](troubleshooting.md) for detailed solutions
- Open an issue on [GitHub](https://github.com/Sarmkadan/signalr-map-realtime/issues)

## Next: Learn the Architecture

Once everything is running, read [Architecture Guide](architecture.md) to understand how the system works.
