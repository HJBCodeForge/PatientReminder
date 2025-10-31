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

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Reminder Service is starting.");
        // Set the timer to call DoWork every minute.
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Checking for appointments to remind...");

        // IMPORTANT: You must create a new scope to resolve scoped services
        // like ApiDbContext. The background service itself is a singleton,
        // but DbContext is scoped to a specific request. This pattern prevents issues.
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
                // In a real app, you would call an SMS service here.
                // For this project, we just log to the console.
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

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Reminder Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}