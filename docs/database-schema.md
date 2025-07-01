// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Database Schema Documentation

This document describes the database schema, tables, relationships, and indexing strategy.

## Entity Relationship Diagram (ERD)

```
┌──────────────────┐         ┌──────────────────┐
│      Users       │◄────────┤    Vehicles      │
├──────────────────┤    1:N  ├──────────────────┤
│ Id (PK)          │         │ Id (PK)          │
│ Email (Unique)   │         │ LicensePlate     │
│ Name             │         │ Manufacturer     │
│ Phone            │         │ Model            │
│ Role             │         │ Status           │
│ CreatedAt        │         │ AssignedUserId FK│
│ UpdatedAt        │         │ CreatedAt        │
└──────────────────┘         └────────┬─────────┘
                                      │
                                    1:N
                                      │
                            ┌─────────▼─────────┐
                            │    Locations      │
                            ├───────────────────┤
                            │ Id (PK)           │
                            │ VehicleId (FK)    │
                            │ Latitude          │
                            │ Longitude         │
                            │ Accuracy          │
                            │ Speed             │
                            │ Heading           │
                            │ Type              │
                            │ Timestamp         │
                            │ CreatedAt         │
                            └───────────────────┘

┌──────────────────┐         ┌──────────────────┐
│     Routes       │◄────────┤    Waypoints     │
├──────────────────┤    1:N  ├──────────────────┤
│ Id (PK)          │         │ Id (PK)          │
│ Name             │         │ RouteId (FK)     │
│ VehicleId (FK)   │         │ Latitude         │
│ Status           │         │ Longitude        │
│ TotalDistance    │         │ Order            │
│ CreatedAt        │         │ Name             │
│ UpdatedAt        │         │ CreatedAt        │
└──────────────────┘         └──────────────────┘

┌──────────────────┐         ┌──────────────────┐
│     Assets       │◄────────┤ TrackingSessions │
├──────────────────┤    1:N  ├──────────────────┤
│ Id (PK)          │         │ Id (PK)          │
│ Name             │         │ VehicleId (FK)   │
│ Type             │         │ Status           │
│ Status           │         │ StartTime        │
│ Location         │         │ EndTime          │
│ LastUpdate       │         │ TotalDistance    │
│ CreatedAt        │         │ CreatedAt        │
└──────────────────┘         └──────────────────┘
```

## Tables

### Users Table

Stores driver, courier, and system user information.

```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(255) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Role NVARCHAR(50) NOT NULL DEFAULT 'Driver',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
```

**Columns:**
- `Id`: Primary key (UUID)
- `Email`: Unique email address
- `FirstName`, `LastName`: User name
- `Phone`: Contact phone number
- `Role`: User role (Driver, Courier, Manager, Admin)
- `IsActive`: Account active status
- `CreatedAt`, `UpdatedAt`: Timestamps

**Indexes:**
- `IX_Users_Email`: For authentication lookups
- `IX_Users_Role`: For role-based filtering
- `IX_Users_IsActive`: For active user queries

---

### Vehicles Table

Tracks vehicle information and status.

```sql
CREATE TABLE Vehicles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LicensePlate NVARCHAR(20) NOT NULL UNIQUE,
    Manufacturer NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    Year INT NOT NULL,
    VIN NVARCHAR(50),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    AssignedUserId UNIQUEIDENTIFIER,
    FuelLevel DECIMAL(5,2),
    OdometerReading BIGINT,
    LastLocationUpdate DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Vehicles_Users 
        FOREIGN KEY (AssignedUserId) 
        REFERENCES Users(Id)
);

CREATE UNIQUE INDEX IX_Vehicles_LicensePlate ON Vehicles(LicensePlate);
CREATE INDEX IX_Vehicles_Status ON Vehicles(Status);
CREATE INDEX IX_Vehicles_AssignedUserId ON Vehicles(AssignedUserId);
CREATE INDEX IX_Vehicles_IsDeleted ON Vehicles(IsDeleted);
CREATE INDEX IX_Vehicles_UpdatedAt ON Vehicles(UpdatedAt);
```

