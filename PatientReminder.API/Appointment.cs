/// <summary>
/// Represents a scheduled patient appointment.
/// </summary>
public class Appointment
{
    /// <summary>
    /// Database-generated identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// E.164 formatted phone number (e.g., +15551234567).
    /// </summary>
    public string? PatientPhoneNumber { get; set; }

    /// <summary>
    /// Appointment start time in UTC.
    /// </summary>
    public DateTime AppointmentTime { get; set; }

    /// <summary>
    /// Flag set by the reminder service once a reminder has been sent (simulated).
    /// </summary>
    public bool IsReminderSent { get; set; } = false;
}
