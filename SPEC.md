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

### 5.1.1 Health Check Alerting

| Condition | Alert Level | Action |
|-----------|-------------|--------|
| /health returns Unhealthy | Critical | Page on-call immediately |
| /ready returns Unhealthy | Warning | Create incident |
| DB check fails | Warning | Create incident |
| Redis check fails | Warning | Create incident |
| 3+ consecutive failures | Critical | Page on-call |

**Health Check Response Time:**
- Expected response time: <500ms
- Timeout: 5 seconds
- Retry interval: 10 seconds

### 5.2 Metric Collection Standards

| Metric Type | Collection Interval | Retention | Aggregation |
|-------------|-------------------|-----------|-------------|
| Request Rate | 10 seconds | 30 days | Sum per interval |
| Response Time | 10 seconds | 30 days | P50, P90, P99 |
| Error Rate | 10 seconds | 30 days | Sum per interval |
| CPU Usage | 10 seconds | 7 days | Avg per interval |
| Memory Usage | 10 seconds | 7 days | Avg per interval |
| Custom Metrics | 30 seconds | 90 days | Per metric type |

**Metrics Export:**
- Push to Prometheus every 10 seconds
- Push to Application Insights every 60 seconds
- Export to OpenTelemetry Collector

### 5.3.1 Circuit Breaker Pattern

| State | Threshold | Action |
|-------|-----------|--------|
| Closed | Normal | Pass through |
| Open | 5 failures/10s | Block requests |
| Half-Open | 3 success | Test recovery |

**Configuration:**
| Setting | Value |
|---------|-------|
| Failure Threshold | 5 |
| Timeout | 30 seconds |
| Sampling Duration | 10 seconds |

### 5.3.2 Trace Context Propagation

| Header | Format | Required |
|--------|--------|----------|
| traceparent | `00-{trace-id}-{span-id}-{flags}` | Yes |
| trace-state | key=value | No |

**W3C Compliance:** All services MUST propagate trace context

### 5.3.3 High Cardinality Metrics

| Metric Type | Recommended Tags | Avoid |
|-------------|-------------------|-------|
| Request | method, endpoint, status | user_id |
| Performance | p50, p95, p99 | per-request values |

**Cardinality Limit:** ≤100 unique values per metric

### 5.4 Infrastructure Availability

#### Jaeger Requirements
| Requirement | Value |
|-------------|-------|
| Availability | 99.9% |
| Retention | 7 days traces |
| Storage | 50GB default |

#### Seq Requirements
| Requirement | Value |
|-------------|-------|
| Availability | 99.9% |
| Retention | 30 days logs |
| Ingestion | 10,000 events/sec |

#### OpenTelemetry Collector
| Requirement | Value |
|-------------|-------|
| Deployment | Sidecar per service |
| Batch Size | 1000 spans |
| Timeout | 5 seconds |

### 5.5 Health Check Endpoints

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

### 7.2 Log Retention Policy

| Environment | Retention Period | Max Storage |
|-------------|-----------------|-------------|
| Development | 7 days | 1 GB |
| Staging | 30 days | 10 GB |
| Production | 90 days | 100 GB |

**Log Level Standards:**
| Level | Usage |
|-------|-------|
| Debug | Detailed debugging information, variable values |
| Information | General application events, user actions |
| Warning | Unexpected but handled issues |
| Error | Errors that affect single operation |
| Critical | System-level failures, data loss risk |

### 7.3 Log Volume Management

| Metric | Threshold | Action |
|--------|-----------|--------|
| Logs per second | >1000 | Enable sampling |
| Error rate | >5% | Alert on-call |
| Disk usage | >80% | Archive old logs |

### 7.4 Log Structure

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

### 7.1 Error Response Standards

```csharp
// Standard Error Response
public class ErrorResponse
{
    public string Type { get; set; }          // Error type identifier
    public string Title { get; set; }          // Error title
    public int Status { get; set; }            // HTTP status code
    public string Detail { get; set; }         // Detailed error message
    public string Instance { get; set; }       // Error instance identifier
    public Dictionary<string, string[]> Errors { get; set; }  // Validation errors
    public DateTime Timestamp { get; set; }     // Error timestamp
    public string TraceId { get; set; }        // Correlation ID
}

// Validation Error Response
public class ValidationErrorResponse : ErrorResponse
{
    public List<ValidationError> ValidationErrors { get; set; }
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
    public string Code { get; set; }
}
```

