using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Mango.Services.RewardAPI.Application.Handlers;
using Mango.Services.RewardAPI.Domain.Interfaces;
using Mango.Services.RewardAPI.Infrastructure.Data;
using Mango.Services.RewardAPI.Infrastructure.Mappings;
using Mango.Services.RewardAPI.Infrastructure.Repositories;

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
        Title = "Reward API",
        Version = "v1",
        Description = "Reward API for Mango E-Commerce"
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RewardDbContext>(options =>
    options.UseSqlServer(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EarnPointsHandler).Assembly));

// Repositories
builder.Services.AddScoped<IUserRewardRepository, UserRewardRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<RewardDbContext>("reward-db");

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
    var db = scope.ServiceProvider.GetRequiredService<RewardDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
