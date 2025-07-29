# 🐳 Deployment Guide

Complete deployment documentation for the Star Wars API.

## Overview

The Star Wars API supports multiple deployment strategies from local development to production containerized environments. This guide covers Docker deployment, production configuration, scaling considerations, and best practices.

## Quick Start Options

### Option 1: Docker Compose (Recommended)
```bash
git clone https://github.com/yourusername/star-wars-api.git
cd star-wars-api
docker-compose up -d
```
- **API**: http://localhost:8080
- **SQL Server**: localhost:1433

### Option 2: Local Development
```bash
git clone https://github.com/yourusername/star-wars-api.git
cd star-wars-api
dotnet restore
dotnet run
```
- **API**: https://localhost:7108
- **Swagger**: https://localhost:7108/swagger

## Docker Deployment

### Development Environment

#### Docker Compose Configuration
```yaml
# docker-compose.yml
version: '3.8'

services:
  starwars-api:
    build: .
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=StarWarsDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - starwars-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: ["/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourStrong@Passw0rd", "-C", "-Q", "SELECT 1"]
      interval: 10s
      retries: 5
      start_period: 30s
      timeout: 5s
    networks:
      - starwars-network

volumes:
  sqlserver_data:

networks:
  starwars-network:
    driver: bridge
```

#### Start Development Environment
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f starwars-api