### 7.2 Pagination Standards

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | int | No | 1 | Page number (1-based) |
| pageSize | int | No | 10 | Items per page (max 100) |
| sortBy | string | No | - | Sort field name |
| sortOrder | string | No | asc | Sort direction (asc/desc) |

**Pagination Response Format:**
```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasNext": true,
  "hasPrevious": false
}
```

### 7.3 Rate Limiting Requirements

| Service | Requests/Minute | Burst | Notes |
|---------|-----------------|-------|-------|
| Gateway | 1000 | 1500 | Per IP |
| Auth API | 100 | 150 | Per user |
| Product API | 500 | 750 | Per IP |
| Cart API | 200 | 300 | Per user |
| Order API | 100 | 150 | Per user |
| Coupon API | 50 | 75 | Per user |

**Rate Limit Headers:**
- `X-RateLimit-Limit`: Maximum requests per window
- `X-RateLimit-Remaining`: Remaining requests
- `X-RateLimit-Reset`: Unix timestamp when limit resets
- `Retry-After`: Seconds to wait (when limited)

### 7.4 Timeout Standards

| Operation Type | Timeout | Notes |
|----------------|---------|-------|
| HTTP Request (Gateway) | 30s | User-facing APIs |
| Internal Service Call | 10s | Service-to-service |
| Database Query | 5s | EF Core queries |
| Cache Operation | 2s | Redis operations |
| Message Publish | 5s | RabbitMQ publish |

### 7.5 File Upload Requirements

**Upload Endpoint:**
- `POST /api/v1/uploads` - Upload file
- `GET /api/v1/uploads/{id}` - Get upload status
- `DELETE /api/v1/uploads/{id}` - Delete upload

**Constraints:**
| Parameter | Limit |
|-----------|-------|
| Max File Size | 10 MB |
| Allowed Types | .jpg, .jpeg, .png, .gif, .pdf, .doc, .docx |
| Max Files per Request | 5 |
| Storage | Azure Blob / S3 |

**Response:**
```json
{
  "id": "guid",
  "fileName": "document.pdf",
  "contentType": "application/pdf",
  "size": 1024000,
  "url": "https://storage.blob.net/uploads/guid",
  "status": "Completed",
  "uploadedAt": "2026-02-22T10:00:00Z"
}
```

### 7.6 Batch Operations

**Batch Endpoint:**
- `POST /api/v1/batch` - Execute batch operations

**Request:**
```json
{
  "operations": [
    { "method": "POST", "path": "/products", "body": {...} },
    { "method": "PUT", "path": "/products/123", "body": {...} },
    { "method": "DELETE", "path": "/products/456" }
  ]
}
```

**Constraints:**
| Parameter | Limit |
|-----------|-------|
| Max Operations | 100 per request |
| Timeout | 60 seconds |
| Atomic | No (partial success allowed) |

### 7.7 API Naming Conventions

| Resource | Plural/Singular | Example |
|----------|-----------------|---------|
| Collection | Plural | /products |
| Single | Plural + ID | /products/{id} |
| Sub-resource | Nested | /orders/{id}/items |
| Action | Verb | /products/{id}/activate |

**Query Parameters:**
- Use camelCase: `pageSize`, `sortBy`, `filterBy`
- Boolean: `isActive`, `includeDetails`

### 7.8 Content-Type Negotiation

| Accept Header | Response Format |
|--------------|----------------|
| application/json | JSON (default) |
| application/xml | XML |
| text/html | HTML (for browser) |

**Default:** application/json if no Accept header

### 7.9 API Key Authentication

| Header | Value | Purpose |
|--------|-------|---------|
| X-Api-Key | 32-char string | Server-to-server auth |
| X-Api-Secret | 64-char string | HMAC signing |

