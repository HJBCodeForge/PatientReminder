<div align="center">

# Patient Reminder API

Lightweight .NET 8 REST API + background worker that stores patient appointments and simulates sending SMS reminders 24 hours before each appointment. Deployed as a single Docker container to AWS Elastic Beanstalk.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white) ![EF Core](https://img.shields.io/badge/EF%20Core-Sqlite-6D3B87) ![Swagger](https://img.shields.io/badge/OpenAPI-Swagger-85EA2D?logo=swagger&logoColor=white) ![Docker](https://img.shields.io/badge/Container-Docker-2496ED?logo=docker&logoColor=white) ![AWS](https://img.shields.io/badge/Deploy-AWS%20Elastic%20Beanstalk-FF9900?logo=amazonaws&logoColor=white)

</div>

## 🌐 Live API

Base URL: `http://PatientReminderApi.us-east-2.elasticbeanstalk.com`

Swagger UI (interactive docs): `http://PatientReminderApi.us-east-2.elasticbeanstalk.com/swagger`

> NOTE: Elastic Beanstalk maps container port `8080` -> `80` externally, so you do not need to specify the port in the public URL.

## ✨ Features

- Schedule appointments via a simple POST endpoint.
- Background hosted service (`AppointmentReminderService`) runs every minute:
	- Finds appointments within the next 24 hours where a reminder was not yet sent.
	- Logs a simulated reminder and flags the record (`IsReminderSent = true`).
- EF Core + SQLite file database (`appointments.db`).
- Automatic database migration / creation on startup.
- OpenAPI (Swagger) for quick testing.
- Containerized (multi-stage Dockerfile) and deployable to AWS Elastic Beanstalk via `Dockerrun.aws.json` or a published image.

## 🧱 Architecture Overview

```
Client --> HTTP (REST)
						|
				ASP.NET Core (Controllers)
						|
		EF Core DbContext (SQLite file)
						|
 Background Hosted Service (Timer every 1 min)
						|
	 Queries pending reminders, logs simulated SMS
```

Core components:

| Component | File | Responsibility |
|-----------|------|----------------|
| API Host | `Program.cs` | Service registration, DB migration/creation, endpoint mapping |
| Data Model | `Appointment.cs` | Domain entity persisted in SQLite |
| Persistence | `ApiDbContext.cs` | EF Core DbContext & DbSet | 
| REST Endpoint | `Controllers/AppointmentsController.cs` | Accepts appointment creation requests |
| Background Worker | `Services/AppointmentReminderService.cs` | Periodic reminder scan & logging |
| Container Build | `PatientReminder.API/Dockerfile` | Multi‑stage build/publish |
| AWS Deployment | `Dockerrun.aws.json` | Single container Elastic Beanstalk definition |

## 📦 Tech Stack

- .NET 8 (ASP.NET Core Minimal Hosting Model)
- Entity Framework Core (SQLite provider)
- Hosted Background Service (`IHostedService`)
- Swashbuckle / OpenAPI
- Docker (multi-stage build)
- AWS Elastic Beanstalk (Single Container Platform)

## 🔌 Endpoints

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/appointments` | Create / schedule an appointment | None |
| GET | `/weatherforecast` | Sample scaffold endpoint (demo only) | None |
| GET | `/swagger` | OpenAPI UI | None |

### POST /appointments

Request body (JSON):

```json
{
	"patientPhoneNumber": "+15551234567",
	"appointmentTime": "2025-11-15T14:30:00Z"
}
```

Response `201 Created`:
```json
{
	"id": 1,
	"patientPhoneNumber": "+15551234567",
	"appointmentTime": "2025-11-15T14:30:00Z",
	"isReminderSent": false
}
```

Validation / Notes:
- Times should be provided in UTC (`Z`).
- `isReminderSent` is server-managed.
- Currently there is no GET/list endpoint for appointments (see Roadmap).

## 🕒 Reminder Logic

Every minute the hosted service:
1. Creates a new DI scope.
2. Queries appointments where:
	 - `IsReminderSent == false`
	 - `AppointmentTime` within next 24 hours AND in the future.
3. Logs a simulated reminder per match.
4. Sets `IsReminderSent = true` then saves changes.

This design avoids holding a long-lived DbContext and is safe for future scoped services (e.g., SMS gateway abstraction).

## 🚀 Quick Start (Local Development)

### Prerequisites
- .NET 8 SDK
- (Optional) Docker Desktop

### Run Without Docker

```powershell
cd PatientReminder.API
dotnet restore
dotnet ef database update  # applies existing migrations if any
dotnet run --project PatientReminder.API.csproj
```

Open: `http://localhost:5020/swagger`

### Run With Docker

```powershell
cd PatientReminder.API
docker build -t patient-reminder:local .
docker run --rm -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development patient-reminder:local
```

Open: `http://localhost:8080/swagger`

> Data persistence: the SQLite file lives inside the container. To persist between runs, mount a volume:
```powershell
docker run --rm -p 8080:8080 -v ${PWD}/data:/app patient-reminder:local
```

## 🗄️ Database & Migrations

SQLite file: `appointments.db` (created at startup if absent).

Add a new migration (example):
```powershell
dotnet ef migrations add AddNotesField --project PatientReminder.API
dotnet ef database update
```

If using Docker, either:
- Run migrations in a build/publish phase, or
- Add an entrypoint script that calls `dotnet ef database update` (future enhancement).

## 🧪 Testing & Verification

### 1. Swagger UI
Use the form under POST /appointments to create an appointment whose time is ~23 hours in the future. Observe logs printing a simulated reminder within the next minute.

### 2. PowerShell (Invoke-RestMethod)
```powershell
$base = "http://localhost:8080"  # or live base URL
Invoke-RestMethod -Method Post -Uri "$base/appointments" -Body (@{ patientPhoneNumber = "+15551230000"; appointmentTime = (Get-Date).ToUniversalTime().AddHours(23).ToString("o") } | ConvertTo-Json) -ContentType 'application/json'
```
Check application console logs for: `SIMULATING REMINDER`.

### 3. cURL
```bash
curl -X POST $base/appointments \
	-H "Content-Type: application/json" \
	-d '{"patientPhoneNumber":"+15551230000","appointmentTime":"2025-11-15T14:00:00Z"}'
```

### 4. Log Assertions (Future)
Consider adding xUnit tests with an in-memory database and a fake logger to assert reminder transitions.

## 🐳 AWS Elastic Beanstalk Deployment

Deployment uses `Dockerrun.aws.json` (v1) referencing a public image: `henninghjbcodeforge/patient-reminder:v1.1.3`.

### Update Flow
1. Increment image tag (e.g., `v1.1.4`).
2. Build & push:
	 ```powershell
	 docker build -t henninghjbcodeforge/patient-reminder:v1.1.4 PatientReminder.API
	 docker push henninghjbcodeforge/patient-reminder:v1.1.4
	 ```
3. Edit `Dockerrun.aws.json` tag.
4. Upload updated `Dockerrun.aws.json` through EB console or via CLI (`eb deploy`).

### Health / Logs
- EB will surface container stdout/stderr. Ensure reminder logs appear.
- Scale considerations: running multiple instances could send duplicate reminders. (See Roadmap for mitigation strategies.)

## 🔐 Environment Variables

Current configuration is minimal:

| Variable | Purpose | Default |
|----------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment mode | Development (local) |
| `ConnectionStrings__DefaultConnection` | Override SQLite path / connection | `Data Source=appointments.db` |

Example override (PowerShell):
```powershell
$env:ConnectionStrings__DefaultConnection = "Data Source=data/appointments.db"; dotnet run
```

## 🧭 Roadmap

- [ ] Add GET /appointments (list & filter upcoming)
- [ ] Add GET /appointments/{id}
- [ ] Add soft deletion / cancellation
- [ ] External SMS gateway integration (Twilio / SNS)
- [ ] Idempotency & duplicate reminder prevention in multi-instance scaling
- [ ] Replace polling with scheduled queue (e.g., Hangfire, Quartz, or AWS EventBridge) for efficiency
- [ ] Health check endpoint (`/healthz`) & EB enhanced health integration
- [ ] Structured logging + OpenTelemetry tracing
- [ ] Container multi-stage build optimization (trim, distroless)

## 🛡️ Scaling & Concurrency Notes

- Current approach is safe for single instance.
- Multiple instances could race and send multiple reminders. Mitigations:
	- Row-level locking and status update inside a transaction.
	- Add a `ReminderSentAt` timestamp column.
	- Centralized task queue.

## 🧹 Code Quality Suggestions (Future Enhancements)

- Introduce a repository layer or CQRS if complexity grows.
- DTOs & FluentValidation for request validation.
- Replace `Timer` with `PeriodicTimer` (async friendly) in .NET 8.
- Integration tests using `WebApplicationFactory` + `Testcontainers` for greater fidelity.

## ❓ Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| No reminders logged | Appointment time not within 24h window | Use time within next 24h but >= now |
| 500 errors on startup in Docker | DB migration race / file permissions | Ensure container user has write permission; consider `EnsureCreated` fallback (already implemented) |
| Data lost between container runs | Ephemeral container FS | Mount a host volume for `/app` or only the DB file |
| Swagger not accessible publicly | Network / EB environment not healthy | Check EB logs & security group rules |

## 🤝 Contributing

1. Fork repo
2. Create feature branch: `git checkout -b feature/xyz`
3. Commit changes: `git commit -m "Add xyz"`
4. Push branch & open PR

Please include tests for logic-heavy changes.

## 📄 License

No license file currently provided. Consider adding `LICENSE` (MIT, Apache 2.0, etc.) to clarify usage rights.

## 🙌 Acknowledgements

Built with the .NET 8 minimal hosting model and common production patterns (dependency injection, hosted services, EF Core migrations).

---

Feel free to open issues for questions or enhancement ideas. Happy building!

