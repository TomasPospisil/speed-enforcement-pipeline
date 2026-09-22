namespace SpeedEnforcement.Generator.Publishing;

public interface IBackoffPolicy
{
    /// <summary>Delay before the next reconnect attempt; grows with consecutive failures.</summary>
    TimeSpan Next();

    /// <summary>Called after a successful connection.</summary>
    void Reset();
}

/// <summary>Exponential growth with a cap and jitter, so a fleet of generators does not reconnect in lockstep.</summary>
public sealed class ExponentialBackoff : IBackoffPolicy
{
    private static readonly TimeSpan Initial = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan Max = TimeSpan.FromSeconds(30);

    private readonly Random _random;
    private int _attempt;

    public ExponentialBackoff(Random random)
    {
        _random = random;
    }

    public TimeSpan Next()
    {
        // TODO: min(Initial * 2^_attempt, Max) * uniform(0.8, 1.2); _attempt++.
        _ = _random;
        _ = _attempt;
        _ = Initial;
        _ = Max;
        throw new NotImplementedException();
    }

    public void Reset() => _attempt = 0;
}
