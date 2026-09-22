using Microsoft.Extensions.Options;
using SpeedEnforcement.Generator.Options;

namespace SpeedEnforcement.Generator.Simulation;

public interface ISpeedModel
{
    /// <summary>Speed (km/h) of the vehicle at the camera it is about to pass.</summary>
    float Next(Vehicle vehicle);
}

/// <summary>
/// Each vehicle gets a personal baseline drawn around the highway limit (normal distribution), then performs
/// a bounded random walk between cameras. Produces plausible traffic: most cars near 130, a tail of speeders,
/// no teleporting between consecutive cameras. Deterministic when <see cref="Random"/> is seeded.
/// </summary>
public sealed class RandomWalkSpeedModel : ISpeedModel
{
    private readonly GeneratorOptions _options;
    private readonly Random _random;

    public RandomWalkSpeedModel(IOptions<GeneratorOptions> options, Random random)
    {
        _options = options.Value;
        _random = random;
    }

    public float Next(Vehicle vehicle)
    {
        // TODO: first camera -> baseline ~ N(BaselineSpeedKmh, SpeedStdDevKmh);
        //       later cameras -> LastSpeed + uniform(-MaxSpeedStepKmh, +MaxSpeedStepKmh);
        //       clamp to [MinSpeedKmh, MaxSpeedKmh].
        _ = _options;
        _ = _random;
        throw new NotImplementedException();
    }
}
