using SpeedEnforcement.Processor.Domain.Leaderboards;
using Xunit;

namespace SpeedEnforcement.Processor.UnitTests.Leaderboards;

public class BoundedLeaderboardTests
{
    private const string Todo = "Skeleton: implement BoundedLeaderboard.Upsert and the comparer first.";

    [Fact]
    public void Snapshot_OnEmptyBoard_IsEmpty()
    {
        var board = new BoundedLeaderboard<CameraLeaderboardEntry>(10, ScoreThenPlateComparer<CameraLeaderboardEntry>.Instance);

        Assert.Empty(board.Snapshot());
        Assert.Equal(10, board.Capacity);
        Assert.Equal(float.NegativeInfinity, board.Threshold);
    }

    [Fact(Skip = Todo)]
    public void Upsert_NeverExceedsCapacity() { }

    [Fact(Skip = Todo)]
    public void Upsert_OrdersByScoreDescending() { }

    [Fact(Skip = Todo)]
    public void Upsert_TieOnScore_OrdersByPlateAscendingOrdinal() { }

    [Fact(Skip = Todo)]
    public void Upsert_SamePlateAgain_ReplacesInsteadOfDuplicating() { }

    [Fact(Skip = Todo)]
    public void Threshold_BecomesWeakestScore_OnceBoardIsFull() { }

    [Fact(Skip = Todo)]
    public void Upsert_BelowThresholdOnFullBoard_IsRejectedWithoutChangingBoard() { }

    [Fact(Skip = Todo)]
    public void Upsert_ManyConcurrentWriters_MatchesSequentialReferenceResult() { }
}
