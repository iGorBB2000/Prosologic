# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Prosologic.Core/Prosologic.Core.csproj", "Prosologic.Core/"]
COPY ["Prosologic.Runtime/Prosologic.Runtime.csproj", "Prosologic.Runtime/"]

# Restore dependencies
RUN dotnet restore "Prosologic.Runtime/Prosologic.Runtime.csproj"

# Copy source code
COPY Prosologic.Core/ Prosologic.Core/
COPY Prosologic.Runtime/ Prosologic.Runtime/

# Build and publish
WORKDIR /src/Prosologic.Runtime
RUN dotnet publish "Prosologic.Runtime.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install dependencies for MQTT/networking
RUN apt-get update && apt-get install -y \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create directory for project files
RUN mkdir -p /app/project

# Set environment variables
ENV PROJECT_PATH=/app/project
ENV DOTNET_ENVIRONMENT=Production

# Expose default OPC UA port (if needed later)
EXPOSE 4840

# Run the application
ENTRYPOINT ["dotnet", "Prosologic.Runtime.dll"]