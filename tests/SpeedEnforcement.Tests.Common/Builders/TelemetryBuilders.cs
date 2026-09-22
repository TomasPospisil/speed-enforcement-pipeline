using SpeedEnforcement.Contracts.Telemetry.V1;
using SpeedEnforcement.Processor.Domain.Telemetry;

namespace SpeedEnforcement.Tests.Common.Builders;

public static class TelemetryReadingBuilder
{
    public static readonly long DefaultTicks = new DateTime(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc).Ticks;

    public static TelemetryReading Build(
        string numberPlate = "1AB2345",
        int cameraId = 1,
        float speed = 130f,
        long? timestampTicks = null)
        => new(numberPlate, timestampTicks ?? DefaultTicks, speed, cameraId);
}

public static class TelemetryMessageBuilder
{
    public static TelemetryMessage Build(
        string numberPlate = "1AB2345",
        int cameraId = 1,
        float speed = 130f,
        long? timestampTicks = null)
        => new()
        {
            NumberPlate = numberPlate,
            CameraId = cameraId,
            CurrentSpeed = speed,
            TimestampTicks = timestampTicks ?? TelemetryReadingBuilder.DefaultTicks,
        };
}
