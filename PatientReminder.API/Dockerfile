# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy everything at once
COPY . .

# Restore and publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Final stage/image using the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PatientReminder.API.dll"]