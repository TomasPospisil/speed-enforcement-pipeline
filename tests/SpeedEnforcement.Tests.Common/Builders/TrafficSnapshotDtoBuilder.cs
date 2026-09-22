using SpeedEnforcement.Contracts.Api;

namespace SpeedEnforcement.Tests.Common.Builders;

/// <summary>Fluent builder for the REST DTO. Defaults to 10 empty camera boards and an empty global board.</summary>
public sealed class TrafficSnapshotDtoBuilder
{
    private readonly Dictionary<int, List<CameraLeaderboardEntryDto>> _cameraEntries = new();
    private readonly List<GlobalLeaderboardEntryDto> _globalEntries = new();
    private DateTimeOffset _generatedAtUtc = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);
    private int _cameraCount = 10;

    public static TrafficSnapshotDtoBuilder CreateDefault() => new();

    public TrafficSnapshotDtoBuilder GeneratedAt(DateTimeOffset generatedAtUtc)
    {
        _generatedAtUtc = generatedAtUtc;
        return this;
    }

    public TrafficSnapshotDtoBuilder WithCameraCount(int cameraCount)
    {
        _cameraCount = cameraCount;
        return this;
    }

    public TrafficSnapshotDtoBuilder WithCameraEntry(int cameraId, string numberPlate, float speed)
    {
        if (!_cameraEntries.TryGetValue(cameraId, out var entries))
        {
            entries = [];
            _cameraEntries[cameraId] = entries;
        }

        entries.Add(new CameraLeaderboardEntryDto(numberPlate, speed));
        return this;
    }

    public TrafficSnapshotDtoBuilder WithGlobalEntry(string numberPlate, float averageSpeed, int camerasPassed)
    {
        _globalEntries.Add(new GlobalLeaderboardEntryDto(numberPlate, averageSpeed, camerasPassed));
        return this;
    }

    public TrafficSnapshotDto Build() => new(
        _generatedAtUtc,
        Enumerable.Range(1, _cameraCount)
            .Select(id => new CameraLeaderboardDto(id, _cameraEntries.GetValueOrDefault(id) ?? []))
            .ToArray(),
        _globalEntries.ToArray());
}
