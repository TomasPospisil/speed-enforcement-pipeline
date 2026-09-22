using System.Diagnostics.Metrics;
using SpeedEnforcement.Processor.Domain.Analytics;
using SpeedEnforcement.Processor.Host.Ingestion;

namespace SpeedEnforcement.Processor.Host.Monitoring;

/// <summary>Domain-flavoured facade over System.Diagnostics.Metrics so call sites stay free of instrument plumbing.</summary>
public interface IProcessorMetrics
{
    void TelemetryIngested();
    void TelemetryRejected(string reason);
    void TelemetryProcessed(int partition);
}

public sealed class ProcessorMetrics : IProcessorMetrics
{
    public const string MeterName = "SpeedEnforcement.Processor";

    private readonly Counter<long> _ingested;
    private readonly Counter<long> _rejected;
    private readonly Counter<long> _processed;

    public ProcessorMetrics(IMeterFactory meterFactory, IIngestPipeline pipeline, IAnalyticsEngine engine)
    {
        var meter = meterFactory.Create(MeterName);

        _ingested = meter.CreateCounter<long>("telemetry.ingested", unit: "{message}",
            description: "Valid telemetry messages accepted from the gRPC stream.");
        _rejected = meter.CreateCounter<long>("telemetry.rejected", unit: "{message}",
            description: "Telemetry messages rejected by validation, tagged by reason.");
        _processed = meter.CreateCounter<long>("telemetry.processed", unit: "{message}",
            description: "Readings applied to the analytics engine, tagged by partition.");

        meter.CreateObservableGauge("ingest.pending", () => pipeline.PendingCount, unit: "{message}",
            description: "Readings buffered in the partitioned pipeline (backlog).");
        meter.CreateObservableGauge("vehicles.active", () => engine.ActiveVehicles, unit: "{vehicle}",
            description: "Vehicles currently tracked inside the monitored sector.");
    }

    public void TelemetryIngested() => _ingested.Add(1);

    public void TelemetryRejected(string reason) => _rejected.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void TelemetryProcessed(int partition) => _processed.Add(1, new KeyValuePair<string, object?>("partition", partition));
}
