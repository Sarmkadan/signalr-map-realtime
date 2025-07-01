# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10 AS builder
WORKDIR /src

COPY ["signalr-map-realtime.sln", "."]
COPY ["src/SignalRMapRealtime/SignalRMapRealtime.csproj", "src/SignalRMapRealtime/"]

RUN dotnet restore "signalr-map-realtime.sln"

COPY . .

RUN dotnet build "signalr-map-realtime.sln" -c Release -o /app/build

RUN dotnet publish "src/SignalRMapRealtime/SignalRMapRealtime.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10

WORKDIR /app

COPY --from=builder /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SignalRMapRealtime.dll"]
