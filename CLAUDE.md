# CLAUDE.md – context for AI agents working in this repository

## What this is
Prototype of a real-time **Highway Speed Enforcement Pipeline** (coding assignment). Three decoupled apps:

| App | Project | Role |
|-----|---------|------|
| Generator | `src/SpeedEnforcement.Generator` (console, Generic Host) | simulates 100 new vehicles/s across 10 cameras, streams telemetry over gRPC |
| Processor | `src/SpeedEnforcement.Processor.Host` (ASP.NET Core, Docker) + `Processor.Domain` | ingests the stream, keeps 10 per-camera Top-10 + 1 global Top-10, serves `GET /api/traffic-snapshot` |
| Client | `src/SpeedEnforcement.Client.Wpf` + `Client.Core` (WPF, MVVM) | polls the REST endpoint every 3 s and shows both leaderboards |

Shared contracts (`.proto` + REST DTOs) live in `src/SpeedEnforcement.Contracts`.

## Read before changing anything
- `docs/architecture.md` – overview and data flow.
- `docs/adr/` – one file per decision (transport, concurrency model, leaderboard structure, purge semantics, ...). **If you need to deviate from an ADR, write a new ADR, do not silently change the code.**

## Commands
```bash
dotnet build SpeedEnforcement.slnx          # warnings are errors
dotnet test  SpeedEnforcement.slnx          # Microsoft.Testing.Platform runner (see global.json)
# If `dotnet test` reports "Zero tests ran" (seen on some machines with the MTP runner), run a test app directly:
dotnet run --project tests/SpeedEnforcement.Processor.UnitTests
docker compose up --build processor         # processor on :5000 (REST) and :5001 (gRPC h2c)
dotnet run --project src/SpeedEnforcement.Generator
dotnet run --project src/SpeedEnforcement.Client.Wpf
```

## Conventions (match the existing code)
- .NET 10, C# latest, `Nullable` + `ImplicitUsings` on, `TreatWarningsAsErrors` on, central package versions in `Directory.Packages.props`.
- Composition lives in one `XxxBuilder.ConfigureXxx(...)` extension per app; `Program.cs` stays a one-liner.
- Options: POCO in `Options/`, bound from a config section, validated with `.Validate(...).ValidateOnStart()`. Env override = `Section__Key`.
- Time and randomness are injected (`TimeProvider`, `Random`); tests use `FakeTimeProvider` and seeds. Never `DateTime.UtcNow` or `Random.Shared` inline.
- Logging via source-generated `[LoggerMessage]` in `Logs/XxxLog.cs`, event ids 1xxx/2xxx per subsystem. No `Console.WriteLine`.
- Metrics via a small `IXxxMetrics` facade over `System.Diagnostics.Metrics`; names are `noun.verb`, tags low-cardinality.
- Domain (`Processor.Domain`) has **no package references**. Keep it that way.
- Tests: xunit.v3 + Moq + `FakeLogger` + `FakeTimeProvider` + `MetricCollector`. Builders/fakes go to `tests/SpeedEnforcement.Tests.Common`. Skeleton tests are `[Fact(Skip = "...")]` with the intended name; keep the name, fill the body.
- `throw new NotImplementedException()` marks intentionally unimplemented skeleton code; the TODO comment above it is the spec.

## Working style
- Small, reviewable commits. One ADR or one component per PR.
- Prefer implementing the test that already exists (skipped) over inventing a new one.
- Do not add packages without a line in `Directory.Packages.props` and a reason in the PR.
