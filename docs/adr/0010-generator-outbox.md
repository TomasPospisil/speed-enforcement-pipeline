# ADR-010: Generator separates simulation from networking via an outbox channel

**Status:** accepted

## Decision
`SimulationLoop` writes `TelemetryMessage`s into a bounded `Channel<T>` (`ChannelTelemetryOutbox`, `DropOldest`). `GrpcTelemetryPublisher` (a second BackgroundService) drains it into the gRPC stream and owns reconnect with exponential backoff.

## Why
- The simulation must tick exactly once per second regardless of network latency or a processor restart.
- Stale vehicle positions are worthless: when the processor is down for long, dropping the oldest messages (counted in `telemetry.dropped`) beats stalling the simulation or growing memory.
- Two small single-purpose services are easier to test than one loop doing both.
