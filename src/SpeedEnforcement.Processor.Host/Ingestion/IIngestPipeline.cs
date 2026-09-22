using System.Threading.Channels;
using SpeedEnforcement.Processor.Domain.Telemetry;

namespace SpeedEnforcement.Processor.Host.Ingestion;

/// <summary>
/// Boundary between transport (gRPC today) and analytics. Anything that can produce a
/// <see cref="TelemetryReading"/> can feed the pipeline; the analytics side never sees the wire format.
/// </summary>
public interface IIngestPipeline
{
    int PartitionCount { get; }

    /// <summary>Readings waiting across all partitions. Drives the readiness probe and the backlog gauge.</summary>
    int PendingCount { get; }

    /// <summary>Routes the reading to its partition. Awaits when that partition is full (backpressure up to the gRPC stream).</summary>
    ValueTask EnqueueAsync(TelemetryReading reading, CancellationToken cancellationToken);

    ChannelReader<TelemetryReading> GetReader(int partition);

    /// <summary>Signals end of input; consumers drain and exit.</summary>
    void Complete();
}
