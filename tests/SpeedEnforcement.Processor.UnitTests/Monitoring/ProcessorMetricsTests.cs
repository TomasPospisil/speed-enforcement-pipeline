using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Moq;
using SpeedEnforcement.Processor.Domain.Analytics;
using SpeedEnforcement.Processor.Host.Ingestion;
using SpeedEnforcement.Processor.Host.Monitoring;
using Xunit;

namespace SpeedEnforcement.Processor.UnitTests.Monitoring;

/// <summary>Metrics are part of the contract with operations: names, tags and gauge sources are asserted here.</summary>
public sealed class ProcessorMetricsTests : IDisposable
{
    private readonly ServiceProvider _provider = new ServiceCollection().AddMetrics().BuildServiceProvider();
    private readonly Mock<IIngestPipeline> _pipeline = new();
    private readonly Mock<IAnalyticsEngine> _engine = new();
    private readonly IMeterFactory _meterFactory;
    private readonly ProcessorMetrics _metrics;

    public ProcessorMetricsTests()
    {
        _meterFactory = _provider.GetRequiredService<IMeterFactory>();
        _metrics = new ProcessorMetrics(_meterFactory, _pipeline.Object, _engine.Object);
    }

    [Fact]
    public void TelemetryRejected_CountsPerReasonTag()
    {
        using var collector = new MetricCollector<long>(_meterFactory, ProcessorMetrics.MeterName, "telemetry.rejected");

        _metrics.TelemetryRejected("empty_plate");
        _metrics.TelemetryRejected("empty_plate");
        _metrics.TelemetryRejected("invalid_speed");

        var measurements = collector.GetMeasurementSnapshot();
        Assert.Equal(3, measurements.Sum(m => m.Value));
        Assert.Equal(2, measurements.Count(m => Equals(m.Tags["reason"], "empty_plate")));
    }

    [Fact]
    public void IngestPending_GaugeReadsPipelineBacklog()
    {
        _pipeline.SetupGet(p => p.PendingCount).Returns(42);
        using var collector = new MetricCollector<int>(_meterFactory, ProcessorMetrics.MeterName, "ingest.pending");

        collector.RecordObservableInstruments();

        Assert.Equal(42, collector.LastMeasurement?.Value);
    }

    [Fact]
    public void VehiclesActive_GaugeReadsEngine()
    {
        _engine.SetupGet(e => e.ActiveVehicles).Returns(1_000);
        using var collector = new MetricCollector<int>(_meterFactory, ProcessorMetrics.MeterName, "vehicles.active");

        collector.RecordObservableInstruments();

        Assert.Equal(1_000, collector.LastMeasurement?.Value);
    }

    public void Dispose() => _provider.Dispose();
}
