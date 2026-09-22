namespace SpeedEnforcement.Contracts.Api;

/// <summary>Payload of <c>GET /api/traffic-snapshot</c>. Serialized with System.Text.Json (camelCase).</summary>
public sealed record TrafficSnapshotDto(
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<CameraLeaderboardDto> CameraLeaderboards,
    IReadOnlyList<GlobalLeaderboardEntryDto> GlobalLeaderboard);

public sealed record CameraLeaderboardDto(
    int CameraId,
    IReadOnlyList<CameraLeaderboardEntryDto> Entries);

/// <summary>Fastest vehicles at one camera, ordered by <see cref="Speed"/> desc, then <see cref="NumberPlate"/> asc.</summary>
public sealed record CameraLeaderboardEntryDto(
    string NumberPlate,
    float Speed);

/// <summary>Highest cumulative average speed, ordered by <see cref="AverageSpeed"/> desc, then <see cref="NumberPlate"/> asc.</summary>
public sealed record GlobalLeaderboardEntryDto(
    string NumberPlate,
    float AverageSpeed,
    int CamerasPassed);
