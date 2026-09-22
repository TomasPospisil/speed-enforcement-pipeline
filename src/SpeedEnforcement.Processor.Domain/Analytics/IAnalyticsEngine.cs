using SpeedEnforcement.Processor.Domain.Telemetry;

namespace SpeedEnforcement.Processor.Domain.Analytics;

public interface IAnalyticsEngine
{
    /// <summary>
    /// Applies one reading. Safe to call concurrently for different plates; readings of the same plate
    /// must be applied sequentially and in camera order (guaranteed by the partitioned ingest pipeline).
    /// </summary>
    void Apply(in TelemetryReading reading);

    AnalyticsSnapshot Snapshot();

    /// <summary>Number of vehicles currently tracked (inside the sector). Exposed for metrics and health.</summary>
    int ActiveVehicles { get; }
}
