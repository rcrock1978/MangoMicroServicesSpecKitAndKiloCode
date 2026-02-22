# Mango Microservices - Technical Specification

## Version: 3.0.0 - .NET 10 Edition
## Target Framework: .NET 10 / ASP.NET Core 10
## Language: C# 13
## Architecture: Microservices (E-Commerce Platform)
## Last Updated: 2026-02-22

---

## 1. Solution Architecture Overview

### 1.1 Microservices Portfolio

| Service | Project | Database | Port | Description |
|---------|---------|----------|------|-------------|
| **Gateway** | Mango.GatewaySolution | N/A | 5000 | Ocelot API Gateway |
| **Web (Frontend)** | Mango.Web | N/A | 5001 | ASP.NET Core MVC |
| **Auth API** | Mango.Services.AuthAPI | IdentityDb | 5002 | JWT Authentication |
| **Product API** | Mango.Services.ProductAPI | CatalogDb | 5003 | Product Catalog |
| **Cart API** | Mango.Services.ShoppingCartAPI | CartDb | 5004 | Shopping Cart |
| **Order API** | Mango.Services.OrderAPI | OrderDb | 5005 | Order Management |
| **Coupon API** | Mango.Services.CouponAPI | CouponDb | 5006 | Discount Coupons |
| **Reward API** | Mango.Services.RewardAPI | RewardDb | 5007 | Loyalty Points |
| **Email API** | Mango.Services.EmailAPI | EmailDb | 5008 | Email Service |

### 1.2 Shared Libraries

| Library | Project | Purpose |
|---------|---------|---------|
| **Message Bus** | Mango.MessageBus | RabbitMQ Integration via MassTransit |
| **Common** | Mango.Common | Shared DTOs, Enums, Extensions |
| **Infrastructure** | Mango.Infrastructure | Cross-cutting concerns |

### 1.3 Infrastructure Services

| Service | Image | Ports | Purpose |
|---------|-------|-------|---------|
| **SQL Server** | mcr.microsoft.com/mssql/server:2022-latest | 1433 | Primary Database |
| **Redis** | redis:7-alpine | 6379 | Caching & Session |
| **RabbitMQ** | rabbitmq:3-management-alpine | 5672, 15672 | Message Broker |
| **Jaeger** | jaegertracing/all-in-one:1.47 | 16686 | Distributed Tracing |
| **Seq** | datalust/seq:latest | 5341 | Structured Logging |

---

## 2. Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET | 10.0 |
| Web Framework | ASP.NET Core | 10.0 |
| Language | C# | 13.0 |
| ORM | Entity Framework Core | 10.0 |
| Database | SQL Server | 2022 |
| API Gateway | Ocelot | 25.0+ |
| Message Bus | MassTransit + RabbitMQ | 10.0 + 3.13 |
| Authentication | JWT Bearer | - |
| Mapping | AutoMapper | 13.0+ |
| Logging | Serilog | 4.0+ |
| API Docs | Swagger/OpenAPI | 7.0+ |
| Containers | Docker + Kubernetes | - |
| Observability | OpenTelemetry | 1.0+ |

---

## 3. Clean Architecture Per Service

### 3.1 Layer Structure

```
Mango.Services.{ServiceName}/
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Aggregates/
│   ├── Events/
│   ├── Interfaces/
│   └── Exceptions/
├── Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Validators/
├── Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   ├── Services/
│   ├── Mappings/
│   └── Authentication/
├── Presentation/
│   ├── Controllers/
│   ├── Filters/
│   └── Validators/
└── Program.cs
```

### 3.2 Project References

- **Presentation** → Application, Domain, Infrastructure
- **Application** → Domain
- **Infrastructure** → Domain
- **Domain** → (No dependencies)

---

## 4. API Versioning Strategy

### 4.1 Configuration

```csharp
// Program.cs
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
});

services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

### 4.2 Route Conventions

- `/api/v1/products` - Version 1
- `/api/v2/products` - Version 2
- Response Headers: `api-deprecated-versions: v1`

---

## 5. Health Checks

### 5.1 Service Health Check Configuration

```csharp
// Program.cs
services.AddHealthChecks()
    .AddDbContextCheck<CatalogDbContext>("catalog-db", tags: new[] { "db", "sql" })
    .AddRedis(redisConnectionString, name: "redis", tags: new[] { "cache" })
    .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq", tags: new[] { "mq" })
    .AddUrlGroup(new Uri("https://api.mango.com/health"), name: "gateway");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});
