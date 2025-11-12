# -----------------------------------------------------------------------------
# Dockerfile (multi-stage)
# -----------------------------------------------------------------------------
# Stage 1: Build & publish application using full SDK image.
# Stage 2: Copy published artifacts into a smaller ASP.NET runtime image.
#
# Notes:
# - Ensure that the final image exposes the same port mapped in Dockerrun.aws.json (8080).
# - For faster builds, consider copying only csproj & restore before copying src (layer caching).
# - Add a healthcheck or non-root user in future hardening steps.
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project and restore dependencies first (improves layer cache on source changes)
COPY PatientReminder.API/*.csproj ./PatientReminder.API/
RUN dotnet restore PatientReminder.API/PatientReminder.API.csproj

# Copy the remaining source
COPY . .

# Publish (self-contained trimming could be explored later)
RUN dotnet publish PatientReminder.API/PatientReminder.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PatientReminder.API.dll"]