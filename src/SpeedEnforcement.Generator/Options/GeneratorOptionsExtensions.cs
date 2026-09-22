using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SpeedEnforcement.Generator.Options;

public static class GeneratorOptionsExtensions
{
    public static IServiceCollection ConfigureGeneratorOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GeneratorOptions>()
            .Bind(configuration.GetSection(GeneratorOptions.SectionName))
            .Validate(o => Uri.TryCreate(o.ProcessorAddress, UriKind.Absolute, out _),
                "Generator:ProcessorAddress must be an absolute URI (e.g. http://localhost:5001).")
            .Validate(o => o.VehiclesPerTick is > 0 and <= 100_000,
                "Generator:VehiclesPerTick must be in (0, 100000].")
            .Validate(o => o.CameraCount is > 0 and <= 100,
                "Generator:CameraCount must be in (0, 100].")
            .Validate(o => o.TickInterval > TimeSpan.Zero,
                "Generator:TickInterval must be positive.")
            .Validate(o => o.OutboxCapacity >= o.VehiclesPerTick * o.CameraCount,
                "Generator:OutboxCapacity must hold at least one full sector (VehiclesPerTick x CameraCount).")
            .Validate(o => o.MinSpeedKmh > 0 && o.MinSpeedKmh < o.BaselineSpeedKmh && o.BaselineSpeedKmh < o.MaxSpeedKmh,
                "Generator speeds must satisfy 0 < MinSpeedKmh < BaselineSpeedKmh < MaxSpeedKmh.")
            .Validate(o => o.SpeedStdDevKmh >= 0 && o.MaxSpeedStepKmh >= 0,
                "Generator:SpeedStdDevKmh and MaxSpeedStepKmh must be non-negative.")
            .ValidateOnStart();

        return services;
    }
}
