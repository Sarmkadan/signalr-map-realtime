// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SignalRMapRealtime.Configuration;
using SignalRMapRealtime.Data;
using SignalRMapRealtime.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var environment = builder.Environment;
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Services registration
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalRServices();
builder.Services.AddSwaggerDocumentation();

// Controllers and routing
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS configuration
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

// Build the application
var app = builder.Build();

// Database initialization and migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        app.Logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        app.Logger.LogInformation("Database migrations completed successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError($"Error applying migrations: {ex.Message}");
    }
}

// Middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalR Map Realtime API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");

// Authentication and authorization (placeholder for future implementation)
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// SignalR hub mapping
app.MapHub<LocationHub>("/hubs/location");

// Health check endpoint
app.MapGet("/health", async () =>
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var healthy = await dbContext.Database.CanConnectAsync();
    return Results.Ok(new { status = healthy ? "healthy" : "unhealthy", timestamp = DateTime.UtcNow });
}).WithName("Health").WithOpenApi();

// API info endpoint
app.MapGet("/api/info", () =>
{
    return Results.Ok(new
    {
        title = "SignalR Map Realtime API",
        version = "1.0.0",
        description = "Real-time location tracking and mapping API using SignalR and Leaflet",
        author = "Vladyslav Zaiets",
        repository = "https://github.com/vladyslavzaiets/signalr-map-realtime"
    });
}).WithName("ApiInfo").WithOpenApi();

var port = app.Configuration["PORT"] ?? "5000";
app.Logger.LogInformation($"Application starting on port {port}...");

app.Run($"http://0.0.0.0:{port}");
