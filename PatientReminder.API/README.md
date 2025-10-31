# PatientReminder API

## Overview

**PatientReminder API** is a simple ASP.NET Core Web API designed to help manage patient appointments and send automated reminders. It uses a lightweight SQLite database, making it ideal for portfolio projects, demos, or small-scale deployments without the need for a separate database server.

## Features

- **Appointment Scheduling:**  
  Create and store patient appointments via RESTful endpoints.

- **Automated Reminders:**  
  A background service (`AppointmentReminderService`) checks for upcoming appointments and sends reminders.

- **Interactive API Documentation:**  
  Integrated Swagger UI for exploring and testing API endpoints directly from your browser.

- **SQLite Database:**  
  All data is stored in a single file (`appointments.db`), requiring no additional setup.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

### Running Locally

1. **Clone the repository:**
1. git clone https://github.com/HJBCodeForge/PatientReminder.git cd PatientReminder/PatientReminder.API

2. **Restore dependencies:**
1. dotnet restore

3. **Apply database migrations:**
1. dotnet ef database update


4. **Run the application:**
1. dotnet run


5. **Access Swagger UI:**
   Open [http://localhost:5000/swagger](http://localhost:5000/swagger) (or the port shown in your terminal) to view and interact with the API documentation.

### Running with Docker

1. **Build the Docker image:**
1. docker build -t patient-reminder-api -f PatientReminder.API/Dockerfile PatientReminder.API

2. **Run the container:**
1. docker run -p 8080:8080 patient-reminder-api


3. **Access Swagger UI:**
   Open [http://localhost:8080/swagger](http://localhost:8080/swagger)

## API Endpoints

- **POST /appointments**  
  Schedule a new appointment.

- **GET /weatherforecast**  
  Example endpoint for testing (default template).

- **Swagger UI**  
  Explore all available endpoints and models.

## How It Works

- **Appointment Scheduling:**  
  Send a POST request with appointment details to `/appointments`. The API stores the appointment and marks it as pending for reminders.

- **Automated Reminders:**  
  The `AppointmentReminderService` runs in the background, periodically checking for appointments that need reminders and sending them (implementation details in `Services/AppointmentReminderService.cs`).

## Technologies Used

- ASP.NET Core 8 Web API
- Entity Framework Core (with SQLite)
- Swagger (via Swashbuckle.AspNetCore)
- Docker (optional)

## License

This project is for educational and portfolio purposes.

---

**For questions or contributions, please open an issue or submit a pull request.**