// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

Complete REST API documentation for SignalR Map Realtime.

## Base URL

```
https://your-domain.com/api/v1
```

## Authentication

All API requests require an API key header:

```
X-API-Key: your-api-key-here
```

For development, use:
```
X-API-Key: default-api-key
```

## Response Format

All responses are JSON with consistent structure:

### Success Response
```json
{
  "data": { /* response data */ },
  "success": true,
  "timestamp": "2024-05-04T10:30:00Z"
}
```

### Error Response
```json
{
  "error": "Error message",
  "code": "ERROR_CODE",
  "details": { /* additional info */ },
  "success": false,
  "timestamp": "2024-05-04T10:30:00Z"
}
```

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 204 | No Content - Successful with no response body |
| 400 | Bad Request - Invalid request format |
| 401 | Unauthorized - Missing/invalid API key |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource already exists |
| 422 | Unprocessable Entity - Validation failed |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error - Server error |
| 503 | Service Unavailable - Service temporarily down |

## Pagination

For list endpoints that return multiple items:

### Request Parameters
- `skip` - Number of items to skip (default: 0)
- `take` - Number of items to return (default: 20, max: 100)

### Example
```
GET /vehicles?skip=20&take=50
```

### Response
```json
{
  "data": [ /* array of items */ ],
  "pageSize": 50,
  "totalCount": 1500,
  "hasMore": true
}
```

## Filtering & Searching

### Vehicle Filtering
```
GET /vehicles?status=Active&skip=0&take=20
```

**Filter Parameters:**
- `status` - Active, Inactive, InMaintenance, Retired
- `assignedUserId` - UUID of assigned driver
- `licensePlate` - Partial or exact license plate

### Location Filtering
```
GET /locations/vehicle/{vehicleId}?startDate=2024-05-01&endDate=2024-05-04&limit=1000
```

**Filter Parameters:**
- `startDate` - ISO 8601 datetime (inclusive)
- `endDate` - ISO 8601 datetime (inclusive)
- `limit` - Max 10000 locations

## Vehicles API

### List All Vehicles
```
GET /vehicles
```

**Query Parameters:**
- `skip` - Offset (default: 0)
- `take` - Limit (default: 20, max: 100)
- `status` - Filter by status
- `licensePlate` - Filter by plate

**Response:**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "licensePlate": "ABC-123",
      "manufacturer": "Toyota",
      "model": "Camry",
      "year": 2023,
      "vin": "JTHBP5C28A5034186",
      "status": "Active",
      "fuelLevel": 85.5,
      "odometerReading": 45230,
      "assignedUserId": "550e8400-e29b-41d4-a716-446655440001",
      "lastLocationUpdate": "2024-05-04T10:25:00Z",
      "currentLatitude": 40.7128,
      "currentLongitude": -74.0060,
      "createdAt": "2024-01-15T09:00:00Z",
      "updatedAt": "2024-05-04T10:25:00Z"
    }
  ],
  "pageSize": 20,
  "totalCount": 150,
  "hasMore": true
}
```

### Get Vehicle Details
```
GET /vehicles/{id}
```

**Path Parameters:**
- `id` - Vehicle UUID

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "licensePlate": "ABC-123",
  "manufacturer": "Toyota",
  "model": "Camry",
  "year": 2023,
  "status": "Active",
  "assignedDriver": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890"
  }
}
```

### Create Vehicle
```
POST /vehicles
```

**Request Body:**
```json
{
  "licensePlate": "XYZ-789",
  "manufacturer": "Ford",
  "model": "Transit",
  "year": 2024,
  "vin": "1FTYR14D04TM00001"
}
```

**Validation Rules:**
- `licensePlate` - Required, must be unique, max 20 chars
- `manufacturer` - Required, max 100 chars
- `model` - Required, max 100 chars
- `year` - Required, 1900-2050
- `vin` - Optional, max 50 chars

**Response:** 201 Created
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "licensePlate": "XYZ-789",
  "status": "Active"
}
```

### Update Vehicle
```
PUT /vehicles/{id}
```

**Request Body:** (all fields optional)
```json
{
  "licensePlate": "XYZ-789",
  "status": "InMaintenance",
  "fuelLevel": 75.0
}
```

**Response:** 204 No Content

### Delete Vehicle
```
DELETE /vehicles/{id}
```

**Response:** 204 No Content

**Note:** Soft-deleted vehicles remain in database for audit trail.

## Locations API

### Get Vehicle Location History
```
GET /locations/vehicle/{vehicleId}
```

**Query Parameters:**
- `skip` - Offset (default: 0)
- `take` - Limit (default: 20, max: 100)
- `startDate` - ISO 8601 (inclusive)
- `endDate` - ISO 8601 (inclusive)

**Response:**
```json
{
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
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
  "count": 100
}
```

### Record Location
```
POST /locations
```

**Request Body:**
```json
{
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "accuracy": 5.0,
  "speed": 35.5,
  "heading": 90,
  "type": "GPS",
  "timestamp": "2024-05-04T10:25:00Z"
}
```

**Validation Rules:**
- `vehicleId` - Required, must exist
- `latitude` - Required, -90 to 90
- `longitude` - Required, -180 to 180
- `accuracy` - Optional, >= 0
- `speed` - Optional, >= 0
- `heading` - Optional, 0-360
- `type` - Optional, default "GPS"
- `timestamp` - Optional, defaults to now

**Response:** 201 Created
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "latitude": 40.7128,
  "longitude": -74.0060
}
```

### Find Nearby Locations
```
GET /locations/nearby
```