```

### 5.2 Health Check Endpoints

| Endpoint | Purpose |
|----------|---------|
| `/health` | Liveness probe - is the service running? |
| `/ready` | Readiness probe - is the service ready to accept traffic? |

---

## 6. OpenTelemetry Observability

### 6.1 Configuration

```csharp
// Program.cs
services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "ProductAPI")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = environmentName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource("MassTransit")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://jaeger:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());
```

### 6.2 Distributed Tracing

- W3C Trace Context propagation
- Correlation ID for cross-service requests
- Trace sampling: 100% errors, 10% success
- Jaeger UI available at port 16686

---

## 7. Structured Logging with Serilog

### 7.1 Configuration

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "ProductAPI")
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq("http://seq:5341")
    .WriteTo.ApplicationInsights(
        telemetryConverter: new SerializationConverter(),
        restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSerilog(dispose: true);
});
```

### 7.2 Log Structure

```json
{
  "Timestamp": "2026-02-22T15:24:38.689Z",
  "Level": "Information",
  "Message": "Product created successfully",
  "Properties": {
    "ProductId": "guid",
    "UserId": "guid",
    "CorrelationId": "correlation-id",
    "SourceContext": "ProductService"
  }
}
```

---

## 8. Swagger/OpenAPI Documentation

### 8.1 Configuration

```csharp
// Program.cs
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product API",
        Version = "v1",
        Description = "E-Commerce Product Catalog API",
        Contact = new OpenApiContact
        {
            Name = "Mango Team",
            Email = "api@mango.com"
        }
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Api.xml"));
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Application.xml"));
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API V1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
});
```

### 8.2 Swagger Endpoints

| Service | URL |
|---------|-----|
| Gateway | http://localhost:5000/swagger |
| Auth API | http://localhost:5002/swagger |
| Product API | http://localhost:5003/swagger |
| Cart API | http://localhost:5004/swagger |
| Order API | http://localhost:5005/swagger |
| Coupon API | http://localhost:5006/swagger |
| Reward API | http://localhost:5007/swagger |
| Email API | http://localhost:5008/swagger |

---

## 9. JWT Authentication

### 9.1 Configuration

```csharp
// Program.cs
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
    };
});

services.AddAuthorization();
```

### 9.2 Token Settings

| Setting | Value |
|---------|-------|
| Access Token Expiry | 15 minutes |
| Refresh Token Expiry | 7 days |
| Algorithm | HS256 |
| Issuer | mango-api |
| Audience | mango-client |

---

## 10. Message Bus (MassTransit + RabbitMQ)

### 10.1 Configuration

```csharp
// Program.cs
services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<OrderConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(configuration["RabbitMQ:User"]);
            h.Password(configuration["RabbitMQ:Password"]);
        });
        
        cfg.ConfigureEndpoints(context);
    });
});
```

### 10.2 Exchange/Queue Naming Convention

- Exchange: `mango.{servicename}`
- Queue: `{servicename}.{eventname}`
- Dead Letter Queue: `{servicename}.{eventname}.dlq`

---

## 11. Docker Compose Configuration

### 11.1 docker-compose.yml

