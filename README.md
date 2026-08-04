# Engineering Intelligence Platform

Engineering Intelligence Platform (EIP) is an extensible platform that
transforms engineering knowledge into actionable intelligence.

## Current Status

🚧 Early Design Phase

## Documentation

- [Product Vision](docs/vision/)
- [Architecture](docs/architecture/)

Implementation has not started.

## Development

VS-001 uses .NET 9 for its pilot implementation. This selects the runtime for
the pilot, not for EIP as a whole.

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet format --verify-no-changes --no-restore
```

The solution contains one executable (`Eip.Cli`) and one test project
(`Eip.Tests`). Logical boundaries will begin as namespaces and folders inside
the executable. No VS-001 behavior has been implemented yet.
