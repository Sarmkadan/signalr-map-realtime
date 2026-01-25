// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

This guide covers deploying SignalR Map Realtime to production environments.

## Pre-Deployment Checklist

- [ ] All tests passing: `dotnet test`
- [ ] Build successful: `dotnet build -c Release`
- [ ] Configuration reviewed and secure
- [ ] Database backups configured
- [ ] Logging configured
- [ ] Monitoring/alerting setup
- [ ] Load testing completed
- [ ] Security scanning passed
- [ ] Documentation updated
- [ ] Release notes prepared

## Local/Development Environment

### Prerequisites

```bash
# Verify .NET installation
dotnet --version

# Install SQL Server LocalDB (Windows)
# Or use Docker: docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=..." -p 1433:1433 mssql/server:2019-latest
```

### Setup

```bash
# Clone repository
git clone https://github.com/Sarmkadan/signalr-map-realtime.git
cd signalr-map-realtime

# Restore and build
dotnet restore
dotnet build

# Apply migrations
dotnet ef database update --project src/SignalRMapRealtime

# Run
dotnet run --project src/SignalRMapRealtime --configuration Development
```

### Configuration

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SignalRMapRealtime;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "SignalRMapRealtime": "Debug"
    }
  }
}
```

## Docker Deployment

### Building Image

```bash
# Single-stage build
docker build -t signalr-map-realtime:latest .

# Multi-stage build (optimized)
docker build -t signalr-map-realtime:latest --target runtime .

# With version tag
docker build -t signalr-map-realtime:1.0.0 .
```

### Running Container

```bash
# Basic run
docker run -p 5001:8080 signalr-map-realtime:latest

# With environment variables
docker run -p 5001:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=sql-server;Database=SignalRMapRealtimeDb;User Id=sa;Password=YourPassword;" \
  signalr-map-realtime:latest

# With volume mount
docker run -p 5001:8080 \
  -v /path/to/appsettings.json:/app/appsettings.json \
  signalr-map-realtime:latest
```

### Docker Compose

Start entire stack (app + database):

```bash
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

See `docker-compose.yml` for complete configuration with SQL Server and Redis.

## Cloud Deployment

### Azure App Service

```bash
# Create resource group
az group create --name signalr-rg --location eastus

# Create App Service plan
az appservice plan create \
  --name signalr-plan \
  --resource-group signalr-rg \
  --sku B2

# Create web app
az webapp create \
  --resource-group signalr-rg \
  --plan signalr-plan \
  --name signalr-map-realtime

# Publish
dotnet publish -c Release -o ./publish
# Upload publish folder to Azure
```

### AWS Elastic Beanstalk

```bash
# Install EB CLI
pip install awsebcli

# Initialize
eb init -p "IIS 10.0" signalr-map-realtime

# Create environment
eb create production

# Deploy
eb deploy

# Monitor
eb status
```

### Google Cloud Run

```bash
# Build image
gcloud builds submit --tag gcr.io/PROJECT_ID/signalr-map-realtime

# Deploy
gcloud run deploy signalr-map-realtime \
  --image gcr.io/PROJECT_ID/signalr-map-realtime \
  --platform managed \
  --region us-central1

# Set environment variables
gcloud run deploy signalr-map-realtime \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production
```

## Kubernetes Deployment

### Prerequisites

```bash
# Install kubectl
kubectl version --client

# Connect to cluster
kubectl config use-context your-cluster
```

### Deployment Manifest

Create `k8s/deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: signalr-map-realtime
spec:
  replicas: 3
  selector:
    matchLabels:
      app: signalr-map-realtime
  template:
    metadata:
      labels:
        app: signalr-map-realtime
    spec:
      containers:
      - name: app
        image: your-registry/signalr-map-realtime:1.0.0
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: signalr-map-realtime
spec:
  selector:
    app: signalr-map-realtime
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: LoadBalancer
```

### Deploy to Kubernetes

```bash
# Create secret for database
kubectl create secret generic db-secret \
  --from-literal=connection-string='Server=sql-server;...'

# Apply deployment
kubectl apply -f k8s/deployment.yaml

# Monitor
kubectl get pods
kubectl logs -f pod/signalr-map-realtime-xxxxx
kubectl describe service signalr-map-realtime
```

## Database Deployment

### SQL Server Setup

```sql
-- Create database
CREATE DATABASE SignalRMapRealtimeDb;

-- Create user
CREATE LOGIN AppUser WITH PASSWORD = 'SecurePassword123!';
CREATE USER AppUser FOR LOGIN AppUser;

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER AppUser;
```

### Entity Framework Migrations

```bash
# Generate migration
dotnet ef migrations add InitialCreate

# Apply to development
dotnet ef database update

# Apply to production (with backup)
dotnet ef database update --environment Production
```

### Database Backup Strategy

```sql
-- Full backup
BACKUP DATABASE SignalRMapRealtimeDb
TO DISK = '/backups/SignalRMapRealtimeDb_full.bak'
WITH COMPRESSION;

-- Differential backup
BACKUP DATABASE SignalRMapRealtimeDb
TO DISK = '/backups/SignalRMapRealtimeDb_diff.bak'
WITH DIFFERENTIAL, COMPRESSION;

-- Transaction log backup
BACKUP LOG SignalRMapRealtimeDb
TO DISK = '/backups/SignalRMapRealtimeDb_log.trn'
WITH COMPRESSION;
```

