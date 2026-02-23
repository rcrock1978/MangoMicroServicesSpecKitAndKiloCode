using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Mango.Services.CouponAPI.Application.Handlers;
using Mango.Services.CouponAPI.Domain.Interfaces;
using Mango.Services.CouponAPI.Infrastructure.Data;
using Mango.Services.CouponAPI.Infrastructure.Mappings;
using Mango.Services.CouponAPI.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Coupon API",
        Version = "v1",
        Description = "Coupon API for Mango E-Commerce"
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CouponDbContext>(options =>
    options.UseSqlServer(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCouponHandler).Assembly));

// Repositories
builder.Services.AddScoped<ICouponRepository, CouponRepository>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CouponDbContext>("coupon-db");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CouponDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
