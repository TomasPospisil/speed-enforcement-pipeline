namespace SpeedEnforcement.Processor.Domain.Leaderboards;

/// <summary>
/// Score descending, then NumberPlate ascending (ordinal). The tie-break is part of the ordering itself,
/// so determinism is a property of the data structure rather than an afterthought at snapshot time.
/// </summary>
public sealed class ScoreThenPlateComparer<TEntry> : IComparer<TEntry>
    where TEntry : ILeaderboardEntry
{
    public static ScoreThenPlateComparer<TEntry> Instance { get; } = new();

    public int Compare(TEntry? x, TEntry? y)
    {
        // TODO: null handling; y.Score.CompareTo(x.Score); then string.CompareOrdinal(x.NumberPlate, y.NumberPlate).
        throw new NotImplementedException();
    }
}