**Columns:**
- `Id`: Primary key
- `LicensePlate`: Vehicle registration (unique)
- `Manufacturer`, `Model`, `Year`: Vehicle details
- `VIN`: Vehicle Identification Number
- `Status`: Current status (Active, Inactive, InMaintenance)
- `AssignedUserId`: FK to driver
- `FuelLevel`: Current fuel percentage
- `OdometerReading`: Total distance traveled
- `LastLocationUpdate`: Last known update time
- `IsDeleted`: Soft delete flag
- `CreatedAt`, `UpdatedAt`: Timestamps

**Indexes:**
- Unique on LicensePlate for lookups
- Status for filtering by status
- AssignedUserId for driver queries
- IsDeleted for active vehicle queries
- UpdatedAt for recent activity queries

---

### Locations Table

Stores GPS location history for vehicles.

```sql
CREATE TABLE Locations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VehicleId UNIQUEIDENTIFIER NOT NULL,
    Latitude DECIMAL(10, 8) NOT NULL,
    Longitude DECIMAL(11, 8) NOT NULL,
    Accuracy DECIMAL(7, 2),
    Altitude DECIMAL(7, 2),
    Speed DECIMAL(7, 2),
    Heading INT,
    Type NVARCHAR(50) NOT NULL DEFAULT 'GPS',
    Timestamp DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Locations_Vehicles 
        FOREIGN KEY (VehicleId) 
        REFERENCES Vehicles(Id) ON DELETE CASCADE
);

CREATE CLUSTERED INDEX IX_Locations_VehicleId_Timestamp 
    ON Locations(VehicleId, Timestamp DESC);
CREATE INDEX IX_Locations_Timestamp ON Locations(Timestamp DESC);
CREATE INDEX IX_Locations_Coordinates 
    ON Locations(Latitude, Longitude);
```

**Columns:**
- `Id`: Primary key
- `VehicleId`: FK to vehicle (cascade delete)
- `Latitude`, `Longitude`: GPS coordinates
- `Accuracy`: Location accuracy in meters
- `Altitude`: Elevation above sea level
- `Speed`: Current speed (km/h)
- `Heading`: Direction (0-360 degrees)
- `Type`: Source type (GPS, CellTri, WiFi, Manual)
- `Timestamp`: GPS timestamp
- `CreatedAt`: Database insert time

**Indexes:**
- Clustered on (VehicleId, Timestamp) for efficient time-range queries
- Timestamp for querying by date
- Coordinates for geospatial queries

**Note:** This is the highest-volume table. Proper indexing is critical for performance.

---

### Routes Table

Planned routes with multiple waypoints.

```sql
CREATE TABLE Routes (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    VehicleId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    TotalDistance DECIMAL(10, 2),
    EstimatedDuration INT,
    ActualDuration INT,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Routes_Vehicles 
        FOREIGN KEY (VehicleId) 
        REFERENCES Vehicles(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Routes_VehicleId ON Routes(VehicleId);
CREATE INDEX IX_Routes_Status ON Routes(Status);
CREATE INDEX IX_Routes_CreatedAt ON Routes(CreatedAt DESC);
```

**Columns:**
- `Id`: Primary key
- `Name`: Route name/description
- `VehicleId`: FK to vehicle
- `Status`: Route status (Pending, Active, Completed, Cancelled)
- `TotalDistance`: Calculated route distance
- `EstimatedDuration`: Predicted time (minutes)
- `ActualDuration`: Completed time (minutes)
- `Notes`: Additional route information
- `CreatedAt`, `UpdatedAt`: Timestamps

---

### Waypoints Table

Individual stops on a route.

```sql
CREATE TABLE Waypoints (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RouteId UNIQUEIDENTIFIER NOT NULL,
    Latitude DECIMAL(10, 8) NOT NULL,
    Longitude DECIMAL(11, 8) NOT NULL,
    [Order] INT NOT NULL,
    Name NVARCHAR(255),
    EstimatedArrivalTime DATETIME2,
    ActualArrivalTime DATETIME2,
    Duration INT,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Waypoints_Routes 
        FOREIGN KEY (RouteId) 
        REFERENCES Routes(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Waypoints_Order CHECK ([Order] > 0)
);

CREATE INDEX IX_Waypoints_RouteId ON Waypoints(RouteId);
CREATE INDEX IX_Waypoints_Order ON Waypoints(RouteId, [Order]);
```

