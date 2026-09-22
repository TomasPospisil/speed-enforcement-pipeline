namespace SpeedEnforcement.Processor.Domain.Leaderboards;

/// <summary>Anything that can be ranked: a score plus the plate used as the deterministic tie-breaker.</summary>
public interface ILeaderboardEntry
{
    string NumberPlate { get; }
    float Score { get; }
}

/// <summary>Instantaneous speed of a vehicle at one camera. Immutable: a vehicle passes a camera once.</summary>
public sealed record CameraLeaderboardEntry(string NumberPlate, float Speed) : ILeaderboardEntry
{
    public float Score => Speed;
}

/// <summary>Cumulative average of a vehicle. Replaced on every camera pass, hence the plate index in <see cref="BoundedLeaderboard{TEntry}"/>.</summary>
public sealed record GlobalLeaderboardEntry(string NumberPlate, float AverageSpeed, int CamerasPassed) : ILeaderboardEntry
{
    public float Score => AverageSpeed;
}
