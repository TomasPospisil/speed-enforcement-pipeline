using System.Collections.Immutable;
using SpeedEnforcement.Processor.Domain.Leaderboards;

namespace SpeedEnforcement.Processor.Domain.Analytics;

/// <summary>
/// Domain-level snapshot (not the REST DTO). Each board is copied under its own lock; the snapshot is
/// not atomic across boards, which is acceptable for a dashboard polled every 3 s (documented trade-off).
/// </summary>
public sealed record AnalyticsSnapshot(
    ImmutableArray<ImmutableArray<CameraLeaderboardEntry>> CameraLeaderboards,
    ImmutableArray<GlobalLeaderboardEntry> GlobalLeaderboard);
