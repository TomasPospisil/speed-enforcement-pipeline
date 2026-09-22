using System.Collections.Immutable;

namespace SpeedEnforcement.Processor.Domain.Leaderboards;

/// <summary>One <see cref="BoundedLeaderboard{TEntry}"/> per camera, indexed 1..CameraCount.</summary>
public sealed class CameraLeaderboardRegistry
{
    private readonly BoundedLeaderboard<CameraLeaderboardEntry>[] _boards;

    public CameraLeaderboardRegistry(AnalyticsOptions options)
    {
        _boards = Enumerable.Range(0, options.CameraCount)
            .Select(_ => new BoundedLeaderboard<CameraLeaderboardEntry>(
                options.LeaderboardCapacity, ScoreThenPlateComparer<CameraLeaderboardEntry>.Instance))
            .ToArray();
    }

    public int CameraCount => _boards.Length;

    /// <param name="cameraId">1-based camera id as it appears on the wire.</param>
    public BoundedLeaderboard<CameraLeaderboardEntry> For(int cameraId)
    {
        // TODO: ArgumentOutOfRangeException for cameraId outside 1..CameraCount.
        return _boards[cameraId - 1];
    }

    public ImmutableArray<ImmutableArray<CameraLeaderboardEntry>> Snapshot()
        => [.. _boards.Select(b => b.Snapshot())];
}
