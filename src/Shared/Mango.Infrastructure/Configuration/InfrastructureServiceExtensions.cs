using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Mango.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring infrastructure services
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Add Serilog structured logging
    /// </summary>
    public static IHostApplicationBuilder AddSerilogLogging(this IHostApplicationBuilder builder)
    {
        var loggingSection = builder.Configuration.GetSection("Logging");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .WriteTo.Console(outputTemplate: 
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://seq:5341")
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(dispose: true);

        return builder;
    }

    /// <summary>
    /// Add health checks with common dependencies
    /// </summary>
    public static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<DbContext>("database", tags: new[] { "db", "sql" })
            .AddRedis(configuration["Redis:Host"] ?? "redis", name: "redis", tags: new[] { "cache" })
            .AddRabbitMQ(new Uri($"amqp://{configuration["RabbitMQ:User"]}:{configuration["RabbitMQ:Password"]}@{configuration["RabbitMQ:Host"]}"), name: "rabbitmq", tags: new[] { "mq" });

        return services;
    }

    /// <summary>
    /// Add OpenTelemetry observability
    /// </summary>
    public static IServiceCollection AddInfrastructureOpenTelemetry(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation()
                .AddSource("MassTransit")
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://jaeger:4317");
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        return services;
    }

    /// <summary>
    /// Add Swagger with JWT authentication support
    /// </summary>
    public static IServiceCollection AddInfrastructureSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = configuration["Application:Name"] ?? "Mango API",
                Version = "v1",
                Description = "E-Commerce Microservices API",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Mango Team",
                    Email = "api@mango.com"
                }
            });
            
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Add API versioning
    /// </summary>
    public static IServiceCollection AddInfrastructureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = Microsoft.AspNetCore.Mvc.Versioning.ApiVersionReader.Combine(
                new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader(),
                new Microsoft.AspNetCore.Mvc.Versioning.HeaderApiVersionReader("X-Api-Version"));
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Map health check endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapInfrastructureHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthCheckOptions
        {
            ResponseWriter = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status200OK,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        endpoints.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthCheckOptions
        {
            ResponseWriter = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResponseWriter.WriteHealthCheckUIResponse,
            ResultStatusCodes =
            {
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status200OK,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        return endpoints;
    }
}

// Placeholder for DbContext reference
public class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbContext() { }
    public DbContext(Microsoft.EntityFrameworkCore.DbContextOptions<DbContext> options) : base(options) { }
}
