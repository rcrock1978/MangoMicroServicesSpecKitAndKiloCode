using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .WriteTo.Console(outputTemplate: 
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();

        builder.Logging.AddSerilog(Log.Logger, dispose: true);
        
        return builder;
    }

    /// <summary>
    /// Add infrastructure services
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add caching
        services.AddDistributedMemoryCache();
        
        // Add response caching
        services.AddResponseCaching();
        
        return services;
    }

    /// <summary>
    /// Use infrastructure middleware
    /// </summary>
    public static IApplicationBuilder UseInfrastructureMiddleware(this IApplicationBuilder app)
    {
        app.UseResponseCaching();
        return app;
    }
}
