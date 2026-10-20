# Contributing to SignalR Map Realtime

Thank you for your interest in contributing to this project.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A C#-capable editor (Visual Studio, Rider, VS Code with C# Dev Kit)

## Building

```bash
dotnet build signalr-map-realtime.sln
```

## Running Tests

```bash
dotnet test signalr-map-realtime.sln
```

All tests must pass before submitting a pull request.

## Coding Conventions

- Follow standard C# naming conventions (PascalCase for public members, camelCase with underscore prefix for private fields)
- Use 4-space indentation (no tabs)
- Nullable reference types are enabled (`#nullable enable`) - avoid suppressing warnings without justification
- Follow the `.editorconfig` in the repository root
- Add XML documentation comments (`<summary>`) to all public types and members
- Keep methods focused and reasonably short

## Branching and Pull Requests

1. Branch from `main` for all changes
2. Use descriptive branch names: `feature/add-geofence-alerts`, `fix/location-validation`, `docs/update-readme`
3. Keep PRs small and focused - one feature or fix per PR
4. Write a clear PR description explaining what changed and why
5. CI must pass before merge
6. Commit messages should follow [Conventional Commits](https://www.conventionalcommits.org/) style (e.g. `feat: add stale location detection`, `fix: correct bounding box check`, `docs: update API reference`)

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
