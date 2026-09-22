using System.Collections.Immutable;

namespace SpeedEnforcement.Processor.Domain.Leaderboards;

/// <summary>
/// Strictly bounded Top-N. Thread-safe via one private lock per instance (fine-grained: the processor holds
/// 11 of these, never a global lock). The critical section is O(log N) over at most N = 10 items.
/// <para>
/// Fast path: <see cref="Threshold"/> is the score of the current N-th entry, published with volatile semantics.
/// Callers may skip the lock entirely when the board is full and the candidate does not beat it,
/// which is the case for the vast majority of readings.
/// </para>
/// </summary>
public sealed class BoundedLeaderboard<TEntry>
    where TEntry : ILeaderboardEntry
{
    private readonly int _capacity;
    private readonly SortedSet<TEntry> _entries;
    private readonly Dictionary<string, TEntry> _byPlate;
    private readonly Lock _lock = new();
    private float _threshold = float.NegativeInfinity;

    public BoundedLeaderboard(int capacity, IComparer<TEntry> comparer)
    {
        _capacity = capacity;
        _entries = new SortedSet<TEntry>(comparer);
        _byPlate = new Dictionary<string, TEntry>(capacity, StringComparer.Ordinal);
    }

    public int Capacity => _capacity;

    /// <summary>Score of the weakest entry when the board is full; <see cref="float.NegativeInfinity"/> otherwise.</summary>
    public float Threshold => Volatile.Read(ref _threshold);

    /// <summary>Inserts or replaces the entry for <c>entry.NumberPlate</c>; evicts the weakest when over capacity.</summary>
    /// <returns><c>true</c> if the entry is on the board after the call.</returns>
    public bool Upsert(TEntry entry)
    {
        // TODO: threshold fast path (skip lock when full and entry.Score <= Threshold and plate not present);
        //       lock (_lock) { remove existing by plate; add; evict _entries.Max when Count > _capacity; refresh _threshold; }
        _ = _entries;
        _ = _byPlate;
        throw new NotImplementedException();
    }

    /// <summary>Consistent copy taken under the lock; the REST endpoint never holds the lock longer than a 10-item copy.</summary>
    public ImmutableArray<TEntry> Snapshot()
    {
        lock (_lock)
        {
            return [.. _entries];
        }
    }
}