**Query Parameters:**
- `latitude` - Center latitude (required)
- `longitude` - Center longitude (required)
- `radiusKm` - Search radius (default: 1, max: 50)
- `limit` - Max results (default: 20, max: 100)

**Response:**
```json
{
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
      "latitude": 40.7180,
      "longitude": -74.0050,
      "distanceKm": 0.8,
      "timestamp": "2024-05-04T10:25:00Z"
    }
  ],
  "centerPoint": {
    "latitude": 40.7128,
    "longitude": -74.0060
  },
  "searchRadiusKm": 1.0
}
```

## Routes API

### List Routes
```
GET /routes
```

**Query Parameters:**
- `skip` - Offset (default: 0)
- `take` - Limit (default: 20, max: 100)
- `status` - Filter by status
- `vehicleId` - Filter by vehicle

**Response:**
```json
{
  "data": [
    {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "name": "Downtown Delivery Route",
      "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
      "status": "Active",
      "totalDistance": 25.5,
      "estimatedDuration": 3600,
      "actualDuration": null,
      "createdAt": "2024-05-04T08:00:00Z",
      "updatedAt": "2024-05-04T10:25:00Z"
    }
  ],
  "pageSize": 20,
  "totalCount": 50
}
```

### Create Route
```
POST /routes
```

**Request Body:**
```json
{
  "name": "Downtown Delivery",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Pending",
  "estimatedDuration": 3600,
  "waypoints": [
    {
      "order": 1,
      "latitude": 40.7128,
      "longitude": -74.0060,
      "name": "Start Point"
    },
    {
      "order": 2,
      "latitude": 40.7489,
      "longitude": -73.9680,
      "name": "Stop 1"
    }
  ]
}
```

**Response:** 201 Created
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440000",
  "name": "Downtown Delivery",
  "status": "Pending"
}
```

### Get Route with Waypoints
```
GET /routes/{id}
```

**Response:**
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440000",
  "name": "Downtown Delivery",
  "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Active",
  "waypoints": [
    {
      "id": "880e8400-e29b-41d4-a716-446655440000",
      "order": 1,
      "latitude": 40.7128,
      "longitude": -74.0060,
      "name": "Start Point",
      "estimatedArrivalTime": "2024-05-04T08:00:00Z",
      "actualArrivalTime": null
    }
  ]
}
```

### Update Route
```
PUT /routes/{id}
```

**Request Body:**
```json
{
  "status": "Completed",
  "actualDuration": 3545
}
```

**Response:** 204 No Content

### Delete Route
```
DELETE /routes/{id}
```

**Response:** 204 No Content

## Assets API

### List Assets
```
GET /assets
```

**Query Parameters:**
- `skip` - Offset (default: 0)
- `take` - Limit (default: 20, max: 100)
- `type` - Filter by type (Vehicle, Package, Equipment)
- `status` - Filter by status

**Response:**
```json
{
  "data": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440000",
      "name": "Package #12345",
      "type": "Package",
      "status": "InTransit",
      "lastKnownLocation": "40.7128, -74.0060",
      "lastLocationUpdate": "2024-05-04T10:25:00Z"
    }
  ],
  "pageSize": 20,
  "totalCount": 200
}
```

### Create Asset
```
POST /assets
```

**Request Body:**
```json
{
  "name": "Equipment #001",
  "type": "Equipment",
  "status": "Active",
  "description": "Field service equipment"
}
```

**Response:** 201 Created

## Tracking Sessions API

### Get Vehicle Sessions
```
GET /sessions/vehicle/{vehicleId}
```

**Query Parameters:**
- `status` - Filter by status (Active, Completed, Paused)
- `startDate` - Filter from date
- `endDate` - Filter to date

**Response:**
```json
{
  "data": [
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440000",
      "vehicleId": "550e8400-e29b-41d4-a716-446655440000",
      "status": "Completed",
      "startTime": "2024-05-04T08:00:00Z",
      "endTime": "2024-05-04T11:00:00Z",
      "totalDistance": 45.5,
      "averageSpeed": 30.2,
      "maxSpeed": 75.5,
      "locationCount": 180
    }
  ]
}
```

## Health & Info Endpoints

### Health Check
```
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-05-04T10:30:00Z",
  "version": "1.2.0",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

### API Information
```
GET /api/info
```

**Response:**
```json
{
  "version": "1.2.0",
  "title": "SignalR Map Realtime API",
  "environment": "Production",
  "documentation": "https://api.example.com/swagger"
}
```

## Rate Limiting

API requests are rate limited per IP address.

**Default Limits:**
- 100 requests per minute for standard endpoints
- 1000 requests per hour for data endpoints
- 10 requests per minute for bulk operations

**Headers:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1620000000
```

When rate limited, the server responds with:
```json
{
  "error": "Rate limit exceeded",
  "retryAfter": 60,
  "success": false
}
```

## Error Codes

| Code | HTTP | Description |
|------|------|-------------|
| VALIDATION_ERROR | 422 | Request validation failed |
| NOT_FOUND | 404 | Resource not found |
| DUPLICATE | 409 | Resource already exists |
| UNAUTHORIZED | 401 | Missing or invalid API key |
| FORBIDDEN | 403 | Insufficient permissions |
| RATE_LIMITED | 429 | Too many requests |
| INVALID_INPUT | 400 | Invalid input format |
| INTERNAL_ERROR | 500 | Server error |

## Changelog & Versions

Current API Version: **v1**

See [CHANGELOG.md](../CHANGELOG.md) for detailed changes.