**Rate Limits for API Keys:**
| Plan | Requests/Hour | Burst |
|------|---------------|-------|
| Free | 100 | 150 |
| Basic | 1,000 | 1,500 |
| Pro | 10,000 | 15,000 |
| Enterprise | 100,000 | 150,000 |

### 7.10 Brute Force Protection

| Action | Threshold | Lockout Duration |
|--------|-----------|------------------|
| Login attempts | 5 failures | 15 minutes |
| Password reset | 3 requests | 1 hour |
| API key | 10 failures | 1 hour |

**Headers:**
- `X-RateLimit-Limit`
- `X-RateLimit-Remaining`
- `X-RateLimit-Reset`

### 7.11 CSRF Protection

| Header | Value |
|--------|-------|
| X-CSRF-Token | Required for state-changing operations |
| SameSite | Strict (cookies) |

**Exempt:** GET requests, API keys

### 7.12 Password Complexity Requirements

| Rule | Requirement |
|------|-------------|
| Minimum Length | 8 characters |
| Maximum Length | 128 characters |
| Uppercase | At least 1 |
| Lowercase | At least 1 |
| Digit | At least 1 |
| Special Character | At least 1 |
| Common Password Check | Must not be in top 10,000 |

### 7.13 Concurrent Request Handling

| Scenario | Handling |
|----------|----------|
| Same user, multiple requests | Allow (use optimistic concurrency) |
| Duplicate submissions | Idempotency key required |
| Race conditions | Use ETags / version numbers |

**Idempotency:**
- POST, PUT, DELETE must support idempotency key
- Header: `Idempotency-Key` (UUID)
- Response: `Idempempotency-Key` echoed back
- Cache responses for 24 hours

### 7.14 Long-Running Operations

| Pattern | Use Case | Timeout |
|---------|----------|---------|
| Async Response | File processing | 30 minutes |
| Webhooks | External callbacks | N/A |
| Polling | Status checks | 5 minutes |

**Async Response Pattern:**
```json
// Immediate Response (202 Accepted)
{
  "operationId": "uuid",
  "status": "Accepted",
  "_links": {
    "status": "/api/v1/operations/{id}",
    "result": "/api/v1/operations/{id}/result"
  }
}
```

### 7.15 Request Cancellation

| Mechanism | Header | Behavior |
|-----------|--------|----------|
| Client Disconnect | N/A | Server continues processing |
| Cancellation Token | X-Cancellation-Token | Graceful stop |
| Timeout | X-Request-Timeout | Auto-cancel |

**Timeout Headers:**
- `X-Request-Timeout`: Max request duration (ISO 8601 duration)
- Response: 408 Request Timeout if exceeded

### 7.16 Edge Case Handling

| Edge Case | Handling |
|-----------|----------|
| Header size > 8KB | 431 Request Header Fields Too Large |
| Invalid Unicode | Replace with U+FFFD, log warning |
| Null bytes in strings | Reject with 400 Bad Request |
| Oversized payload | 413 Payload Too Large |

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

### 9.1 Authorization (RBAC)

```csharp
// Roles Definition
public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Support = "Support";
    public const string API = "API";
}

// Permissions
public static class Permissions
{
    // Product permissions
    public const string ProductsRead = "products:read";
    public const string ProductsWrite = "products:write";
    public const string ProductsDelete = "products:delete";
    
    // Order permissions
    public const string OrdersRead = "orders:read";
    public const string OrdersWrite = "orders:write";
    
    // Cart permissions
    public const string CartRead = "cart:read";
    public const string CartWrite = "cart:write";
    
    // User management
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
}

// Role-Permission Mapping
public static class RolePermissions
{
    public static readonly Dictionary<string, string[]> Permissions = new()
    {
        [Roles.Admin] = new[] { "*" },  // All permissions
        [Roles.Support] = new[] { 
            Permissions.ProductsRead, 
            Permissions.OrdersRead,
            Permissions.UsersRead 
        },
        [Roles.Customer] = new[] { 
            Permissions.ProductsRead,
            Permissions.CartRead,
            Permissions.CartWrite,
            Permissions.OrdersRead,
            Permissions.OrdersWrite
        },
        [Roles.API] = new[] { 
            Permissions.ProductsRead,
            Permissions.OrdersRead
        }
    };
}

// Authorization Policy Example
services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageProducts", policy =>
        policy.RequireRole(Roles.Admin));
    
    options.AddPolicy("CanReadOrders", policy =>
        policy.RequireAssertion(ctx => 
            ctx.User.IsInRole(Roles.Admin) ||
            ctx.User.HasPermission(Permissions.OrdersRead)));
});
```

