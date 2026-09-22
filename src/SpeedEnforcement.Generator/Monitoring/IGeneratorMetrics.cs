using System.Diagnostics.Metrics;

namespace SpeedEnforcement.Generator.Monitoring;

public interface IGeneratorMetrics
{
    void TickCompleted(int activeVehicles, int emitted, int exited);
    void TelemetryPublished();
    void TelemetryDropped();
    void StreamFailed();
}

public sealed class GeneratorMetrics : IGeneratorMetrics
{
    public const string MeterName = "SpeedEnforcement.Generator";

    private readonly Counter<long> _ticks;
    private readonly Counter<long> _emitted;
    private readonly Counter<long> _published;
    private readonly Counter<long> _dropped;
    private readonly Counter<long> _streamFailures;
    private int _activeVehicles;

    public GeneratorMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _ticks = meter.CreateCounter<long>("simulation.ticks", unit: "{tick}");
        _emitted = meter.CreateCounter<long>("telemetry.emitted", unit: "{message}",
            description: "Messages written to the outbox by the simulation.");
        _published = meter.CreateCounter<long>("telemetry.published", unit: "{message}",
            description: "Messages written to the gRPC stream.");
        _dropped = meter.CreateCounter<long>("telemetry.dropped", unit: "{message}",
            description: "Messages evicted from the full outbox while the processor was unreachable.");
        _streamFailures = meter.CreateCounter<long>("stream.failures", unit: "{failure}");

        meter.CreateObservableGauge("vehicles.active", () => Volatile.Read(ref _activeVehicles), unit: "{vehicle}");
    }

    public void TickCompleted(int activeVehicles, int emitted, int exited)
    {
        Volatile.Write(ref _activeVehicles, activeVehicles);
        _ticks.Add(1);
        _emitted.Add(emitted);
        _ = exited;
    }

    public void TelemetryPublished() => _published.Add(1);

    public void TelemetryDropped() => _dropped.Add(1);

    public void StreamFailed() => _streamFailures.Add(1);
}
