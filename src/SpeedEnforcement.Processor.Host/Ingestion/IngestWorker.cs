using SpeedEnforcement.Processor.Domain.Analytics;
using SpeedEnforcement.Processor.Host.Logs;
using SpeedEnforcement.Processor.Host.Monitoring;

namespace SpeedEnforcement.Processor.Host.Ingestion;

/// <summary>Runs one consumer loop per partition and feeds the analytics engine. Failures are per-reading, never fatal.</summary>
public sealed class IngestWorker : BackgroundService
{
    private readonly IIngestPipeline _pipeline;
    private readonly IAnalyticsEngine _engine;
    private readonly IProcessorMetrics _metrics;
    private readonly ILogger<IngestWorker> _logger;

    public IngestWorker(
        IIngestPipeline pipeline,
        IAnalyticsEngine engine,
        IProcessorMetrics metrics,
        ILogger<IngestWorker> logger)
    {
        _pipeline = pipeline;
        _engine = engine;
        _metrics = metrics;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.IngestWorkerStarted(_pipeline.PartitionCount);

        var consumers = Enumerable.Range(0, _pipeline.PartitionCount)
            .Select(partition => ConsumeAsync(partition, stoppingToken));

        return Task.WhenAll(consumers);
    }

    private async Task ConsumeAsync(int partition, CancellationToken stoppingToken)
    {
        var reader = _pipeline.GetReader(partition);

        await foreach (var reading in reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _engine.Apply(in reading);
                _metrics.TelemetryProcessed(partition);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.ReadingFailed(ex, reading.NumberPlate, reading.CameraId, partition);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _pipeline.Complete();
        return base.StopAsync(cancellationToken);
    }
}
