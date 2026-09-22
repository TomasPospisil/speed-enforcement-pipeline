namespace SpeedEnforcement.Generator.Simulation;

public interface IPlateGenerator
{
    /// <summary>Returns a plate unique within this run and, thanks to the run prefix, across generator restarts.</summary>
    string Next();
}

/// <summary>
/// Run prefix (derived from process start) + zero-padded sequence. Uniqueness is guaranteed by construction,
/// not by a random draw, so no collision check is needed. Restarting the generator yields a new prefix:
/// the processor never sees a plate reused for a different vehicle.
/// </summary>
public sealed class SequentialPlateGenerator : IPlateGenerator
{
    private readonly string _runPrefix;
    private long _sequence = 0;

    public SequentialPlateGenerator(TimeProvider timeProvider)
    {
        // TODO: derive a short prefix, e.g. 2 base-36 chars from timeProvider.GetUtcNow().
        _runPrefix = string.Empty;
        _ = timeProvider;
    }

    public string Next()
    {
        // TODO: $"{_runPrefix}{Interlocked.Increment(ref _sequence):D7}" formatted like a plate.
        _ = _runPrefix;
        _ = _sequence;
        throw new NotImplementedException();
    }
}
