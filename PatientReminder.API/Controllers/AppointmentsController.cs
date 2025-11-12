using Microsoft.AspNetCore.Mvc;

// -----------------------------------------------------------------------------
// AppointmentsController
// -----------------------------------------------------------------------------
// Purpose: Exposes endpoints to manage appointments.
// Currently implemented: POST /appointments to create a new appointment.
// Future extensions: GET (list), GET by id, DELETE/cancel, PATCH (reschedule).
//
// Design notes:
// - Uses constructor injection for ApiDbContext (scoped lifetime) provided by DI.
// - Returns 201 Created with resource representation for REST compliance.
// - Validation is minimal; extend with FluentValidation / data annotations as needed.
// -----------------------------------------------------------------------------
[ApiController]
[Route("[controller]")] // Resolves to /appointments due to controller name
public class AppointmentsController : ControllerBase
{
    private readonly ApiDbContext _context;

    public AppointmentsController(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Schedules a new appointment.
    /// </summary>
    /// <param name="appointment">Incoming appointment payload (JSON).</param>
    /// <returns>HTTP 201 with created appointment or 400 on validation failure.</returns>
    /// <remarks>
    /// Example request:
    /// {
    ///   "patientPhoneNumber": "+15551234567",
    ///   "appointmentTime": "2025-11-15T14:30:00Z"
    /// }
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> ScheduleAppointment(Appointment appointment)
    {
        // Guard: model binder produced null (e.g., empty body)
        if (appointment == null)
        {
            return BadRequest("Appointment data is required.");
        }

        // Defensive initialization (hosted reminder service expects false until sent)
        appointment.IsReminderSent = false;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Returns location header (self since no GET by id yet). Consider adding GET route for richer REST navigation.
        return CreatedAtAction(nameof(ScheduleAppointment), new { id = appointment.Id }, appointment);
    }
}