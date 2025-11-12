/// <summary>
/// Background worker that periodically scans for upcoming appointments and
/// marks reminders as sent. This implementation simulates sending SMS by
/// logging to console output.
/// </summary>
/// <remarks>
/// Implementation notes:
/// - Runs every 1 minute via System.Threading.Timer.
/// - Creates a DI scope per tick to resolve scoped services (ApiDbContext).
/// - Uses UTC for time comparisons to avoid timezone drift.
/// - Safe in single-instance deployments; add coordination for multi-instance.
/// </remarks>
public class AppointmentReminderService : IHostedService, IDisposable
{
    private readonly ILogger<AppointmentReminderService> _logger;
    private Timer? _timer = null;
    private readonly IServiceProvider _serviceProvider;

    public AppointmentReminderService(ILogger<AppointmentReminderService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Starts the periodic scan. Initialization is lightweight; no blocking IO.
    /// </summary>
    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Reminder Service is starting.");
        // Fire immediately, then every 1 minute
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Periodic execution body: find upcoming appointments within 24 hours and
    /// mark reminders as sent (simulated).
    /// </summary>
    private void DoWork(object? state)
    {
        _logger.LogInformation("Checking for appointments to remind...");

        // IMPORTANT: Create a new scope to resolve scoped services like ApiDbContext.
        // The hosted service is a singleton; scoping avoids DbContext lifetime issues.
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            var now = DateTime.UtcNow;
            var reminderWindow = now.AddHours(24);

            var appointmentsToRemind = dbContext.Appointments
              .Where(a => !a.IsReminderSent && a.AppointmentTime <= reminderWindow && a.AppointmentTime > now)
              .ToList();

            foreach (var appointment in appointmentsToRemind)
            {
                // In a real app, integrate with an SMS provider (e.g., Twilio, SNS).
                _logger.LogInformation($"--- SIMULATING REMINDER for appointment {appointment.Id} to {appointment.PatientPhoneNumber} ---");
                appointment.IsReminderSent = true;
            }

            if (appointmentsToRemind.Any())
            {
                dbContext.SaveChanges();
                _logger.LogInformation($"{appointmentsToRemind.Count} reminders sent.");
            }
        }
    }

    /// <summary>
    /// Stops the periodic scan and disposes timing resources.
    /// </summary>
    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Reminder Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Frees managed resources associated with the timer.
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose();
    }
}