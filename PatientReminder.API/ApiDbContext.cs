using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core DbContext for the Patient Reminder API.
/// </summary>
/// <remarks>
/// - Uses SQLite by default (see Program.cs and appsettings.json).
/// - Registered with a scoped lifetime and resolved via dependency injection.
/// </remarks>
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    /// <summary>
    /// Appointments persisted in the SQLite database.
    /// </summary>
    public DbSet<Appointment> Appointments { get; set; }
}