using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Contracts.Telemetry.V1;
using SpeedEnforcement.Generator.Logs;
using SpeedEnforcement.Generator.Monitoring;
using SpeedEnforcement.Generator.Options;
using SpeedEnforcement.Generator.Publishing;

namespace SpeedEnforcement.Generator.Simulation;

/// <summary>
/// The predictable state machine from the assignment, executed once per tick in this exact order:
/// 1) influx, 2) progression + emission, 3) exit. <see cref="PeriodicTimer"/> does not accumulate drift,
/// and <see cref="TimeProvider"/> makes the loop testable with a fake clock.
/// </summary>
public sealed class SimulationLoop : BackgroundService
{
    private readonly VehicleRegistry _registry;
    private readonly IPlateGenerator _plates;
    private readonly ISpeedModel _speedModel;
    private readonly ITelemetryOutbox _outbox;
    private readonly IGeneratorMetrics _metrics;
    private readonly TimeProvider _timeProvider;
    private readonly GeneratorOptions _options;
    private readonly ILogger<SimulationLoop> _logger;

    public SimulationLoop(
        VehicleRegistry registry,
        IPlateGenerator plates,
        ISpeedModel speedModel,
        ITelemetryOutbox outbox,
        IGeneratorMetrics metrics,
        TimeProvider timeProvider,
        IOptions<GeneratorOptions> options,
        ILogger<SimulationLoop> logger)
    {
        _registry = registry;
        _plates = plates;
        _speedModel = speedModel;
        _outbox = outbox;
        _metrics = metrics;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.SimulationStarted(_options.VehiclesPerTick, _options.CameraCount, _options.TickInterval);

        using var timer = new PeriodicTimer(_options.TickInterval, _timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await TickAsync(stoppingToken);
        }
    }

    /// <summary>One simulation second. Internal so tests can drive it without the timer.</summary>
    internal async Task TickAsync(CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();

        // 1. New vehicle influx.
        _registry.Spawn(_options.VehiclesPerTick, _plates);

        // 2. Sequential camera progression & emission.
        //    A vehicle advancing past the last camera gets no message: the assignment fixes the lifespan at
        //    exactly CameraCount messages per vehicle.
        var emitted = 0;
        foreach (var vehicle in _registry.Active)
        {
            vehicle.Advance(_speedModel.Next(vehicle));
            if (vehicle.CameraCounter > _options.CameraCount)
            {
                continue;
            }

            await _outbox.WriteAsync(new TelemetryMessage
            {
                NumberPlate = vehicle.NumberPlate,
                TimestampTicks = now.UtcTicks,
                CurrentSpeed = vehicle.LastSpeed,
                CameraId = vehicle.CameraCounter,
            }, cancellationToken);
            emitted++;
        }

        // 3. Vehicle exit (cleanup).
        var exited = _registry.RemoveExited(_options.CameraCount);

        _metrics.TickCompleted(_registry.Count, emitted, exited);
        _logger.TickCompleted(_registry.Count, emitted, exited);
    }
}
