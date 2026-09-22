using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Processor.Domain;
using SpeedEnforcement.Processor.Host.Options;
using Xunit;

namespace SpeedEnforcement.Processor.UnitTests.Options;

public class OptionsValidationTests
{
    [Theory]
    [InlineData("Ingestion:PartitionCount", "0")]
    [InlineData("Ingestion:PartitionCount", "65")]
    [InlineData("Ingestion:ChannelCapacity", "10")]
    [InlineData("Ingestion:RejectionAbortRatio", "1.5")]
    public void IngestionOptions_OutOfRange_FailsValidation(string key, string value)
    {
        var provider = Build((key, value));

        var ex = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<IngestionOptions>>().Value);

        Assert.Contains(key, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnalyticsOptions_GateAboveCameraCount_FailsValidation()
    {
        var provider = Build(("Analytics:CameraCount", "5"), ("Analytics:GlobalLeaderboardGate", "6"));

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AnalyticsOptions>>().Value);
    }

    [Fact]
    public void AnalyticsOptions_PlainInstance_IsRegisteredForTheDomain()
    {
        var provider = Build(("Analytics:CameraCount", "7"));

        var options = provider.GetRequiredService<AnalyticsOptions>();

        Assert.Equal(7, options.CameraCount);
    }

    private static ServiceProvider Build(params (string Key, string Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)))
            .Build();

        return new ServiceCollection()
            .ConfigureIngestionOptions(configuration)
            .ConfigureAnalyticsOptions(configuration)
            .BuildServiceProvider();
    }
}
