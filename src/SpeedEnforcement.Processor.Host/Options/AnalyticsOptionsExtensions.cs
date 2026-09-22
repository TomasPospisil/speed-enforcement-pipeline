using Microsoft.Extensions.Options;
using SpeedEnforcement.Processor.Domain;

namespace SpeedEnforcement.Processor.Host.Options;

public static class AnalyticsOptionsExtensions
{
    public const string SectionName = "Analytics";

    public static IServiceCollection ConfigureAnalyticsOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AnalyticsOptions>()
            .Bind(configuration.GetSection(SectionName))
            .Validate(o => o.CameraCount is > 0 and <= 100,
                "Analytics:CameraCount must be in (0, 100].")
            .Validate(o => o.LeaderboardCapacity is > 0 and <= 1_000,
                "Analytics:LeaderboardCapacity must be in (0, 1000].")
            .Validate(o => o.GlobalLeaderboardGate > 0 && o.GlobalLeaderboardGate <= o.CameraCount,
                "Analytics:GlobalLeaderboardGate must be in (0, CameraCount].")
            .ValidateOnStart();

        // The domain takes the plain options object: it must not depend on Microsoft.Extensions.Options.
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AnalyticsOptions>>().Value);

        return services;
    }
}
