using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Mango.Services.ShoppingCartAPI.Application.Handlers;
using Mango.Services.ShoppingCartAPI.Domain.Interfaces;
using Mango.Services.ShoppingCartAPI.Infrastructure.Data;
using Mango.Services.ShoppingCartAPI.Infrastructure.Mappings;
using Mango.Services.ShoppingCartAPI.Infrastructure.Repositories;

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
        Title = "Cart API",
        Version = "v1",
        Description = "Shopping Cart API for Mango E-Commerce"
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CartDbContext>(options =>
    options.UseSqlServer(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AddToCartHandler).Assembly));

// Repositories
builder.Services.AddScoped<ICartRepository, CartRepository>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CartDbContext>("cart-db");

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
    var db = scope.ServiceProvider.GetRequiredService<CartDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
