# ADR-009: Handling invalid messages in a client stream

**Status:** accepted

## Context
A gRPC client stream has exactly one back channel: the terminal status. The server cannot say "message 4 711 was invalid, carry on"; it can only end the stream.

## Decision
Hybrid, implemented by `IIngestRejectionPolicy` (`ThresholdRejectionPolicy`):
- Isolated invalid messages are logged, counted in `telemetry.rejected{reason}` and skipped; the stream continues.
- When rejections are systematic (N consecutive, or ratio over a sliding window above a limit) the server aborts the stream with `InvalidArgument` and a summary, so the producer learns immediately.
- `IngestSummary.Rejected` at stream end is informational only.
- The policy is registered **transient**: gRPC creates the service per call, so each stream has its own window.

## Rejected
- Fail-fast on first error: one bad reading stops all vehicles; a buggy producer would reconnect in a loop.
- Bidirectional stream with per-message acks: correct when the producer must know each message's fate; telemetry does not need it and the next reading arrives in one second.
