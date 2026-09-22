# ADR-002: Partitioned channels + fine-grained leaderboard locks

**Status:** accepted

## Context
Ingestion must be concurrent and thread-safe, yet per-vehicle logic (gate ≥ 3 cameras, purge at camera 10) is order-sensitive: processing camera 10 before camera 9 of the same plate would purge the track and resurrect it.

## Decision
`PartitionedIngestPipeline`: N bounded `Channel<TelemetryReading>`, reading routed by `hash(NumberPlate) % N`, one consumer per partition (`IngestWorker`). Leaderboards (10 camera + 1 global) each own a private `Lock`; `BoundedLeaderboard.Threshold` is published with volatile semantics so callers skip the lock when a candidate cannot enter a full board.

## Why
- Same plate → same consumer → per-vehicle ordering preserved, `VehicleTrack` is single-writer (no lock).
- Different plates → parallel across partitions.
- 11 tiny critical sections (≤ 10 items, O(log 10)) instead of one global lock.
- Bounded channels with `Wait` propagate backpressure up to the gRPC stream.

## Rejected
- One global lock: serialises everything.
- Lock-free everywhere: correct Top-N under concurrent eviction is hard and unnecessary at this size.
- Actor model (Akka.NET/Orleans): the right next step for scale-out, disproportionate for a prototype.
