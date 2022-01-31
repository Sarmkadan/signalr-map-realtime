# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =====================================================================

# Build stage using Alpine-based SDK image for minimal size
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
WORKDIR /src

# Copy project files for dependency restoration
COPY ["signalr-map-realtime.sln", "."]
COPY ["src/SignalRMapRealtime/SignalRMapRealtime.csproj", "src/SignalRMapRealtime/"]

# Restore dependencies before copying source to leverage Docker layer caching
RUN dotnet restore "src/SignalRMapRealtime/SignalRMapRealtime.csproj"

# Copy remaining source code
COPY . .

# Build and publish the application
RUN dotnet build "src/SignalRMapRealtime/SignalRMapRealtime.csproj" -c Release -o /app/build
RUN dotnet publish "src/SignalRMapRealtime/SignalRMapRealtime.csproj" \
    -c Release -o /app/publish \
    --no-restore

# Runtime stage using Alpine-based ASP.NET image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Create non-root user for security
RUN adduser -D -u 1001 appuser
WORKDIR /app

# Copy published application from builder stage
COPY --from=builder /app/publish .

# Set permissions for non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Expose port and configure environment
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check configuration
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "SignalRMapRealtime.dll"]
