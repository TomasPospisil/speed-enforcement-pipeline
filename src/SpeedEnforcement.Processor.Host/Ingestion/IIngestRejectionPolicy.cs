using Microsoft.Extensions.Options;
using SpeedEnforcement.Processor.Host.Options;

namespace SpeedEnforcement.Processor.Host.Ingestion;

/// <summary>
/// ADR-009: a client stream has no per-message back channel, only the terminal status. Isolated invalid
/// messages are tolerated (logged + counted); a systematic error rate aborts the stream so the producer
/// learns about it immediately instead of after hours.
/// </summary>
public interface IIngestRejectionPolicy
{
    /// <returns><c>true</c> when the stream should be aborted with <c>InvalidArgument</c>.</returns>
    bool RecordRejection(string reason);

    void RecordAccepted();
}

/// <summary>Aborts after N consecutive rejections or when the rejection ratio over a sliding window exceeds a limit.</summary>
public sealed class ThresholdRejectionPolicy : IIngestRejectionPolicy
{
    private readonly IngestionOptions _options;

    public ThresholdRejectionPolicy(IOptions<IngestionOptions> options)
    {
        _options = options.Value;
    }

    public bool RecordRejection(string reason)
    {
        // TODO: consecutive counter + ring buffer of last RejectionWindowSize outcomes;
        //       abort when consecutive >= RejectionAbortConsecutive or ratio > RejectionAbortRatio.
        _ = _options;
        throw new NotImplementedException();
    }

    public void RecordAccepted()
    {
        throw new NotImplementedException();
    }
}
