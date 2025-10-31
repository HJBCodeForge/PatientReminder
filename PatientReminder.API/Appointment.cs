public class Appointment
{
    public int Id { get; set; }
    public string? PatientPhoneNumber { get; set; }
    public DateTime AppointmentTime { get; set; }
    public bool IsReminderSent { get; set; } = false;
}
