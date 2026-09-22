# ADR-005: Processor = pure Domain + ASP.NET Core Host

**Status:** accepted

## Decision
Two projects: `Processor.Domain` (no package references: leaderboards, tracks, engine) and `Processor.Host` (gRPC service, channel pipeline, REST endpoint, DI, options, health, metrics). No separate Application layer.

## Why
- The Domain/Host boundary is the one that buys testability: the entire analytics core is testable without ASP.NET or gRPC.
- An Application layer would be ceremony without content in a prototype (YAGNI); it can be introduced when use cases multiply.
- Host uses `Microsoft.NET.Sdk.Web` (not Worker SDK) because it must serve REST and gRPC.
