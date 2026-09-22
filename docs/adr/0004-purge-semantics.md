# ADR-004: Purge semantics of the global leaderboard

**Status:** accepted (interpretation of an ambiguous requirement)

## Context
The assignment titles the global board "Live Active Traffic Only" but the purge rule says it "represents all recorded traffic" and purges *tracking data* when a vehicle passes camera 10 without scoring.

## Decision
- Leaderboards store **value copies** (plate + score), never references to `VehicleTrack`.
- `VehicleTrack` is removed at camera 10 **unconditionally** – its average is final and it has no further use.
- Leaderboard entries survive the vehicle's exit until displaced by a better one ("all recorded traffic").

## Why
The purge text is the more specific statement; keeping copies makes the purge trivially safe. If "live-only" is intended, the change is one line: remove the plate from the global board at camera 10.

## Follow-up
Orphan sweep (drop tracks with no reading for > X s) to survive lost camera-10 messages after a generator restart.
