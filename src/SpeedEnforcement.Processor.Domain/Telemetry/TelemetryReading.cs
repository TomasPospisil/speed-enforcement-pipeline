namespace SpeedEnforcement.Processor.Domain.Telemetry;

/// <summary>Validated, transport-agnostic telemetry sample. Produced by the host's mapper from the wire message.</summary>
public readonly record struct TelemetryReading(
    string NumberPlate,
    long TimestampTicks,
    float Speed,
    int CameraId);
