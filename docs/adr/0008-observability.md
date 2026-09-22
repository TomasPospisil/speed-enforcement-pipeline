# ADR-008: Observability baseline

**Status:** accepted

## Decision
- Metrics via `System.Diagnostics.Metrics` behind a small facade per app (`IProcessorMetrics`, `IGeneratorMetrics`), exported through OpenTelemetry (console exporter opt-in). Names `noun.verb`, tags low-cardinality (`reason`, `partition`).
- Health: `/health/live` (process up) and `/health/ready` (ingest backlog below 80 % of capacity).
- Logging: source-generated `[LoggerMessage]`, structured, event ids per subsystem.
- Options validated at startup (`ValidateOnStart`) so misconfiguration fails fast with a readable message.

## Why
In a fire-and-forget stream (ADR-009) the metric `telemetry.rejected` *is* the error channel; the rest is what any deployable service needs to be operated.