### 9.2 Password Security

| Requirement | Value |
|-------------|-------|
| Minimum Length | 8 characters |
| Maximum Length | 128 characters |
| Require Uppercase | Yes (at least 1) |
| Require Lowercase | Yes (at least 1) |
| Require Number | Yes (at least 1) |
| Require Special Character | Yes (at least 1) |
| Hashing Algorithm | bcrypt (work factor 12) or Argon2id |
| Maximum Login Attempts | 5 |
| Lockout Duration | 15 minutes |

### 9.3 Session Management

| Setting | Value |
|---------|-------|
| Session Timeout | 30 minutes idle |
| Absolute Session Lifetime | 24 hours |
| Sliding Expiration | Yes |
| Secure Cookie | Yes (HTTPS only) |
| HttpOnly Cookie | Yes |
| SameSite Policy | Strict |

### 9.4 Multi-Factor Authentication (MFA)

**MFA Methods:**
| Method | Code Length | Expiry | Notes |
|--------|-------------|--------|-------|
| TOTP (Authenticator) | 6 digits | 30 seconds | Time-based |
| Email OTP | 6 digits | 5 minutes | Less secure |
| SMS OTP | 6 digits | 5 minutes | Deprecated - use TOTP |

**MFA Requirements:**
- Optional for customers (enabled by default for Admin role)
- Must be enabled within 30 days for Admin users
- Backup codes: 10 single-use codes generated on enable
- Trusted devices: 30 days expiry

### 9.5 Data Encryption

**At Rest:**
| Data Type | Encryption | Key Management |
|-----------|------------|----------------|
| Database fields | AES-256 | Azure Key Vault |
| File storage | AES-256 | Azure Key Vault |
| Backups | AES-256 | Offline secure storage |
| Logs | None (PII redaction) | N/A |

**In Transit:**
| Channel | Protocol | Version |
|---------|----------|---------|
| External API | TLS | 1.3 |
| Internal Service | TLS | 1.2+ |
| Database | TLS | 1.2 |

**PII Fields (Require Encryption):**
- Passwords (hashed, not encrypted)
- Credit card numbers (tokenized)
- Social Security Numbers
- Personal addresses
- Phone numbers

### 9.6 JWT Configuration

### 9.7 Security Edge Cases

#### Concurrent Login Handling
| Scenario | Behavior |
|----------|----------|
| Same user, different devices | Allow (no limit) |
| Same user, same device | Reuse existing session |
| Token theft | Revoke all sessions option |

#### Password Change with Active Token
| Action | Behavior |
|--------|----------|
| Password changed | All tokens invalidated |
| Notification | Email sent to user |
| Force logout | All devices logged out |

#### Token Revocation
| Method | Endpoint | Use Case |
|--------|----------|----------|
| Revoke single | POST /auth/revoke | Specific token |
| Revoke all | POST /auth/revoke-all | Account security |
| Token blacklist | Redis set | Active until expiry |

**Revocation Response:**
```json
{
  "success": true,
  "revokedAt": "2026-02-22T10:00:00Z",
  "tokensRevoked": 3
}
```

#### Account Deletion with Active Sessions
| Step | Action |
|------|--------|
| 1 | Mark account as deleted |
| 2 | Revoke all tokens |
| 3 | Anonymize personal data |
| 4 | Close all sessions |
| 5 | Return 401 for future requests |

#### Clock Skew Tolerance
| Setting | Value |
|---------|-------|
| Accept Future | 60 seconds |
| Accept Past | 0 seconds |
| NTP Sync | Required |

---

### 9.8 Observability for Security

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

## 10.3 Container Orchestration Standards

