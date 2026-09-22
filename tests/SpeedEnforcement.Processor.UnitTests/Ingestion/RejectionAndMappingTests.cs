using Xunit;

namespace SpeedEnforcement.Processor.UnitTests.Ingestion;

public class ThresholdRejectionPolicyTests
{
    private const string Todo = "Skeleton: implement ThresholdRejectionPolicy first (ADR-009).";

    [Fact(Skip = Todo)]
    public void RecordRejection_BelowBothThresholds_DoesNotAbort() { }

    [Fact(Skip = Todo)]
    public void RecordRejection_ConsecutiveLimitReached_Aborts() { }

    [Fact(Skip = Todo)]
    public void RecordAccepted_ResetsConsecutiveCounter() { }

    [Fact(Skip = Todo)]
    public void RecordRejection_RatioOverWindowExceeded_Aborts() { }
}

public class TelemetryMapperTests
{
    private const string Todo = "Skeleton: implement TelemetryMapper.TryMap first.";

    [Fact(Skip = Todo)]
    public void TryMap_ValidMessage_ProducesReadingWithSameValues() { }

    [Fact(Skip = Todo)]
    public void TryMap_EmptyPlate_RejectsWithEmptyPlateReason() { }

    [Fact(Skip = Todo)]
    public void TryMap_CameraOutOfRange_RejectsWithCameraOutOfRangeReason() { }

    [Fact(Skip = Todo)]
    public void TryMap_NonPositiveOrNaNSpeed_RejectsWithInvalidSpeedReason() { }
}
