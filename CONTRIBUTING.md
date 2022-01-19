# Contributing

Thank you for investing your time in contributing to this project!

## Getting Started

### Development Requirements

- **.NET 8.0 SDK** or **.NET 10.0 SDK**
- **Git**
- A code editor with `.editorconfig` support (Visual Studio, VS Code with C# extension, Rider)

### Build Locally

```bash
# Clone the repository
git clone https://github.com/sarmkadan/signalr-map-realtime.git
cd signalr-map-realtime

# Restore dependencies
dotnet restore

# Build the project
dotnet build --configuration Release

# Build a specific project
dotnet build src/SignalRMapRealtime/SignalRMapRealtime.csproj --configuration Release
```

### Run Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run with detailed output and generate a TRX report
dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Run a specific test project
dotnet test tests/signalr-map-realtime.Tests/signalr-map-realtime.Tests.csproj

# Run integration tests only
dotnet test tests/signalr-map-realtime.IntegrationTests/signalr-map-realtime.IntegrationTests.csproj
```

### Pack NuGet (local)

```bash
dotnet pack src/SignalRMapRealtime/SignalRMapRealtime.csproj --configuration Release --output ./nupkg
```

## How to Contribute

1. **Fork the repository** and clone your fork:
   ```bash
   git clone https://github.com/<your-username>/signalr-map-realtime.git
   cd signalr-map-realtime
   ```
2. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes**, ensuring all tests pass locally.
4. **Commit with a clear message:**
   ```bash
   git commit -m "feat: describe what you added"
   ```
5. **Push and open a Pull Request** targeting the `main` branch with a clear description of the change.

## Pull Request Guidelines

- Keep PRs focused — one feature or fix per PR.
- Reference any related issues with `Closes #<issue-number>`.
- Ensure CI passes before requesting review.
- Update documentation if your change affects public behaviour or configuration.

## Code Style

The project uses `.editorconfig` for consistent formatting. Please respect these rules:

- **Indentation:** 4 spaces for C#, 2 spaces for JSON/YAML/XML.
- **Naming:** PascalCase for types and public members, camelCase for local variables and parameters.
- **XML Docs:** Required for all public types, methods, and properties.
- **`var`:** Use when the type is obvious from the right-hand side; avoid for built-in types.
- **Keep author headers:** Do not remove existing author attributions in source files.

## Reporting Issues

Use **GitHub Issues** to report bugs or request features.
- For bugs: include clear reproduction steps, expected vs. actual behaviour, and environment details.
- For features: describe the problem you are solving and why it belongs in this library.

## License

By contributing, you agree that your contributions will be licensed under the **MIT License**.