```yaml
version: '3.8'

services:
  # Infrastructure
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: mango-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=@StrongPassword123
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P @StrongPassword123 -Q "SELECT 1"
      interval: 10s
      timeout: 3s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: mango-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: mango-rabbitmq
    environment:
      - RABBITMQ_DEFAULT_USER=mango
      - RABBITMQ_DEFAULT_PASS=mango123
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: rabbitmq-diagnostics -q ping

  # API Gateway
  mango-gateway:
    build:
      context: ./src
 ../Mango.GatewaySolution/Docker      dockerfile:file
    ports:
      - "5000:80"

  # Web Frontend
  mango-web:
    build:
      context: ./src
      dockerfile: ../Mango.Web/Dockerfile
    ports:
      - "5001:80"

  # Microservices
  auth-api:
    build:
      context: ./src/Services/AuthAPI
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=MangoAuth;User=sa;Password=@StrongPassword123;TrustServerCertificate=True
      - Jwt__SecretKey=SuperSecretKeyForDevelopmentMustBe32Characters!
      - Jwt__Issuer=mango-api
      - Jwt__Audience=mango-client
    ports:
      - "5002:80"
    depends_on:
      sqlserver:
        condition: service_healthy

  product-api:
    build:
      context: ./src/Services/ProductAPI
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=MangoProduct;User=sa;Password=@StrongPassword123;TrustServerCertificate=True
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5003:80"
    depends_on:
      sqlserver:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  cart-api:
    build:
      context: ./src/Services/ShoppingCartAPI
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=MangoCart;User=sa;Password=@StrongPassword123;TrustServerCertificate=True
      - Redis__Host=redis
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5004:80"

  order-api:
    build:
      context: ./src/Services/OrderAPI
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=MangoOrder;User=sa;Password=@StrongPassword123;TrustServerCertificate=True
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5005:80"

  coupon-api:
    build:
      context: ./src/Services/CouponAPI
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=MangoCoupon;User=sa;Password=@StrongPassword123;TrustServerCertificate=True
    ports:
      - "5006:80"

  reward-api:
    build:
      context: ./src/Services/RewardAPI
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=MangoReward;User=sa;Password=@StrongPassword123;TrustServerCertificate=True
    ports:
      - "5007:80"

  email-api:
    build:
      context: ./src/Services/EmailAPI
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=MangoEmail;User=sa;Password=@StrongPassword123;TrustServerCertificate=True
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5008:80"

  # Observability
  jaeger:
    image: jaegertracing/all-in-one:1.47
    ports:
      - "16686:16686"
      - "4317:4317"
      - "4318:4318"

  seq:
    image: datalust/seq:latest
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80"

networks:
  mango-network:
    driver: bridge

volumes:
  sqlserver_data:
  redis_data:
  rabbitmq_data:
```

---

## 12. Kubernetes Manifests

### 12.1 Namespace & ConfigMap

```yaml
# k8s/00-namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: mango-production
  labels:
    environment: production
    app: mango-ecommerce

---
# k8s/01-configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mango-config
  namespace: mango-production
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:80"
  Logging__LogLevel__Default: "Information"
  Database__Provider: "SqlServer"
  Redis__InstanceName: "mango_"
  RabbitMQ__Host: "rabbitmq.mango-messaging.svc.cluster.local"

---
# k8s/02-secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: mango-secrets
  namespace: mango-production
type: Opaque
stringData:
  ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=Mango;User=sa;Password=@StrongPassword"
  ConnectionStrings__Redis: "redis:6379"
  Jwt__SecretKey: "your-256-bit-secret-key-here-min-32-chars"
  Jwt__Issuer: "mango-api"
  Jwt__Audience: "mango-client"
```

### 12.2 Deployment Example (ProductAPI)

```yaml
# k8s/03-productapi-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: product-api
  namespace: mango-production
  labels:
    app: product-api
    version: v1
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: product-api
  template:
    metadata:
      labels:
        app: product-api
        version: v1
    spec:
      serviceAccountName: mango-sa
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
      containers:
      - name: product-api
        image: mango/product-api:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: mango-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: mango-secrets
              key: ConnectionStrings__DefaultConnection
        resources:
          requests:
            memory: "256Mi"
            cpu: "200m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5

---
# k8s/04-productapi-service.yaml
apiVersion: v1
kind: Service
metadata:
  name: product-api
  namespace: mango-production
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 80
  selector:
    app: product-api
```

### 12.3 Horizontal Pod Autoscaler

```yaml
# k8s/05-hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: product-api-hpa
  namespace: mango-production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: product-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### 12.4 Ingress

```yaml
# k8s/10-ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: mango-ingress
  namespace: mango-production
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - api.mangoecommerce.com
    secretName: mango-tls-cert
  rules:
  - host: api.mangoecommerce.com
    http:
      paths:
      - path: /api/auth
        pathType: Prefix
        backend:
          service:
            name: auth-api
            port:
              number: 80
      - path: /api/products
        pathType: Prefix
        backend:
          service:
            name: product-api
            port:
              number: 80
      - path: /api/cart
        pathType: Prefix
        backend:
          service:
            name: cart-api
            port:
              number: 80
      - path: /api/orders
        pathType: Prefix
        backend:
          service:
            name: order-api
            port:
              number: 80
