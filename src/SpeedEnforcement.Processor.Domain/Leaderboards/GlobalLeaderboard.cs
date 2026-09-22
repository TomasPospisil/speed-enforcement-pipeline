using System.Collections.Immutable;
using SpeedEnforcement.Processor.Domain.Tracking;

namespace SpeedEnforcement.Processor.Domain.Leaderboards;

/// <summary>
/// System-wide Top-N by cumulative average speed. Applies the entry gate (min. cameras passed) and delegates
/// ranking to a <see cref="BoundedLeaderboard{TEntry}"/>. Entries are value copies, so purging the
/// <see cref="VehicleTrack"/> at the last camera leaves the board intact (ADR-004: "all recorded traffic").
/// </summary>
public sealed class GlobalLeaderboard
{
    private readonly BoundedLeaderboard<GlobalLeaderboardEntry> _board;
    private readonly int _gate;

    public GlobalLeaderboard(AnalyticsOptions options)
    {
        _board = new BoundedLeaderboard<GlobalLeaderboardEntry>(
            options.LeaderboardCapacity, ScoreThenPlateComparer<GlobalLeaderboardEntry>.Instance);
        _gate = options.GlobalLeaderboardGate;
    }

    /// <summary>Re-ranks the vehicle if it has passed the gate; no-op otherwise.</summary>
    public void Consider(VehicleTrack track)
    {
        // TODO: if (track.CamerasPassed < _gate) return;
        //       _board.Upsert(new GlobalLeaderboardEntry(track.NumberPlate, track.AverageSpeed, track.CamerasPassed));
        _ = _gate;
        throw new NotImplementedException();
    }

    public ImmutableArray<GlobalLeaderboardEntry> Snapshot() => _board.Snapshot();
}
