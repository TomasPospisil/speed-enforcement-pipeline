# ADR-007: Time and randomness are injected

**Status:** accepted

## Decision
`TimeProvider` everywhere a clock is read (tick loop, timestamps, snapshot time, poll interval, backoff delays); `Random` registered once (seeded when `Generator:RandomSeed` is set, otherwise `Random.Shared`).

## Why
- `FakeTimeProvider` makes timer-driven code deterministic and instant in tests.
- A seeded run reproduces a traffic pattern for bug reports and speed-model tests.
- `PeriodicTimer(period, TimeProvider)` and `Task.Delay(delay, TimeProvider, ct)` are first-class in .NET 8+.
