namespace SpeedEnforcement.Processor.Host.Options;

public static class IngestionOptionsExtensions
{
    // Validation runs at startup (ValidateOnStart): a misconfigured deployment fails fast with a readable
    // message instead of running with a silently clamped or default value.
    public static IServiceCollection ConfigureIngestionOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IngestionOptions>()
            .Bind(configuration.GetSection(IngestionOptions.SectionName))
            .Validate(o => o.PartitionCount is > 0 and <= 64,
                "Ingestion:PartitionCount must be in (0, 64].")
            .Validate(o => o.ChannelCapacity is >= 100 and <= 1_000_000,
                "Ingestion:ChannelCapacity must be in [100, 1000000].")
            .Validate(o => o.RejectionAbortConsecutive > 0,
                "Ingestion:RejectionAbortConsecutive must be positive.")
            .Validate(o => o.RejectionAbortRatio is > 0 and <= 1,
                "Ingestion:RejectionAbortRatio must be in (0, 1].")
            .Validate(o => o.RejectionWindowSize >= 10,
                "Ingestion:RejectionWindowSize must be at least 10.")
            .ValidateOnStart();

        return services;
    }
}
