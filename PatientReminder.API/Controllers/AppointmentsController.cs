using Microsoft.AspNetCore.Mvc;

// These attributes configure the class as an API controller.
[ApiController]
[Route("[controller]")]

public class AppointmentsController : ControllerBase
{
        private readonly ApiDbContext _context;
        // The constructor accepts an ApiDbContext instance.
        // This is an example of DEPENDENCY INJECTION, a key concept Phreesia asks about.[1]
        // The.NET runtime provides this instance automatically because you registered it in Program.cs.
        public AppointmentsController(ApiDbContext context)
    {
        _context = context;
    }

    // This attribute marks this method to handle HTTP POST requests.
    [HttpPost]
    public async Task<IActionResult> ScheduleAppointment(Appointment appointment)
    {
        // Basic validation to ensure the request body was not empty.
        if (appointment == null)
        {
            return BadRequest("Appointment data is required.");
        }

        // Set default values before saving to the database.
        appointment.IsReminderSent = false;

        // Add the new appointment to the DbContext and save changes asynchronously.
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Return a 201 Created response. This is a RESTful best practice.
        // It tells the client the request was successful and where the new resource is located.
        return CreatedAtAction(nameof(ScheduleAppointment), new { id = appointment.Id }, appointment);
    }
}