**Columns:**
- `Id`: Primary key
- `RouteId`: FK to route
- `Latitude`, `Longitude`: Waypoint location
- `Order`: Sequence number (1, 2, 3...)
- `Name`: Stop name
- `EstimatedArrivalTime`: Planned arrival
- `ActualArrivalTime`: Actual arrival
- `Duration`: Stop duration (minutes)
- `CreatedAt`: Timestamp

---

### Assets Table

Equipment, packages, or items being tracked.

```sql
CREATE TABLE Assets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    Description NVARCHAR(MAX),
    LastKnownLocation NVARCHAR(MAX),
    LastLocationUpdate DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Assets_Type ON Assets(Type);
CREATE INDEX IX_Assets_Status ON Assets(Status);
CREATE INDEX IX_Assets_UpdatedAt ON Assets(UpdatedAt DESC);
```

**Columns:**
- `Id`: Primary key
- `Name`: Asset identifier
- `Type`: Asset type (Vehicle, Package, Equipment, Container)
- `Status`: Current status
- `Description`: Asset details
- `LastKnownLocation`: Last location info
- `LastLocationUpdate`: Last update timestamp
- `CreatedAt`, `UpdatedAt`: Timestamps

---

### TrackingSessions Table

Continuous tracking periods with statistics.

```sql
CREATE TABLE TrackingSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VehicleId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    StartTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EndTime DATETIME2,
    TotalDistance DECIMAL(10, 2),
    AverageSpeed DECIMAL(7, 2),
    MaxSpeed DECIMAL(7, 2),
    LocationCount INT DEFAULT 0,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Sessions_Vehicles 
        FOREIGN KEY (VehicleId) 
        REFERENCES Vehicles(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Sessions_VehicleId ON TrackingSessions(VehicleId);
CREATE INDEX IX_Sessions_Status ON TrackingSessions(Status);
CREATE INDEX IX_Sessions_StartTime ON TrackingSessions(StartTime DESC);
```

**Columns:**
- `Id`: Primary key
- `VehicleId`: FK to vehicle
- `Status`: Session status (Active, Paused, Completed, Cancelled)
- `StartTime`: Session start
- `EndTime`: Session end
- `TotalDistance`: Calculated total distance
- `AverageSpeed`: Mean speed
- `MaxSpeed`: Peak speed
- `LocationCount`: Number of locations recorded
- `Notes`: Session notes
- `CreatedAt`, `UpdatedAt`: Timestamps

---

## Key Relationships

### One-to-Many (1:N)

1. **Users → Vehicles**: One user can have many assigned vehicles
2. **Vehicles → Locations**: One vehicle has many location records
3. **Vehicles → Routes**: One vehicle can have many routes
4. **Routes → Waypoints**: One route has many waypoints
5. **Vehicles → TrackingSessions**: One vehicle can have many sessions

### Cascade Delete

- Deleting a vehicle cascades to: Locations, Routes, TrackingSessions
- Deleting a route cascades to: Waypoints
- Orphaned locations are removed when vehicle is deleted

## Indexes Strategy

### Primary Indexes (Required)

```sql
-- Fast vehicle lookups by license plate
CREATE UNIQUE INDEX IX_Vehicles_LicensePlate ON Vehicles(LicensePlate);

-- Fast location queries by vehicle and time
CREATE CLUSTERED INDEX IX_Locations_VehicleId_Timestamp 
    ON Locations(VehicleId, Timestamp DESC);

-- Fast user lookups by email
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
```

### Secondary Indexes (Performance)

