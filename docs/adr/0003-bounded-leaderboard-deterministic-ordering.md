# ADR-003: `BoundedLeaderboard<T>` with deterministic ordering

**Status:** accepted

## Decision
`SortedSet<TEntry>` with `ScoreThenPlateComparer` (score DESC, plate ordinal ASC), capacity N, eviction of `Max` when over capacity, plus a `Dictionary<plate, entry>` for O(1) replace of a vehicle's global entry as its average changes. `Snapshot()` returns an `ImmutableArray` copied under the lock.

## Why
- The tie-break required by the assignment is part of the ordering itself, so it is a property of the data structure and unit-testable in isolation.
- Camera entries are immutable (a vehicle passes a camera once); global entries are replaced via the plate index.
- The REST endpoint holds a lock only for a 10-item copy.

## Consequence
Snapshot is not atomic across the 11 boards; acceptable for a 3-second dashboard (documented in `architecture.md`).
