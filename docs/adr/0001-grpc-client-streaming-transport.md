# ADR-001: gRPC client streaming for Generator → Processor

**Status:** accepted

## Context
The generator emits ~1 000 small messages per second in one direction. The assignment leaves the transport open (gRPC, MQTT, WebSockets, REST, TCP).

## Decision
One long-lived **gRPC client stream** (`rpc Ingest(stream TelemetryMessage) returns (IngestSummary)`), contract in `Contracts/Protos/telemetry.proto`. Kestrel exposes a dedicated HTTP/2 (h2c) endpoint on :5001 for it; REST stays on :5000.

## Why
1. Shape fits: an endless one-way stream, not request/response.
2. Zero extra infrastructure: two processes, no broker container.
3. Backpressure (HTTP/2 flow control) and in-order delivery come from the protocol; the partitioned pipeline relies on the ordering.
4. Contract-first `.proto` generates both sides at build time.
5. Binary Protobuf is cheap on the hot path (secondary at this volume).

## Rejected
- **MQTT + broker** – natural for many devices and many consumers; here it adds a container without a second consumer. Revisit when one appears.
- **REST per message** – 1 000 HTTP requests/s of overhead, no ordering guarantee across connections.
- **WebSockets / raw TCP** – custom framing and reconnect logic for no gain.

## Consequences
- Client stream has no per-message back channel → ADR-009.
- Binary payload needs `grpcurl`/Postman to inspect.
