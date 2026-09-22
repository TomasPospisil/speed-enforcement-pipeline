using Grpc.Core;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Contracts.Telemetry.V1;
using SpeedEnforcement.Processor.Domain;
using SpeedEnforcement.Processor.Host.Logs;
using SpeedEnforcement.Processor.Host.Monitoring;

namespace SpeedEnforcement.Processor.Host.Ingestion;

/// <summary>
/// gRPC server side of the client stream. Thin: validate, count, hand over to the pipeline.
/// Instantiated per call, so all fields are per-stream.
/// </summary>
public sealed class TelemetryIngestService : TelemetryIngest.TelemetryIngestBase
{
    private readonly IIngestPipeline _pipeline;
    private readonly IIngestRejectionPolicy _rejectionPolicy;
    private readonly IProcessorMetrics _metrics;
    private readonly ILogger<TelemetryIngestService> _logger;
    private readonly int _cameraCount;

    public TelemetryIngestService(
        IIngestPipeline pipeline,
        IIngestRejectionPolicy rejectionPolicy,
        IProcessorMetrics metrics,
        IOptions<AnalyticsOptions> analyticsOptions,
        ILogger<TelemetryIngestService> logger)
    {
        _pipeline = pipeline;
        _rejectionPolicy = rejectionPolicy;
        _metrics = metrics;
        _logger = logger;
        _cameraCount = analyticsOptions.Value.CameraCount;
    }

    public override async Task<IngestSummary> Ingest(
        IAsyncStreamReader<TelemetryMessage> requestStream,
        ServerCallContext context)
    {
        long accepted = 0;
        long rejected = 0;
        _logger.StreamOpened(context.Peer);

        await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
        {
            if (!TelemetryMapper.TryMap(message, _cameraCount, out var reading, out var reason))
            {
                rejected++;
                _metrics.TelemetryRejected(reason);
                _logger.TelemetryRejected(message.NumberPlate, message.CameraId, reason);

                if (_rejectionPolicy.RecordRejection(reason))
                {
                    _logger.StreamAborted(context.Peer, accepted, rejected, reason);
                    throw new RpcException(new Status(
                        StatusCode.InvalidArgument,
                        $"Rejection threshold exceeded after {accepted} accepted / {rejected} rejected messages; last reason: {reason}."));
                }

                continue;
            }

            _rejectionPolicy.RecordAccepted();
            await _pipeline.EnqueueAsync(reading, context.CancellationToken);
            accepted++;
            _metrics.TelemetryIngested();
        }

        _logger.StreamCompleted(context.Peer, accepted, rejected);
        return new IngestSummary { Accepted = accepted, Rejected = rejected };
    }
}