## Production Configuration

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=production-sql-server;Database=SignalRMapRealtimeDb;User Id=sa;Password=VERY_SECURE_PASSWORD;Encrypt=true;Connection Timeout=30;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "SignalRMapRealtime": "Information"
    },
    "File": {
      "Path": "/var/log/signalr-map/",
      "Enabled": true
    }
  },
  "AppSettings": {
    "Environment": "Production",
    "EnableSwagger": false,
    "RequestTimeoutSeconds": 30
  },
  "Cors": {
    "AllowedOrigins": [
      "https://your-domain.com",
      "https://app.your-domain.com"
    ]
  },
  "Caching": {
    "Enabled": true,
    "DurationSeconds": 600,
    "RedisConnectionString": "production-redis:6379"
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 1000,
    "BurstSize": 2000
  }
}
```

## Health Checks & Monitoring

### Health Check Endpoint

```bash
curl https://your-domain.com/health
```

Response:
```json
{
  "status": "Healthy",
  "timestamp": "2024-05-04T10:30:00Z",
  "version": "1.0.0",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

### Application Insights Integration

```csharp
// In Program.cs
services.AddApplicationInsightsTelemetry(configuration);
```

Monitor from Azure Portal:
- Request rates
- Response times
- Exceptions
- Custom metrics

### Prometheus Metrics

Add endpoint for Prometheus scraping:

```csharp
app.MapMetrics(); // /metrics endpoint
```

## Load Testing

### Using Apache Bench

```bash
# 1000 requests, 10 concurrent
ab -n 1000 -c 10 https://your-domain.com/api/v1/vehicles

# With headers
ab -n 1000 -H "Authorization: Bearer token" https://your-domain.com/api/v1/vehicles
```

### Using k6

```javascript
// test.js
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  vus: 100,
  duration: '1m'
};

export default function () {
  let res = http.get('https://your-domain.com/api/v1/vehicles');
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
}
```

Run: `k6 run test.js`

## SSL/TLS Configuration

### Self-Signed Certificate (Development)

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Production Certificate

Using Let's Encrypt on Linux:

```bash
sudo apt-get install certbot

sudo certbot certonly --standalone -d your-domain.com

# Copy certificate to app
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /app/cert.pem
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /app/key.pem
```

In Program.cs:
```csharp
app.UseHttpsRedirection();
// Kestrel will use HTTPS by default
```

## Scaling Strategy

### Horizontal Scaling

1. **Multiple App Instances**
   - Load balancer distributes requests
   - Sticky sessions for SignalR
   - Shared session store

2. **SignalR Backplane**
   ```csharp
   services.AddSignalR()
       .AddStackExchangeRedis(options =>
       {
           options.ConnectionFactory = async writer =>
               await ConnectionMultiplexer.ConnectAsync("production-redis:6379");
       });
   ```

3. **Database Replication**
   - Read replicas for queries
   - Master-slave setup

### Vertical Scaling

Increase instance resources:
- More CPU cores
- More RAM
- Faster storage (SSD)

## Monitoring & Logging

### Structured Logging

```csharp
_logger.LogInformation("Vehicle {VehicleId} created by {UserId}", vehicleId, userId);
```

Log aggregation:
- **ELK Stack**: Elasticsearch, Logstash, Kibana
- **Splunk**: Enterprise logging
- **DataDog**: APM + Logging
- **New Relic**: Application monitoring

### Key Metrics to Monitor

- **API Metrics**
  - Request count
  - Response time (p50, p95, p99)
  - Error rate
  - Status code distribution

- **Business Metrics**
  - Active vehicles
  - Locations per minute
  - Active sessions

- **System Metrics**
  - CPU usage
  - Memory usage
  - Disk I/O
  - Network bandwidth

- **SignalR Metrics**
  - Connected clients
  - Message throughput
  - Reconnection rate

## Disaster Recovery

### Backup Strategy

```bash
# Daily full backup
0 2 * * * /backup/full-backup.sh

# Hourly transaction log backup
0 * * * * /backup/log-backup.sh

# Weekly archive backup (off-site)
0 3 * * 0 /backup/archive-backup.sh
```

### Restore Procedure

```sql
-- Restore full backup
RESTORE DATABASE SignalRMapRealtimeDb
FROM DISK = '/backups/SignalRMapRealtimeDb_full.bak'
WITH REPLACE, RECOVERY;

-- Or point-in-time restore
RESTORE LOG SignalRMapRealtimeDb
FROM DISK = '/backups/SignalRMapRealtimeDb_log.trn'
WITH STOPAT = '2024-05-04 10:30:00';
```

### RTO/RPO Targets

- **RTO** (Recovery Time Objective): < 1 hour
- **RPO** (Recovery Point Objective): < 15 minutes

## Post-Deployment

1. **Smoke Testing**: Verify core functionality
2. **User Acceptance Testing**: UAT environment
3. **Performance Baseline**: Establish metrics
4. **Security Audit**: Final review
5. **Documentation Update**: Keep runbooks current
6. **Team Training**: Deployment procedures
7. **Monitoring Setup**: Alerts configured
8. **Incident Response**: On-call rotation ready

## Rollback Procedure

If issues arise:

```bash
# Kubernetes
kubectl rollout undo deployment/signalr-map-realtime

# Database (if schema changed)
dotnet ef database update --migration <previous>

# Docker/App Service
# Re-deploy previous version
```

## Security Hardening

1. **Network**
   - HTTPS/TLS required
   - WAF rules enabled
   - DDoS protection

2. **Authentication**
   - API key rotation
   - Token expiration
   - MFA for admin access

3. **Database**
   - Encryption at rest
   - Backup encryption
   - Audit logging

4. **Application**
   - Input validation
   - SQL injection prevention
   - XSS protection
   - CSRF tokens
