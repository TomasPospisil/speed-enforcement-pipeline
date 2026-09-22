using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using SpeedEnforcement.Processor.Domain.Analytics;
using SpeedEnforcement.Processor.Domain.Telemetry;
using SpeedEnforcement.Processor.Host.Ingestion;
using SpeedEnforcement.Processor.Host.Monitoring;
using SpeedEnforcement.Tests.Common.Builders;
using Xunit;

namespace SpeedEnforcement.Processor.UnitTests.Ingestion;

/// <summary>Worker wiring: consumes every partition, one failure never stops the loop, failures are logged.</summary>
public class IngestWorkerTests
{
    private readonly Channel<TelemetryReading> _partition = Channel.CreateUnbounded<TelemetryReading>();
    private readonly Mock<IIngestPipeline> _pipeline = new();
    private readonly Mock<IAnalyticsEngine> _engine = new();
    private readonly Mock<IProcessorMetrics> _metrics = new();
    private readonly FakeLogger<IngestWorker> _logger = new();
    private readonly IngestWorker _worker;

    public IngestWorkerTests()
    {
        _pipeline.SetupGet(p => p.PartitionCount).Returns(1);
        _pipeline.Setup(p => p.GetReader(0)).Returns(_partition.Reader);
        _worker = new IngestWorker(_pipeline.Object, _engine.Object, _metrics.Object, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_AppliesEveryReadingAndRecordsMetric()
    {
        var ct = TestContext.Current.CancellationToken;
        await _worker.StartAsync(ct);

        await _partition.Writer.WriteAsync(TelemetryReadingBuilder.Build("AAA0001"), ct);
        await _partition.Writer.WriteAsync(TelemetryReadingBuilder.Build("AAA0002"), ct);
        _partition.Writer.Complete();
        await _worker.ExecuteTask!.WaitAsync(ct);

        _engine.Verify(e => e.Apply(It.Ref<TelemetryReading>.IsAny), Times.Exactly(2));
        _metrics.Verify(m => m.TelemetryProcessed(0), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_EngineThrows_LogsErrorAndKeepsConsuming()
    {
        var ct = TestContext.Current.CancellationToken;
        _engine.SetupSequence(e => e.Apply(It.Ref<TelemetryReading>.IsAny))
            .Throws(new InvalidOperationException("boom"))
            .Pass();
        await _worker.StartAsync(ct);

        await _partition.Writer.WriteAsync(TelemetryReadingBuilder.Build("AAA0001"), ct);
        await _partition.Writer.WriteAsync(TelemetryReadingBuilder.Build("AAA0002"), ct);
        _partition.Writer.Complete();
        await _worker.ExecuteTask!.WaitAsync(ct);

        _engine.Verify(e => e.Apply(It.Ref<TelemetryReading>.IsAny), Times.Exactly(2));
        var error = Assert.Single(_logger.Collector.GetSnapshot(), r => r.Level == LogLevel.Error);
        Assert.Equal(2001, error.Id.Id);
        Assert.Contains("AAA0001", error.Message, StringComparison.Ordinal);
    }
}
