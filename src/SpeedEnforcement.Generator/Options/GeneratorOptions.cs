namespace SpeedEnforcement.Generator.Options;

/// <summary>Bound from the "Generator" section; overridable via env vars <c>Generator__Key</c>.</summary>
public sealed class GeneratorOptions
{
    public const string SectionName = "Generator";

    public string ProcessorAddress { get; set; } = "http://localhost:5001";

    public int VehiclesPerTick { get; set; } = 100;

    public int CameraCount { get; set; } = 10;

    public TimeSpan TickInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Outbox size. Default holds two full sectors' worth of messages (2 × VehiclesPerTick × CameraCount).</summary>
    public int OutboxCapacity { get; set; } = 2_000;

    public float BaselineSpeedKmh { get; set; } = 130f;

    public float SpeedStdDevKmh { get; set; } = 12f;

    /// <summary>Max change between two consecutive cameras.</summary>
    public float MaxSpeedStepKmh { get; set; } = 5f;

    public float MinSpeedKmh { get; set; } = 60f;

    public float MaxSpeedKmh { get; set; } = 200f;

    /// <summary>Set for reproducible runs; null uses <see cref="Random.Shared"/>.</summary>
    public int? RandomSeed { get; set; }
}
