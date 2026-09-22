# Architecture

## Data flow

### Generator tick (every 1 s, `SimulationLoop.TickAsync`)
1. **Influx** – `VehicleRegistry.Spawn(100)` with unique plates (run prefix + sequence), counter 0.
2. **Progression & emission** – for every active vehicle: counter++, speed from `ISpeedModel`, one `TelemetryMessage` into the outbox if counter ≤ 10.
3. **Exit** – `RemoveExited(10)` drops vehicles with counter > 10.

Invariants: 1 000 active vehicles in steady state; every vehicle emits exactly 10 messages, camera ids 1..10 in order.

### Processor
```
gRPC TelemetryIngestService.Ingest(stream)
  └─ TelemetryMapper.TryMap  → reject: metric + log + IIngestRejectionPolicy (abort on systematic errors)
  └─ IIngestPipeline.EnqueueAsync → channels[hash(plate) % N]   (bounded, Wait = backpressure)
IngestWorker consumer i
  └─ AnalyticsEngine.Apply(reading)
       ├─ tracks.GetOrAdd(plate).Record(speed)                 single writer per plate → no lock
       ├─ cameraBoards[cameraId].Upsert(plate, speed)          lock per camera, threshold fast path
       ├─ if track.CamerasPassed ≥ 3: globalBoard.Upsert(...)  lock global
       └─ if cameraId == 10: tracks.TryRemove(plate)           purge
GET /api/traffic-snapshot
  └─ engine.Snapshot() → each board copied under its own lock → SnapshotDtoMapper → JSON
```

### Client
`SnapshotPoller` (BackgroundService, `PeriodicTimer(3 s, TimeProvider)`) → `ITrafficSnapshotClient` → `IUiDispatcher.InvokeAsync(vm.Apply)`. Errors go to `vm.ReportError`; the UI keeps the last good data.

## Threading model
| Component | Concurrency | Protection |
|-----------|-------------|-----------|
| `VehicleRegistry` (generator) | single simulation thread | none needed |
| Outbox channel | 1 writer, 1 reader | `Channel<T>` |
| `PartitionedIngestPipeline` | many writers (gRPC calls), 1 reader per partition | `Channel<T>` per partition |
| `VehicleTrack` | 1 writer (its partition) | none needed |
| `BoundedLeaderboard` | N writers, 1 snapshot reader | private `Lock`, volatile threshold |
| `ConcurrentDictionary<plate, track>` | N writers | lock-free CD |

## Observability
- Metrics (`System.Diagnostics.Metrics`, exported via OpenTelemetry): processor `telemetry.ingested|rejected|processed`, `ingest.pending`, `vehicles.active`; generator `simulation.ticks`, `telemetry.emitted|published|dropped`, `stream.failures`.
- Health: `/health/live` (process), `/health/ready` (ingest backlog < 80 % capacity).
- Logs: structured, source-generated, event ids per subsystem.

## Configuration
Every app binds a POCO from a section (`Ingestion`, `Analytics`, `Generator`, `Client`) with `ValidateOnStart`. Env override: `Section__Key` (see `docker-compose.yml`).

## Known limitations / next steps
- Snapshot is not atomic across the 11 boards (fine for a dashboard; use copy-on-write state for audit-grade consistency).
- Single processor instance, in-memory state. Scaling out would reuse the partition key on a durable log (Kafka/Redis Streams) or move to per-vehicle actors (Orleans).
- Telemetry is fire-and-forget; nothing is persisted, so nothing can be replayed after a bug fix.
- No UI automation tests yet; `Client.Core`/`Client.Wpf` split is the prerequisite (FlaUI or Appium/WinAppDriver would drive `MainWindow`).
