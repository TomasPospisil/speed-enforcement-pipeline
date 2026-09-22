using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Contracts.Telemetry.V1;
using SpeedEnforcement.Generator.Monitoring;
using SpeedEnforcement.Generator.Options;
using SpeedEnforcement.Generator.Publishing;
using SpeedEnforcement.Generator.Simulation;

namespace SpeedEnforcement.Generator;

/// <summary>
/// Composition root. Two hosted services: <see cref="SimulationLoop"/> ticks the state machine once per second,
/// <see cref="GrpcTelemetryPublisher"/> drains the outbox into the gRPC stream (ADR-010 keeps them independent).
/// </summary>
public static class GeneratorBuilder
{
    public static HostApplicationBuilder ConfigureGenerator(this HostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services
            .AddSingleton(TimeProvider.System)
            .ConfigureGeneratorOptions(configuration)
            // Seeded Random makes a run reproducible (tests, bug reports); unseeded uses the shared instance.
            .AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<GeneratorOptions>>().Value;
                return options.RandomSeed is { } seed ? new Random(seed) : Random.Shared;
            })
            .AddSingleton<IPlateGenerator, SequentialPlateGenerator>()
            .AddSingleton<ISpeedModel, RandomWalkSpeedModel>()
            .AddSingleton<VehicleRegistry>()
            .AddSingleton<ITelemetryOutbox, ChannelTelemetryOutbox>()
            .AddSingleton<IBackoffPolicy, ExponentialBackoff>()
            .AddSingleton<IGeneratorMetrics, GeneratorMetrics>()
            .AddHostedService<SimulationLoop>()
            .AddHostedService<GrpcTelemetryPublisher>();

        services
            .AddGrpcClient<TelemetryIngest.TelemetryIngestClient>((sp, o) =>
                o.Address = new Uri(sp.GetRequiredService<IOptions<GeneratorOptions>>().Value.ProcessorAddress))
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // Detect a dead processor even while the outbox is quiet.
                KeepAlivePingDelay = TimeSpan.FromSeconds(30),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
                KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
            });

        return builder;
    }
}
