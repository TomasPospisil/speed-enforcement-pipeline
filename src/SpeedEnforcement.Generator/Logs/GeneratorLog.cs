using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace SpeedEnforcement.Generator.Logs;

/// <summary>Source-generated structured logging. Event ids: 1xxx simulation, 2xxx publishing.</summary>
internal static partial class GeneratorLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "Simulation started: {VehiclesPerTick} vehicles/tick, {CameraCount} cameras, tick {TickInterval}")]
    public static partial void SimulationStarted(this ILogger logger, int vehiclesPerTick, int cameraCount, TimeSpan tickInterval);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Debug,
        Message = "Tick completed: {ActiveVehicles} active, {Emitted} emitted, {Exited} exited")]
    public static partial void TickCompleted(this ILogger logger, int activeVehicles, int emitted, int exited);

    [LoggerMessage(EventId = 2000, Level = LogLevel.Information,
        Message = "Telemetry stream opened")]
    public static partial void StreamOpened(this ILogger logger);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information,
        Message = "Telemetry stream completed: {Accepted} accepted, {Rejected} rejected")]
    public static partial void StreamCompleted(this ILogger logger, long accepted, long rejected);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "Telemetry stream failed with {StatusCode}; reconnecting in {Delay}")]
    public static partial void StreamFailed(this ILogger logger, Exception exception, StatusCode statusCode, TimeSpan delay);
}
