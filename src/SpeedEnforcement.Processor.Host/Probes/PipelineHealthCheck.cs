using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Processor.Host.Ingestion;
using SpeedEnforcement.Processor.Host.Options;

namespace SpeedEnforcement.Processor.Host.Probes;

/// <summary>Readiness: degraded when the ingest backlog approaches capacity (consumers are not keeping up).</summary>
public sealed class PipelineHealthCheck : IHealthCheck
{
    private const double DegradedRatio = 0.8;

    private readonly IIngestPipeline _pipeline;
    private readonly IngestionOptions _options;

    public PipelineHealthCheck(IIngestPipeline pipeline, IOptions<IngestionOptions> options)
    {
        _pipeline = pipeline;
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var pending = _pipeline.PendingCount;
        var capacity = (long)_options.ChannelCapacity * _pipeline.PartitionCount;
        var data = new Dictionary<string, object> { ["pending"] = pending, ["capacity"] = capacity };

        var result = pending >= capacity * DegradedRatio
            ? HealthCheckResult.Degraded($"Ingest backlog at {pending}/{capacity}.", data: data)
            : HealthCheckResult.Healthy(data: data);

        return Task.FromResult(result);
    }
}
