# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help restore build run test clean db-update db-drop publish docker-build docker-up docker-down

help:
	@echo "SignalR Map Realtime - Build Commands"
	@echo "======================================"
	@echo ""
	@echo "Development:"
	@echo "  make restore      - Restore NuGet packages"
	@echo "  make build        - Build solution (Debug)"
	@echo "  make run          - Run application"
	@echo "  make watch        - Run with file watching"
	@echo "  make clean        - Clean build artifacts"
	@echo ""
	@echo "Testing:"
	@echo "  make test         - Run unit tests"
	@echo "  make test-watch   - Run tests with watching"
	@echo ""
	@echo "Database:"
	@echo "  make db-update    - Apply pending migrations"
	@echo "  make db-drop      - Drop and recreate database"
	@echo "  make db-migrate   - Create migration"
	@echo ""
	@echo "Production:"
	@echo "  make publish      - Publish for production"
	@echo "  make docker-build - Build Docker image"
	@echo "  make docker-up    - Start Docker containers"
	@echo "  make docker-down  - Stop Docker containers"

restore:
	dotnet restore

build:
	dotnet build

run:
	dotnet run --project src/SignalRMapRealtime

watch:
	dotnet watch run --project src/SignalRMapRealtime

clean:
	dotnet clean
	find . -type d -name "bin" -exec rm -rf {} +
	find . -type d -name "obj" -exec rm -rf {} +

test:
	dotnet test

test-watch:
	dotnet watch test

db-update:
	dotnet ef database update --project src/SignalRMapRealtime

db-drop:
	dotnet ef database drop --force --project src/SignalRMapRealtime
	dotnet ef database update --project src/SignalRMapRealtime

db-migrate:
	@read -p "Enter migration name: " name; \
	dotnet ef migrations add $$name --project src/SignalRMapRealtime

publish:
	dotnet publish -c Release src/SignalRMapRealtime

docker-build:
	docker build -t signalr-map-realtime:latest .

docker-up:
	docker-compose up -d

docker-down:
	docker-compose down

docker-logs:
	docker-compose logs -f

docker-clean:
	docker-compose down -v
	docker rmi signalr-map-realtime:latest

format:
	dotnet format

lint:
	dotnet build /p:EnforceCodeStyleInBuild=true

all: clean restore build test

.DEFAULT_GOAL := help
