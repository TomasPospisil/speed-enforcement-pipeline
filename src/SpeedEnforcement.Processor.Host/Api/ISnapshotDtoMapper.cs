using SpeedEnforcement.Contracts.Api;
using SpeedEnforcement.Processor.Domain.Analytics;

namespace SpeedEnforcement.Processor.Host.Api;

/// <summary>Domain snapshot -> REST DTO. Keeps the wire shape out of the domain and the domain types out of the API.</summary>
public interface ISnapshotDtoMapper
{
    TrafficSnapshotDto Map(AnalyticsSnapshot snapshot, DateTimeOffset generatedAtUtc);
}

public sealed class SnapshotDtoMapper : ISnapshotDtoMapper
{
    public TrafficSnapshotDto Map(AnalyticsSnapshot snapshot, DateTimeOffset generatedAtUtc)
    {
        var cameraBoards = snapshot.CameraLeaderboards
            .Select((board, index) => new CameraLeaderboardDto(
                CameraId: index + 1,
                Entries: board.Select(e => new CameraLeaderboardEntryDto(e.NumberPlate, e.Speed)).ToArray()))
            .ToArray();

        var globalBoard = snapshot.GlobalLeaderboard
            .Select(e => new GlobalLeaderboardEntryDto(e.NumberPlate, e.AverageSpeed, e.CamerasPassed))
            .ToArray();

        return new TrafficSnapshotDto(generatedAtUtc, cameraBoards, globalBoard);
    }
}
