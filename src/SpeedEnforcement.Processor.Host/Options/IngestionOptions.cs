namespace SpeedEnforcement.Processor.Host.Options;

/// <summary>Bound from the "Ingestion" section; every key is overridable via env var <c>Ingestion__Key</c>.</summary>
public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    /// <summary>Number of channels/consumers. Same plate always lands in the same partition.</summary>
    public int PartitionCount { get; set; } = Environment.ProcessorCount;

    /// <summary>Per-partition buffer. Full partition = the gRPC stream waits (backpressure).</summary>
    public int ChannelCapacity { get; set; } = 10_000;

    /// <summary>ADR-009: abort the stream after this many rejections in a row.</summary>
    public int RejectionAbortConsecutive { get; set; } = 10;

    /// <summary>ADR-009: abort the stream when rejected/total over the window exceeds this ratio.</summary>
    public double RejectionAbortRatio { get; set; } = 0.05;

    public int RejectionWindowSize { get; set; } = 1_000;
}
