namespace SpeedEnforcement.Processor.Domain;

/// <summary>Tunables of the analytics engine. Bound and validated by the host (see AnalyticsOptionsExtensions).</summary>
public sealed class AnalyticsOptions
{
    public int CameraCount { get; set; } = 10;
    public int LeaderboardCapacity { get; set; } = 10;

    /// <summary>A vehicle enters the global leaderboard only after passing this many cameras (early-stage skew gate).</summary>
    public int GlobalLeaderboardGate { get; set; } = 3;
}
