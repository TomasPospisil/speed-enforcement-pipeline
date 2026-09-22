using Microsoft.AspNetCore.Http.HttpResults;
using SpeedEnforcement.Contracts.Api;
using SpeedEnforcement.Processor.Domain.Analytics;

namespace SpeedEnforcement.Processor.Host.Api;

public static class TrafficSnapshotEndpoint
{
    public static IEndpointRouteBuilder MapTrafficSnapshot(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet(TrafficApiRoutes.TrafficSnapshot, GetSnapshot)
            .WithName("GetTrafficSnapshot")
            .WithSummary("Current per-camera Top-N and global average-speed Top-N with a UTC generation timestamp.");

        return endpoints;
    }

    private static Ok<TrafficSnapshotDto> GetSnapshot(
        IAnalyticsEngine engine,
        ISnapshotDtoMapper mapper,
        TimeProvider timeProvider)
        => TypedResults.Ok(mapper.Map(engine.Snapshot(), timeProvider.GetUtcNow()));
}
