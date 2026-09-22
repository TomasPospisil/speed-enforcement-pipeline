using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpeedEnforcement.Contracts.Telemetry.V1;
using SpeedEnforcement.Generator.Logs;
using SpeedEnforcement.Generator.Monitoring;

namespace SpeedEnforcement.Generator.Publishing;

/// <summary>
/// Drains the outbox into one long-lived gRPC client stream (ADR-001). On transport failure the stream is
/// reopened after a backoff; messages that were in flight are lost by design (telemetry is fire-and-forget,
/// the next reading arrives one second later).
/// </summary>
public sealed class GrpcTelemetryPublisher : BackgroundService
{
    private readonly TelemetryIngest.TelemetryIngestClient _client;
    private readonly ITelemetryOutbox _outbox;
    private readonly IBackoffPolicy _backoff;
    private readonly IGeneratorMetrics _metrics;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GrpcTelemetryPublisher> _logger;

    public GrpcTelemetryPublisher(
        TelemetryIngest.TelemetryIngestClient client,
        ITelemetryOutbox outbox,
        IBackoffPolicy backoff,
        IGeneratorMetrics metrics,
        TimeProvider timeProvider,
        ILogger<GrpcTelemetryPublisher> logger)
    {
        _client = client;
        _outbox = outbox;
        _backoff = backoff;
        _metrics = metrics;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var call = _client.Ingest(cancellationToken: stoppingToken);
                _logger.StreamOpened();
                _backoff.Reset();

                await foreach (var message in _outbox.Reader.ReadAllAsync(stoppingToken))
                {
                    await call.RequestStream.WriteAsync(message, stoppingToken);
                    _metrics.TelemetryPublished();
                }

                // Reached only if the outbox writer completes (graceful shutdown path, TODO: complete the
                // outbox from SimulationLoop.StopAsync so the summary is logged instead of a cancellation).
                await call.RequestStream.CompleteAsync();
                var summary = await call;
                _logger.StreamCompleted(summary.Accepted, summary.Rejected);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (RpcException ex)
            {
                var delay = _backoff.Next();
                _metrics.StreamFailed();
                _logger.StreamFailed(ex, ex.StatusCode, delay);
                await Task.Delay(delay, _timeProvider, stoppingToken);
            }
        }
    }
}