**Docker Compose Best Practices:**
| Practice | Requirement |
|----------|-------------|
| Image Tag | Always use specific versions (not :latest) |
| Restart Policy | Always use `restart: unless-stopped` |
| Health Checks | Required for all application containers |
| Resource Limits | Required for all containers |
| Logging | Use json logging driver |
| Networks | Use custom bridge networks |

**Service Dependencies:**
```yaml
services:
  app:
    depends_on:
      db:
        condition: service_healthy
      redis:
        condition: service_started
```

### 10.4 Service Mesh Requirements

**When to Use Service Mesh:**
- Service-to-service mTLS encryption
- Distributed tracing enhancement
- Traffic management (canary, blue-green)
- Circuit breaker at network level

**Service Mesh Features (Future):**
| Feature | Priority | Notes |
|---------|----------|-------|
| mTLS | Medium | Istio/Linkerd |
| Traffic Splitting | Medium | Canary deployments |
| Observability | High | Automatic tracing |
| Rate Limiting | Low | L7 rate limiting |

### 10.5 Scaling Requirements

| Scenario | Configuration |
|----------|----------------|
| Horizontal Scaling | Add pod replicas (HPA) |
| Vertical Scaling | Increase CPU/memory requests |
| Database Scaling | Read replicas |
| Cache Scaling | Redis Cluster mode |

**Auto-scaling Triggers:**
| Metric | Threshold | Action |
|--------|-----------|--------|
| CPU | >70% | Scale up 1 replica |
| Memory | >80% | Scale up 1 replica |
| Request Latency | >500ms p99 | Scale up 1 replica |
| Error Rate | >5% | Alert + scale |

### 10.6 Failure Scenarios

| Scenario | Handling |
|----------|----------|
| Database unavailable | Return 503, circuit breaker |
| Redis unavailable | Fallback to in-memory, alert |
| RabbitMQ unavailable | Queue locally, retry on recover |
| Network partition | Graceful degradation |
| Service unavailable | Return 502, fallback service |

**Circuit Breaker States:**
- Closed: Normal operation
- Open: Fail fast, return cached response
- Half-Open: Test if service recovered

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

### 12.0 Kubernetes Requirements

**Required Resources per Service:**

| Resource | Request | Limit | Notes |
|----------|---------|-------|-------|
| CPU | 200m | 500m | 0.2 to 0.5 cores |
| Memory | 256Mi | 512Mi | 256MB to 512MB |
| Replicas (Production) | 3 | 10 | Min 3, HPA max 10 |
| Replicas (Staging) | 2 | 5 | Min 2, HPA max 5 |

**Pod Disruption Budget:**
```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: product-api-pdb
spec:
  minAvailable: 2
  selector:
    matchLabels:
      app: product-api
```

**Resource Quotas:**
```yaml
apiVersion: v1
kind: ResourceQuota
metadata:
  name: mango-quota
spec:
  hard:
    requests.cpu: "10"
    requests.memory: 20Gi
    limits.cpu: "20"
    limits.memory: 40Gi
    pods: "50"
    services: "20"
    secrets: "30"
    configmaps: "30"
```

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

### 15.2 Database Migrations

**Migration Standards:**

| Rule | Requirement |
|------|-------------|
| Migration Naming | `{Timestamp}_{Description}` |
| Idempotent | Must handle already-applied migrations |
| Reversible | Include DOWN script or use idempotent operations |
| Test Data | Include seed data for development |
| Large Data | Use separate data migration process |

**Migration Commands:**
```bash
# Add migration (per service)
dotnet ef migrations add InitialCreate --project src/Services/ProductAPI

# Apply migrations
dotnet ef database update --project src/Services/ProductAPI

# Remove migration (if not applied)
dotnet ef migrations remove --project src/Services/ProductAPI

# Generate SQL script (for review)
dotnet ef migrations script --project src/Services/ProductAPI -o migrate.sql

# List migrations
dotnet ef migrations list --project src/Services/ProductAPI
```

**Migration Best Practices:**
1. Never modify existing migrations - create new ones
2. Always test migrations in staging before production
3. Include rollback strategy in PR description
4. Use transactions for multi-step migrations
5. Archive old migrations after 1 year

### 15.3 Running Locally

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
