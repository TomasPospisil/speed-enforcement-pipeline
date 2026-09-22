using SpeedEnforcement.Processor.Domain;
using SpeedEnforcement.Processor.Domain.Analytics;
using Xunit;

namespace SpeedEnforcement.Processor.UnitTests.Analytics;

public class AnalyticsEngineTests
{
    private const string Todo = "Skeleton: implement VehicleTrack, BoundedLeaderboard and AnalyticsEngine.Apply first.";

    [Fact]
    public void Snapshot_OnFreshEngine_HasOneEmptyBoardPerCamera()
    {
        var engine = new AnalyticsEngine(new AnalyticsOptions { CameraCount = 10 });

        var snapshot = engine.Snapshot();

        Assert.Equal(10, snapshot.CameraLeaderboards.Length);
        Assert.All(snapshot.CameraLeaderboards, board => Assert.Empty(board));
        Assert.Empty(snapshot.GlobalLeaderboard);
        Assert.Equal(0, engine.ActiveVehicles);
    }

    [Fact(Skip = Todo)]
    public void Apply_FirstReading_CreatesTrackAndEntersCameraBoard() { }

    [Fact(Skip = Todo)]
    public void Apply_BeforeGate_DoesNotEnterGlobalBoard() { }

    [Fact(Skip = Todo)]
    public void Apply_AtGate_EntersGlobalBoardWithCumulativeAverage() { }

    [Fact(Skip = Todo)]
    public void Apply_LastCamera_PurgesTrack_ButKeepsLeaderboardEntries() { }

    [Fact(Skip = Todo)]
    public void Apply_ReadingsOfDifferentPlatesInParallel_MatchesSequentialResult() { }
}
