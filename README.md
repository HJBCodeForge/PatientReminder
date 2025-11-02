# PatientReminder API

A lightweight ASP.NET Core Web API for scheduling patient appointments and sending automated reminders. It uses SQLite for persistence (single file DB), runs well in Docker, and can be deployed to AWS Elastic Beanstalk.

Live (AWS Elastic Beanstalk)
- Base URL: http://PatientReminderApi.us-east-2.elasticbeanstalk.com
- Note: Swagger UI is enabled by default only in Development. See the Swagger section below if you want it enabled in cloud environments.

## Features
- Appointment scheduling via REST endpoint
- Background reminder service (`AppointmentReminderService`) checks upcoming appointments and logs reminder attempts
- SQLite database (`appointments.db`) — no server installation required
- Dockerized for easy local runs and deployments

## Tech Stack
- .NET8 (ASP.NET Core Web API)
- Entity Framework Core8 (SQLite)
- Swagger/OpenAPI (via `Swashbuckle.AspNetCore`)
- Docker
- AWS Elastic Beanstalk (single-container Docker)

## Project Structure (key parts)
- `PatientReminder.API/Program.cs` — app bootstrap, DI, middleware, DB init
- `PatientReminder.API/Controllers/` — API endpoints (for example `AppointmentsController`)
- `PatientReminder.API/appsettings.json` — configuration (connection strings, logging)
- `PatientReminder.API/Dockerfile` — container image build
- `Dockerrun.aws.json` — EB single-container deployment definition

## Getting Started (Local)

Prerequisites
- .NET8 SDK
- Optional: Docker Desktop (to run in containers)

Clone and run
1) Clone
```
git clone https://github.com/HJBCodeForge/PatientReminder.git
cd PatientReminder/PatientReminder.API
```
2) Restore
```
dotnet restore
```
3) Initialize database
- Option A (migrations, recommended if you have migrations added):
```
dotnet ef database update
```
- Option B (no migrations): the app will create the schema automatically at startup (via `Migrate()` when migrations exist, otherwise `EnsureCreated()`).

4) Run
```
dotnet run
```
5) Open the app
- Check the console for the listening URLs (e.g., `http://localhost:5xxx`).
- Example test endpoint: `GET /weatherforecast`.

Swagger UI (local)
- Swagger is configured and enabled by default in Development.
- When running locally, navigate to `/swagger` (e.g., `http://localhost:5xxx/swagger`).

## Running with Docker

Build image (from repo root)
```
docker build -t patient-reminder-api -f PatientReminder.API/Dockerfile PatientReminder.API
```
Run container
```
docker run -p8080:8080 patient-reminder-api
```
Open
- API: http://localhost:8080
- Example: http://localhost:8080/weatherforecast
- Swagger UI (if enabled in production): http://localhost:8080/swagger

Notes
- The container listens on port8080.
- HTTPS redirect is disabled when running in a container to avoid “Failed to determine the https port” warnings.

## API Usage

Schedule an appointment
- Route: `POST /Appointments`
- Body (JSON):
```
{
 "patientPhoneNumber": "+15555551234",
 "appointmentTime": "2025-01-01T14:30:00Z"
}
```
- cURL example:
```
curl -X POST http://localhost:8080/Appointments \
 -H "Content-Type: application/json" \
 -d '{
 "patientPhoneNumber": "+15555551234",
 "appointmentTime": "2025-01-01T14:30:00Z"
 }'
```
- Response: `201 Created` with the created resource.

Health/sample endpoint
- `GET /weatherforecast`

### Entities (simplified)
- `Appointment` — `Id`, `PatientPhoneNumber`, `AppointmentTime`, `IsReminderSent`

### Background reminders
- `AppointmentReminderService` runs periodically, finds pending appointments within a time window, and logs reminder attempts. You can integrate SMS/Email providers in this service.

## Configuration

Connection string
- Default is in `PatientReminder.API/appsettings.json`:
```
"ConnectionStrings": {
 "DefaultConnection": "Data Source=appointments.db"
}
```
- Override with environment variable:
 - Windows/PowerShell: `$env:ConnectionStrings__DefaultConnection="Data Source=appointments.db"`
 - Docker/EB: set `ConnectionStrings__DefaultConnection` in environment config if you need a different path.

Ports
- The app listens on `8080` in containers. Update `Dockerrun.aws.json` or `-p` mapping accordingly.

## Deployment (AWS Elastic Beanstalk)

- Image: `henninghjbcodeforge/patient-reminder:v1.1.3`
- `Dockerrun.aws.json` (v1) maps container port `8080`.
- Public URL: http://PatientReminderApi.us-east-2.elasticbeanstalk.com

Tips
- Swagger UI is disabled by default in Production. To enable it in EB for demo purposes, move `app.UseSwagger()` and `app.UseSwaggerUI()` outside the development check in `Program.cs`, or set `ASPNETCORE_ENVIRONMENT=Development` in EB (for demo only).
- Consider setting a health check URL in EB (e.g., `/weatherforecast`).

## Troubleshooting

- Connection refused on localhost
 - Ensure you run with port mapping: `docker run -p8080:8080 patient-reminder-api`
 - Verify published ports with `docker ps`.

- “Failed to determine the https port for redirect”
 - This is avoided in container runs by skipping HTTPS redirection when `DOTNET_RUNNING_IN_CONTAINER=true`.

- “no such table: Appointments”
 - Ensure the DB schema is applied. At startup the app runs `Migrate()` if migrations exist, else `EnsureCreated()`. If switching from `EnsureCreated()` to migrations, delete the existing SQLite file to avoid conflicts.

## Roadmap / Ideas
- Add real SMS/email integration for reminders
- Add authentication/authorization
- Add validation and richer appointment workflow
- CI/CD pipeline for automated builds and deployments

## Contributing
Issues and PRs are welcome. Please open an issue to discuss major changes.

## License
This project is provided for educational and portfolio purposes.
