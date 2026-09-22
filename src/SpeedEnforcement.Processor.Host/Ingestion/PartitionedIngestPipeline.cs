using System.Threading.Channels;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Processor.Domain.Telemetry;
using SpeedEnforcement.Processor.Host.Options;

namespace SpeedEnforcement.Processor.Host.Ingestion;

/// <summary>
/// N bounded channels, one consumer each. A plate always hashes to the same partition, so readings of one
/// vehicle are processed sequentially and in order while different vehicles are processed in parallel
/// (same idea as a Kafka partition key). This is what lets <c>VehicleTrack</c> be lock-free (ADR-002).
/// </summary>
public sealed class PartitionedIngestPipeline : IIngestPipeline
{
    private readonly Channel<TelemetryReading>[] _partitions;

    public PartitionedIngestPipeline(IOptions<IngestionOptions> options)
    {
        var o = options.Value;
        _partitions = Enumerable.Range(0, o.PartitionCount)
            .Select(_ => Channel.CreateBounded<TelemetryReading>(new BoundedChannelOptions(o.ChannelCapacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait,
            }))
            .ToArray();
    }

    public int PartitionCount => _partitions.Length;

    public int PendingCount => _partitions.Sum(p => p.Reader.Count);

    public ValueTask EnqueueAsync(TelemetryReading reading, CancellationToken cancellationToken)
        => _partitions[SelectPartition(reading.NumberPlate)].Writer.WriteAsync(reading, cancellationToken);

    public ChannelReader<TelemetryReading> GetReader(int partition) => _partitions[partition].Reader;

    public void Complete()
    {
        foreach (var partition in _partitions)
        {
            partition.Writer.TryComplete();
        }
    }

    /// <summary>Stable within the process is enough: partitions are not persisted. Must be non-negative.</summary>
    internal int SelectPartition(string numberPlate)
    {
        // TODO: e.g. (int)((uint)string.GetHashCode(numberPlate, StringComparison.Ordinal) % (uint)PartitionCount)
        throw new NotImplementedException();
    }
}
