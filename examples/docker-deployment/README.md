# Docker Deployment Example

This example demonstrates how to deploy signalr-map-realtime using Docker and Docker Compose.

## Quick Start

### Prerequisites
- Docker (v20.10+)
- Docker Compose (v2.0+)
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/sarmkadan/signalr-map-realtime.git
cd signalr-map-realtime
```

### 2. Build and Run with Docker Compose

```bash
# Build the application image
docker-compose build

# Start all services (API, SQL Server, Redis)
docker-compose up -d

# Verify services are running
docker-compose ps
```

### 3. Access the Application

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### 4. Verify Database Setup

```bash
# Check SQL Server logs
docker-compose logs sql-server

# Connect to SQL Server (password: Your_password123)
docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "Your_password123" \
  -Q "SELECT name FROM sys.databases"
```

## Environment Variables

The docker-compose.yml file includes all necessary environment variables. Key configurations:

| Variable | Description | Default Value |
|----------|-------------|---------------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` |
| `ASPNETCORE_URLS` | Application URLs | `http://0.0.0.0:5000` |
| `ConnectionStrings__DefaultConnection` | SQL Server connection | `Server=sql-server;Database=SignalRMapRealtimeDb;...` |
| `Caching__Enabled` | Enable caching | `true` |
| `RateLimiting__Enabled` | Enable rate limiting | `true` |

## Customizing the Deployment

### Change Ports

Edit the `ports` section in docker-compose.yml:

```yaml
ports:
  - "8080:5000"  # Map host port 8080 to container port 5000
```

### Use PostgreSQL Instead of SQL Server

1. Update `docker-compose.yml` to use PostgreSQL service
2. Change connection string in `appsettings.Docker.json`
3. Rebuild and restart:

```bash
docker-compose down
docker-compose build
docker-compose up -d
```

### Enable Redis for SignalR Backplane

```yaml
services:
  signalr-map-realtime:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://0.0.0.0:5000
      - ConnectionStrings__DefaultConnection=Server=sql-server;...;
      - SignalR__BackplaneType=Redis
      - SignalR__RedisConnectionString=redis:6379
```

## Production Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure proper database connection string
- [ ] Set up HTTPS with valid certificate
- [ ] Configure Redis for SignalR backplane in production
- [ ] Set up proper logging (ELK stack, Datadog, etc.)
- [ ] Configure monitoring and health checks
- [ ] Set up backup for SQL Server database
- [ ] Configure firewall rules
- [ ] Set up load balancer for multiple instances
- [ ] Enable rate limiting and request validation
- [ ] Configure CORS for your frontend domains
- [ ] Set up proper authentication (API keys, JWT, etc.)

## Troubleshooting

### Common Issues

#### Database connection fails

**Error**: `Login failed for user 'SA'`

**Solution**: Wait for SQL Server to fully initialize (takes 1-2 minutes). Check logs:

```bash
docker-compose logs sql-server
```

#### Port already in use

**Error**: `Port 5000 is already allocated`

**Solution**: Change the port mapping in docker-compose.yml:

```yaml
ports:
  - "8080:5000"
```

#### Slow performance

**Solution**: Check resource allocation:

```bash
docker stats
```

Increase CPU/memory limits in docker-compose.yml:

```yaml
services:
  signalr-map-realtime:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
```

### Useful Commands

```bash
# View logs
docker-compose logs -f signalr-map-realtime

# Restart a specific service
docker-compose restart signalr-map-realtime

# Rebuild and restart
docker-compose up -d --build

# Clean up (removes volumes!)
docker-compose down -v

# Enter container for debugging
docker exec -it signalr-map-realtime sh

# Check running processes
docker top signalr-map-realtime
```

## Scaling with Docker

For production deployments with multiple instances:

```bash
# Scale API instances (requires Redis backplane)
docker-compose up -d --scale signalr-map-realtime=3

# Use load balancer (nginx, HAProxy, Traefik) to distribute traffic
```

## Health Checks

The application includes built-in health checks:

```bash
# Check application health
curl http://localhost:5000/health

# Expected response:
# {
#   "status": "Healthy",
#   "timestamp": "2024-05-18T10:00:00Z",
#   "version": "2.0.0"
# }
```

## Security Considerations

- Change default SQL Server password in production
- Use HTTPS in production (configure in docker-compose.yml)
- Set up proper authentication (API keys, OAuth, etc.)
- Configure network isolation between containers
- Use secrets management for sensitive data
- Regularly update base images

## Performance Tuning

### Database Performance

```yaml
services:
  sql-server:
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_strong_password123
      - MSSQL_PID=Express
    volumes:
      - sql-data:/var/opt/mssql
```

### Application Performance

```yaml
services:
  signalr-map-realtime:
    environment:
      - Caching__Enabled=true
      - Caching__DurationSeconds=300
      - RateLimiting__Enabled=true
      - RateLimiting__RequestsPerMinute=100
```

## Complete Example: Production-Ready Setup

For a production deployment, create a `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  signalr-map-realtime:
    image: your-registry/signalr-map-realtime:2.0.0
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://0.0.0.0:5001
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=SignalRMapRealtimeDb;User=sa;Password=Your_secure_password123;TrustServerCertificate=true
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=Your_cert_password
      - SignalR__BackplaneType=Redis
      - SignalR__RedisConnectionString=redis:6379
    ports:
      - "443:5001"
    depends_on:
      - sql-server
      - redis
    networks:
      - app-network
    restart: unless-stopped

  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_secure_password123
      - MSSQL_PID=Standard
    volumes:
      - sql-data:/var/opt/mssql
    networks:
      - app-network
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - app-network
    restart: unless-stopped

volumes:
  sql-data:
  redis-data:

networks:
  app-network:
    driver: bridge
```

Then deploy:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

**Next Steps**:
- [ ] Configure your frontend to connect to the API
- [ ] Set up proper authentication
- [ ] Configure monitoring and alerts
- [ ] Set up CI/CD pipeline for automated deployments