# Stop services
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v
```

### Production Environment

#### Production Docker Compose
```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  starwars-api:
    build: 
      context: .
      dockerfile: Dockerfile.prod
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=StarWarsDB;User Id=sa;Password=${SQL_SA_PASSWORD};TrustServerCertificate=true;
      - ApiKeySettings__ExpirationMinutes=30
      - Logging__LogLevel__Default=Information
      - Authentication__RequireHttps=true
    volumes:
      - ./logs:/app/logs
      - ./certs:/app/certs
    depends_on:
      sqlserver:
        condition: service_healthy
    restart: unless-stopped
    networks:
      - starwars-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SQL_SA_PASSWORD}
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./sql-backup:/backup
    healthcheck:
      test: ["/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "${SQL_SA_PASSWORD}", "-C", "-Q", "SELECT 1"]
      interval: 30s
      retries: 3
      start_period: 60s
      timeout: 10s
    restart: unless-stopped
    networks:
      - starwars-network

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./certs:/etc/nginx/certs
    depends_on:
      - starwars-api
    restart: unless-stopped
    networks:
      - starwars-network

volumes:
  sqlserver_data:

networks:
  starwars-network:
    driver: bridge
```

#### Environment Variables
```bash
# .env.prod
SQL_SA_PASSWORD=YourVeryStrong@ProductionPassw0rd123
API_ADMIN_KEY=FOR-PADME-FOR-LOVE
ASPNETCORE_HTTPS_PORT=443
```

#### Start Production Environment
```bash
# Use production configuration
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d

# Monitor production logs
docker-compose -f docker-compose.prod.yml logs -f --tail=100
```

### Dockerfile Configuration

#### Development Dockerfile
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Star Wars.csproj", "."]
RUN dotnet restore "./Star Wars.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Star Wars.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Star Wars.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Star Wars.dll"]
```

#### Production Dockerfile
```dockerfile
# Dockerfile.prod
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install debugging tools for production monitoring
RUN apt-get update && apt-get install -y curl htop && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Star Wars.csproj", "."]
RUN dotnet restore "./Star Wars.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Star Wars.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Star Wars.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chmod 755 /app/logs

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "Star Wars.dll"]
```

## Container Orchestration

### Docker Swarm Deployment

#### Initialize Swarm
```bash
# Initialize Docker Swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.prod.yml starwars-stack

# Scale services
docker service scale starwars-stack_starwars-api=3

# Update service
docker service update --image starwars-api:latest starwars-stack_starwars-api
```

#### Stack Configuration
```yaml
# docker-stack.yml
version: '3.8'

services:
  starwars-api:
    image: starwars-api:latest
    ports:
      - "80:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
    networks:
      - starwars-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD_FILE=/run/secrets/sql_password
    deploy:
      replicas: 1
      restart_policy:
        condition: on-failure
      resources:
        limits:
          cpus: '1.0'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 1G
    secrets:
      - sql_password
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - starwars-network

secrets:
  sql_password:
    external: true

volumes:
  sqlserver_data:

networks:
  starwars-network:
    driver: overlay
```

### Kubernetes Deployment

#### Deployment Configuration
```yaml
# k8s-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: starwars-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: starwars-api
  template:
    metadata:
      labels:
        app: starwars-api
    spec:
      containers:
      - name: starwars-api
        image: starwars-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: starwars-secrets
              key: connection-string
        resources:
          limits:
            cpu: 500m
            memory: 512Mi
          requests:
            cpu: 250m
            memory: 256Mi
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: starwars-api-service
spec:
  selector:
    app: starwars-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

#### Deploy to Kubernetes
```bash
# Apply configuration
kubectl apply -f k8s-deployment.yml

# Scale deployment
kubectl scale deployment starwars-api --replicas=5

# Update image
kubectl set image deployment/starwars-api starwars-api=starwars-api:v2.0

# Monitor rollout
kubectl rollout status deployment/starwars-api
```

## Cloud Deployment

### Azure Container Instances

#### Deploy to Azure
```bash
# Create resource group
az group create --name starwars-rg --location eastus

# Create container instance
az container create \
  --resource-group starwars-rg \
  --name starwars-api \
  --image starwars-api:latest \
  --dns-name-label starwars-api-unique \
  --ports 80 443 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
  --secure-environment-variables \
    ConnectionStrings__DefaultConnection="Server=..." \
  --cpu 2 \
  --memory 4
```

### AWS ECS Deployment

#### Task Definition
```json
{
  "family": "starwars-api",
  "networkMode": "awsvpc",
  "requiresAttributes": [
    {
      "name": "com.amazonaws.ecs.capability.docker-remote-api.1.25"
    }
  ],
  "cpu": "512",
  "memory": "1024",
  "containerDefinitions": [
    {
      "name": "starwars-api",
      "image": "your-account.dkr.ecr.region.amazonaws.com/starwars-api:latest",
      "portMappings": [
        {
          "containerPort": 80,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__DefaultConnection",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:starwars-db-connection"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/starwars-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

## Configuration Management

### Production Configuration

#### appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "System": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver,1433;Database=StarWarsDB;User Id=sa;Password=${SQL_SA_PASSWORD};TrustServerCertificate=true;"
  },
  "Authentication": {
    "RequireHttps": true,
    "CookieSecure": true,
    "AllowInsecurePasswords": false,
    "RequireEmailConfirmation": true,
    "LockoutEnabled": true,
    "ApiKeyExpirationMinutes": 30,
    "MaxFailedAccessAttempts": 3,
    "LockoutTimeMinutes": 60
  },
  "ApiKeySettings": {
    "ExpirationMinutes": 30,
    "CookieSecure": true,
    "CookieSameSite": "Strict",
    "RequireHttps": true
  },
  "AdminSettings": {
    "AdminKey": "${API_ADMIN_KEY}"
  },
  "AllowedHosts": "*"
}
```

### Environment Variables
```bash
# Production environment variables
export ASPNETCORE_ENVIRONMENT=Production
export SQL_SA_PASSWORD=YourVeryStrong@ProductionPassw0rd123
export API_ADMIN_KEY=FOR-PADME-FOR-LOVE
export ConnectionStrings__DefaultConnection="Server=sqlserver,1433;Database=StarWarsDB;User Id=sa;Password=${SQL_SA_PASSWORD};TrustServerCertificate=true;"
```

### Secrets Management

#### Docker Secrets
```bash
# Create secrets
echo "YourVeryStrong@ProductionPassw0rd123" | docker secret create sql_password -
echo "FOR-PADME-FOR-LOVE" | docker secret create admin_key -

# Use in docker-compose
services:
  starwars-api:
    environment:
      - SQL_SA_PASSWORD_FILE=/run/secrets/sql_password
      - API_ADMIN_KEY_FILE=/run/secrets/admin_key
    secrets:
      - sql_password
      - admin_key
```

#### Kubernetes Secrets
```bash
# Create secret
kubectl create secret generic starwars-secrets \
  --from-literal=connection-string="Server=..." \
  --from-literal=admin-key="FOR-PADME-FOR-LOVE"

# Use in deployment
env:
- name: ConnectionStrings__DefaultConnection
  valueFrom:
    secretKeyRef:
      name: starwars-secrets
      key: connection-string
```

## Monitoring & Health Checks

### Health Check Endpoints
```csharp
// Configure in Program.cs
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://swapi.info/api/"), "SWAPI");

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### Monitoring with Prometheus
```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'starwars-api'
    static_configs:
      - targets: ['starwars-api:80']
    metrics_path: '/metrics'
    scrape_interval: 5s
```

### Log Aggregation
```yaml
# docker-compose monitoring stack
version: '3.8'

services:
  starwars-api:
    # ... existing config
    logging:
      driver: "fluentd"
      options:
        fluentd-address: localhost:24224
        tag: starwars.api

  fluentd:
    image: fluent/fluentd:v1.14-1
    volumes:
      - ./fluentd.conf:/fluentd/etc/fluent.conf
    ports:
      - "24224:24224"
    depends_on:
      - elasticsearch

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:7.15.0
    environment:
      - discovery.type=single-node
    ports:
      - "9200:9200"

  kibana:
    image: docker.elastic.co/kibana/kibana:7.15.0
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
```

## Performance Optimization

### Database Optimization
```sql
-- Production indexes
CREATE INDEX IX_AspNetUsers_ApiKey ON AspNetUsers(ApiKey) 
  WHERE ApiKey IS NOT NULL;

CREATE INDEX IX_AspNetUsers_Email ON AspNetUsers(Email);

CREATE INDEX IX_Starships_Name ON Starships(Name);
CREATE INDEX IX_Starships_Manufacturer ON Starships(Manufacturer);
CREATE INDEX IX_Starships_Class ON Starships(StarshipClass);

-- Connection pooling
ALTER DATABASE StarWarsDB SET MAX_POOL_SIZE = 100;
```

### Application Performance
```csharp
// Configure in Program.cs
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 30000000; // 30MB
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 100;
});
```

## Security Hardening

### HTTPS Configuration
```csharp
// Program.cs production security
if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

### Nginx Reverse Proxy
```nginx
# nginx.conf
server {
    listen 80;
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/nginx/certs/cert.pem;
    ssl_certificate_key /etc/nginx/certs/key.pem;

    location / {
        proxy_pass http://starwars-api:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Security headers
    add_header X-Content-Type-Options nosniff;
    add_header X-Frame-Options DENY;
    add_header X-XSS-Protection "1; mode=block";
}
```

## Backup & Disaster Recovery

### Database Backup
```bash
# Automated backup script
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="starwars_backup_${DATE}.bak"

docker exec starwars-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "${SQL_SA_PASSWORD}" -C \
  -Q "BACKUP DATABASE StarWarsDB TO DISK = '/backup/${BACKUP_FILE}'"

# Upload to cloud storage
aws s3 cp "/backup/${BACKUP_FILE}" "s3://your-backup-bucket/"
```

### Restore Process
```bash
# Restore from backup
docker exec starwars-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "${SQL_SA_PASSWORD}" -C \
  -Q "RESTORE DATABASE StarWarsDB FROM DISK = '/backup/starwars_backup_20250729_120000.bak' WITH REPLACE"
```

## Troubleshooting

### Common Deployment Issues

| Issue | Symptoms | Solution |
|-------|----------|----------|
| **Container Won't Start** | API container exits immediately | Check logs: `docker logs starwars-api` |
| **Database Connection** | "Cannot connect to SQL Server" | Verify SQL Server is healthy: `docker exec starwars-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P password -C -Q "SELECT 1"` |
| **Port Conflicts** | "Port already in use" | Change port mapping or stop conflicting services |
| **Memory Issues** | Container OOM killed | Increase memory limits in docker-compose |
| **SSL Certificate** | HTTPS not working | Check certificate paths and permissions |

### Diagnostic Commands
```bash
# Check container status
docker ps -a

# View container logs
docker logs starwars-api --tail=100 -f

# Inspect container
docker inspect starwars-api

# Execute commands in container
docker exec -it starwars-api bash

# Check resource usage
docker stats

# Network connectivity
docker exec starwars-api curl -I http://localhost:80/health
```

### Performance Monitoring
```bash
# Monitor API performance
curl -w "@curl-format.txt" -o /dev/null -s "http://localhost:8080/api/starships"

# Database performance
docker exec starwars-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P password -C \
  -Q "SELECT TOP 10 * FROM sys.dm_exec_query_stats ORDER BY total_elapsed_time DESC"
```

---

**[← Back to Main README](../README.md)**
