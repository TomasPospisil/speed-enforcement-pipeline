using SpeedEnforcement.Contracts.Telemetry.V1;
using SpeedEnforcement.Processor.Domain.Telemetry;

namespace SpeedEnforcement.Processor.Host.Ingestion;

/// <summary>Wire message -> validated domain reading. The only place that knows both shapes.</summary>
public static class TelemetryMapper
{
    public static class RejectionReason
    {
        public const string EmptyPlate = "empty_plate";
        public const string CameraOutOfRange = "camera_out_of_range";
        public const string InvalidSpeed = "invalid_speed";
    }

    public static bool TryMap(TelemetryMessage message, int cameraCount, out TelemetryReading reading, out string reason)
    {
        // TODO: plate non-empty; 1 <= CameraId <= cameraCount; speed finite and > 0.
        //       reason values are low-cardinality on purpose: they become a metric tag.
        throw new NotImplementedException();
    }
}
