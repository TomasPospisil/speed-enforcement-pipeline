using System.Threading.Channels;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Contracts.Telemetry.V1;
using SpeedEnforcement.Generator.Monitoring;
using SpeedEnforcement.Generator.Options;

namespace SpeedEnforcement.Generator.Publishing;

/// <summary>
/// ADR-010: decouples the 1 s simulation tick from network latency and reconnects. The simulation must never
/// block on the processor; the publisher must never touch simulation state.
/// </summary>
public interface ITelemetryOutbox
{
    ValueTask WriteAsync(TelemetryMessage message, CancellationToken cancellationToken);

    ChannelReader<TelemetryMessage> Reader { get; }

    int Pending { get; }
}

/// <summary>
/// Bounded channel with <see cref="BoundedChannelFullMode.DropOldest"/>: when the processor is down for long,
/// stale positions are worthless, so we drop the oldest and count the drops instead of stalling the simulation.
/// </summary>
public sealed class ChannelTelemetryOutbox : ITelemetryOutbox
{
    private readonly Channel<TelemetryMessage> _channel;

    public ChannelTelemetryOutbox(IOptions<GeneratorOptions> options, IGeneratorMetrics metrics)
    {
        _channel = Channel.CreateBounded<TelemetryMessage>(
            new BoundedChannelOptions(options.Value.OutboxCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true,
            },
            itemDropped: _ => metrics.TelemetryDropped());
    }

    public ValueTask WriteAsync(TelemetryMessage message, CancellationToken cancellationToken)
        => _channel.Writer.WriteAsync(message, cancellationToken);

    public ChannelReader<TelemetryMessage> Reader => _channel.Reader;

    public int Pending => _channel.Reader.Count;
}