```

---

## 13. Docker Dockerfile Template

```dockerfile
# Multi-stage Dockerfile for .NET 10 Microservice
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Services/{ServiceName}/Mango.Services.{ServiceName}.csproj", "src/Services/{ServiceName}/"]
RUN dotnet restore "src/Services/{ServiceName}/Mango.Services.{ServiceName}.csproj"
COPY . .
RUN dotnet build "src/Services/{ServiceName}/Mango.Services.{ServiceName}.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/{ServiceName}/Mango.Services.{ServiceName}.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 appuser
USER appuser

ENTRYPOINT ["dotnet", "Mango.Services.{ServiceName}.dll"]
```

---

## 14. Quick Start Commands

### Docker Compose

```bash
# Start all services
docker-compose up -d

# Start specific service
docker-compose up -d product-api

# View logs
docker-compose logs -f auth-api

# Stop all services
docker-compose down

# Rebuild services
docker-compose build --no-cache

# Full reset (remove volumes)
docker-compose down -v
```

### Kubernetes

```bash
# Apply manifests
kubectl apply -f k8s/

# Check status
kubectl get pods -n mango-production

# View logs
kubectl logs -n mango-production -l app=product-api

# Scale deployment
kubectl scale deployment product-api --replicas=5 -n mango-production
```

---

## 15. Development Workflow

### 15.1 Prerequisites

- .NET 10 SDK
- Docker Desktop
- Visual Studio 2026 or VS Code
- SQL Server Management Studio

### 15.2 Running Locally

1. Clone repository
2. Run `docker-compose up -d` (infrastructure only)
3. Run individual services from IDE
4. Access Gateway at http://localhost:5000

### 15.3 Database Migrations

```bash
# Add migration
dotnet ef migrations add InitialCreate --project src/Services/ProductAPI

# Apply migrations
dotnet ef database update --project src/Services/ProductAPI

# Remove migration
dotnet ef migrations remove --project src/Services/ProductAPI
```

---

## 16. Quality Standards Checklist

- [ ] Clean Architecture implemented
- [ ] Domain-Driven Design bounded contexts
- [ ] Repository pattern with Unit of Work
- [ ] Mediator pattern (CQRS) for commands/queries
- [ ] FluentValidation for input validation
- [ ] JWT authentication configured
- [ ] Role-based authorization
- [ ] Database-per-service pattern
- [ ] Event-driven architecture (RabbitMQ)
- [ ] Circuit breaker pattern
- [ ] Structured logging in place
- [ ] Health checks configured
- [ ] API versioning documented
- [ ] OpenTelemetry tracing enabled
- [ ] Swagger documentation
- [ ] Docker image optimized
- [ ] Kubernetes deployment ready

---

## 17. File Structure

```
MangoMicroservice/
├── speckit.constitution
├── README.md
├── src/
│   ├── Mango.sln
│   ├── Services/
│   │   ├── AuthAPI/
│   │   │   ├── Mango.Services.AuthAPI.csproj
│   │   │   ├── Domain/
│   │   │   ├── Application/
│   │   │   ├── Infrastructure/
│   │   │   ├── Presentation/
│   │   │   ├── Program.cs
│   │   │   └── Dockerfile
│   │   ├── ProductAPI/
│   │   ├── ShoppingCartAPI/
│   │   ├── OrderAPI/
│   │   ├── CouponAPI/
│   │   ├── RewardAPI/
│   │   └── EmailAPI/
│   ├── Gateway/
│   │   └── Mango.GatewaySolution/
│   ├── Web/
│   │   └── Mango.Web/
│   └── Shared/
│       ├── Mango.MessageBus/
│       ├── Mango.Common/
│       └── Mango.Infrastructure/
├── docker-compose.yml
├── .env
└── k8s/
    ├── 00-namespace.yaml
    ├── 01-configmap.yaml
    ├── 02-secret.yaml
    ├── 03-productapi-deployment.yaml
    ├── 04-productapi-service.yaml
    ├── 05-hpa.yaml
    └── 10-ingress.yaml
```

---

*This specification is based on the Mango Microservices Technical Constitution v3.0.0*
