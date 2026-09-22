namespace SpeedEnforcement.Processor.Host.Logs;

/// <summary>Source-generated, structured log messages. Event ids: 1xxx ingest stream, 2xxx worker.</summary>
internal static partial class ProcessorLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "Telemetry stream opened by {Peer}")]
    public static partial void StreamOpened(this ILogger logger, string peer);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Telemetry stream from {Peer} completed: {Accepted} accepted, {Rejected} rejected")]
    public static partial void StreamCompleted(this ILogger logger, string peer, long accepted, long rejected);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning,
        Message = "Telemetry stream from {Peer} aborted after {Accepted} accepted / {Rejected} rejected; last reason {Reason}")]
    public static partial void StreamAborted(this ILogger logger, string peer, long accepted, long rejected, string reason);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Debug,
        Message = "Rejected telemetry for plate {NumberPlate} at camera {CameraId}: {Reason}")]
    public static partial void TelemetryRejected(this ILogger logger, string numberPlate, int cameraId, string reason);

    [LoggerMessage(EventId = 2000, Level = LogLevel.Information,
        Message = "Ingest worker started with {PartitionCount} partitions")]
    public static partial void IngestWorkerStarted(this ILogger logger, int partitionCount);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Error,
        Message = "Failed to apply reading for plate {NumberPlate} at camera {CameraId} on partition {Partition}")]
    public static partial void ReadingFailed(this ILogger logger, Exception exception, string numberPlate, int cameraId, int partition);
}