```sql
-- Status filtering
CREATE INDEX IX_Vehicles_Status ON Vehicles(Status);
CREATE INDEX IX_Routes_Status ON Routes(Status);

-- Date range queries
CREATE INDEX IX_Routes_CreatedAt ON Routes(CreatedAt DESC);
CREATE INDEX IX_Sessions_StartTime ON TrackingSessions(StartTime DESC);

-- Geospatial queries
CREATE INDEX IX_Locations_Coordinates ON Locations(Latitude, Longitude);
```

### Monitoring Indexes

```sql
-- View index usage statistics
SELECT OBJECT_NAME(OBJECT_ID) as [Table],
    name as [Index],
    user_updates,
    user_seeks,
    user_lookups,
    user_scans
FROM sys.dm_db_index_usage_stats
WHERE database_id = DB_ID()
ORDER BY user_seeks + user_lookups + user_scans DESC;
```

## Data Growth Estimates

Assuming 1,000 active vehicles, 50 location updates/vehicle/day:

| Table | Daily Records | Yearly Records | Size Growth |
|-------|---|---|---|
| Locations | 50,000 | 18.25M | ~50 GB/year |
| Vehicles | 100 | 36,500 | Minimal |
| Routes | 500 | 182,500 | Minimal |
| TrackingSessions | 1,500 | 547,500 | ~1 GB/year |
| Waypoints | 5,000 | 1.825M | ~500 MB/year |

**Archival Strategy:**
- Keep 3 months of locations in hot storage
- Archive older locations to separate table/database
- Maintain 1 year of session data for analytics
- Delete sessions older than 3 years

## Query Examples

### Get Recent Vehicle Locations

```sql
SELECT TOP 100 *
FROM Locations
WHERE VehicleId = @VehicleId
ORDER BY Timestamp DESC;
```

### Find Vehicles by Status

```sql
SELECT *
FROM Vehicles
WHERE Status = 'Active'
AND IsDeleted = 0
ORDER BY UpdatedAt DESC;
```

### Get Route with Waypoints

```sql
SELECT r.*, w.*
FROM Routes r
LEFT JOIN Waypoints w ON r.Id = w.RouteId
WHERE r.Id = @RouteId
ORDER BY w.[Order];
```

### Nearby Locations (Geospatial)

```sql
SELECT TOP 50 *
FROM Locations
WHERE ABS(Latitude - @Latitude) < 0.1
AND ABS(Longitude - @Longitude) < 0.1
ORDER BY Timestamp DESC;
```

### Session Statistics

```sql
SELECT 
    VehicleId,
    COUNT(*) as SessionCount,
    AVG(TotalDistance) as AvgDistance,
    MAX(MaxSpeed) as TopSpeed
FROM TrackingSessions
WHERE StartTime >= DATEADD(MONTH, -1, GETUTCDATE())
GROUP BY VehicleId;
```

## Maintenance Tasks

### Weekly
```sql
-- Update statistics
EXEC sp_updatestats;

-- Check index fragmentation
```

### Monthly
```sql
-- Rebuild fragmented indexes (>30% fragmentation)
-- Reorganize moderately fragmented indexes (10-30%)

-- Archive old locations
INSERT INTO LocationsArchive
SELECT * FROM Locations
WHERE Timestamp < DATEADD(MONTH, -3, GETUTCDATE());

DELETE FROM Locations
WHERE Timestamp < DATEADD(MONTH, -3, GETUTCDATE());
```

### Quarterly
```sql
-- Full database backup
-- Test restore procedures
-- Analyze query performance
-- Review table sizes and row counts
```

## Backup & Recovery

### Backup Strategy

```sql
-- Full backup daily
-- Transaction log backup every 30 minutes
-- Differential backup every 12 hours
```

### Point-in-Time Recovery Example

```sql
-- Recover database to specific time
RESTORE DATABASE SignalRMapRealtimeDb
FROM DISK = '/backups/SignalRMapRealtimeDb_full.bak'
WITH RECOVERY;

RESTORE LOG SignalRMapRealtimeDb
FROM DISK = '/backups/SignalRMapRealtimeDb_log.trn'
WITH STOPAT = '2024-05-04 10:30:00';
```
