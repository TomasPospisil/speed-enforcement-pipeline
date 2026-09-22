using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Generator.Options;
using SpeedEnforcement.Generator.Publishing;
using SpeedEnforcement.Generator.Simulation;
using Xunit;

namespace SpeedEnforcement.Generator.UnitTests;

/// <summary>Composition tests: the real DI graph resolves, and options validation guards startup.</summary>
public class GeneratorBuilderTests
{
    [Fact]
    public void ConfigureGenerator_ResolvesBothHostedServices()
    {
        using var host = BuildHost();

        var hostedServices = host.Services.GetServices<IHostedService>().ToArray();

        Assert.Contains(hostedServices, s => s is SimulationLoop);
        Assert.Contains(hostedServices, s => s is GrpcTelemetryPublisher);
    }

    [Fact]
    public void ConfigureGenerator_BindsOptionsFromConfiguration()
    {
        using var host = BuildHost(("Generator:VehiclesPerTick", "5"), ("Generator:CameraCount", "3"), ("Generator:OutboxCapacity", "15"));

        var options = host.Services.GetRequiredService<IOptions<GeneratorOptions>>().Value;

        Assert.Equal(5, options.VehiclesPerTick);
        Assert.Equal(3, options.CameraCount);
    }

    [Fact]
    public void ConfigureGenerator_SeededRandom_IsNotSharedInstance()
    {
        using var host = BuildHost(("Generator:RandomSeed", "42"));

        var random = host.Services.GetRequiredService<Random>();

        Assert.NotSame(Random.Shared, random);
    }

    [Theory]
    [InlineData("Generator:VehiclesPerTick", "0")]
    [InlineData("Generator:CameraCount", "0")]
    [InlineData("Generator:ProcessorAddress", "not-a-uri")]
    [InlineData("Generator:OutboxCapacity", "10")]
    public void ConfigureGenerator_InvalidOption_FailsValidation(string key, string value)
    {
        using var host = BuildHost((key, value));

        var ex = Assert.Throws<OptionsValidationException>(
            () => host.Services.GetRequiredService<IOptions<GeneratorOptions>>().Value);

        Assert.Contains(key, ex.Message, StringComparison.Ordinal);
    }

    private static IHost BuildHost(params (string Key, string Value)[] settings)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)));
        return builder.ConfigureGenerator().Build();
    }
}
