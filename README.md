# Highway Speed Enforcement Pipeline

```
┌────────────────────┐  gRPC client stream  ┌──────────────────────────┐  GET /api/traffic-snapshot  ┌────────────────────┐
│ Generator (console)│ ───────────────────▶ │ Processor (ASP.NET Core, │ ◀────────────────────────── │ Client (WPF, MVVM) │
│ 100 vehicles/s     │  TelemetryMessage    │ Docker)                  │  TrafficSnapshotDto (JSON)  │ polls every 3 s    │
└────────────────────┘                      └──────────────────────────┘                             └────────────────────┘
```

> **Status: architecture skeleton.** Composition (DI, hosting, options, gRPC/REST wiring, health, metrics,
> logging, Docker) is complete and builds warning-free; the algorithmic cores are intentionally
> `NotImplementedException` with the intended logic in a TODO comment above each one. Tests that need them are
> present but skipped, so the intent is visible. See `docs/architecture.md` and `docs/adr/`.

## Run

```bash
docker compose up --build processor                   # REST :5000, gRPC :5001, health at /health/live|ready
dotnet run --project src/SpeedEnforcement.Generator    # streams to http://localhost:5001
dotnet run --project src/SpeedEnforcement.Client.Wpf   # polls http://localhost:5000
```

```bash
dotnet build SpeedEnforcement.slnx
dotnet test  SpeedEnforcement.slnx
# or, per test project (works even where the MTP `dotnet test` integration reports "Zero tests ran"):
dotnet run --project tests/SpeedEnforcement.Processor.UnitTests
```

OpenAPI (Development only): `http://localhost:5000/openapi/v1.json`.

## Solution layout

| Project | Purpose |
|---------|---------|
| `Contracts` | `telemetry.proto` (gRPC client-streaming ingest) and REST DTOs. The only shared code. |
| `Generator` | Generic Host console app. `SimulationLoop` (1 s `PeriodicTimer`, spawn → advance+emit → exit) writes to an outbox channel; `GrpcTelemetryPublisher` drains it into one long-lived stream with reconnect/backoff. |
| `Processor.Domain` | Pure domain, no dependencies: `BoundedLeaderboard<T>` (Top-N, deterministic tie-break, per-instance lock), `VehicleTrack`, `AnalyticsEngine`. |
| `Processor.Host` | ASP.NET Core: gRPC `TelemetryIngestService` → `PartitionedIngestPipeline` (channels keyed by plate) → `IngestWorker` → engine; minimal API snapshot endpoint; health checks; OpenTelemetry metrics; Dockerfile. |
| `Client.Core` | WPF-free: typed `HttpClient`, `SnapshotPoller` (3 s `PeriodicTimer`, UI marshalling via `IUiDispatcher`), `MainViewModel`. |
| `Client.Wpf` | Generic Host inside WPF, `WpfDispatcher`, `MainWindow` with global and per-camera grids. |
| `tests/*` | xunit.v3; composition, options validation, worker/poller wiring, metrics and REST shape are real tests; algorithm tests are skipped skeletons. |

## Key decisions (details in `docs/adr/`)

1. **gRPC client streaming** for ingest: one connection, ordered, backpressure from HTTP/2 flow control, contract-first `.proto`. MQTT/broker deliberately deferred until there is a second consumer.
2. **Partitioned channels + fine-grained locks**: readings are routed by `hash(plate)` so one vehicle is always processed by one consumer (ordering, lock-free tracks); leaderboards protect themselves with 11 small locks and a lock-free threshold fast path.
3. **Deterministic Top-N**: `SortedSet` with `score DESC, plate ASC` comparer; tie-break is part of the ordering.
4. **Purge semantics**: boards keep value copies, tracks are purged at camera 10, boards represent all recorded traffic.
5. **Client stream error handling**: tolerate isolated invalid messages (metric + log), abort the stream on a systematic rejection rate.
6. **Testability**: `TimeProvider` and `Random` injected everywhere; view models live outside WPF.
