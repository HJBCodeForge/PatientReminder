using Microsoft.EntityFrameworkCore;

// -----------------------------------------------------------------------------
// Application bootstrap (top-level statements)
// -----------------------------------------------------------------------------
// Responsibilities:
// 1) Register services (Controllers, EF Core DbContext, Hosted Services, Swagger)
// 2) Apply database schema on startup (Migrate if migrations exist; otherwise EnsureCreated)
// 3) Configure middleware (Swagger UI, HTTPS redirection when not in container)
// 4) Map endpoints and start the web host
//
// Notes for maintainers:
// - EF Core SQLite database file path is controlled by ConnectionStrings:DefaultConnection.
// - The hosted service (AppointmentReminderService) depends on proper DI scoping; see its file for details.
// - When running inside Docker, HTTPS redirection is disabled to simplify container networking.
// -----------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// 1) Service registration ------------------------------------------------------
// MVC Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Enables endpoint discovery for Swagger
builder.Services.AddSwaggerGen();           // Registers Swagger generator

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Background worker that periodically sends simulated reminders
builder.Services.AddHostedService<AppointmentReminderService>();
// builder.WebHost.UseUrls("http://*:8080"); // Example: expose HTTP on all interfaces

var app = builder.Build();

// 2) Apply database schema before the app starts ------------------------------
// This ensures the hosted service and controllers can access a ready database.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

    // If you have migrations, this will apply them.
    // If you don't, fall back to creating the schema from the model.
    if (db.Database.GetMigrations().Any())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

// 3) Middleware pipeline ------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI();
// Swagger (enable in Development; move outside if you want it in Docker/Production)
if (app.Environment.IsDevelopment())
{

}

// Only redirect to HTTPS when not running in a container
var isInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!isInContainer)
{
    app.UseHttpsRedirection();
}

// 4) Route mapping ------------------------------------------------------------
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Example minimal API endpoint scaffolded by template
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

// Records and DTOs ------------------------------------------------------------
// WeatherForecast is used only by the demo endpoint above.
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}