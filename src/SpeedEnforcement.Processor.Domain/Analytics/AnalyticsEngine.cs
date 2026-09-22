using System.Collections.Concurrent;
using SpeedEnforcement.Processor.Domain.Leaderboards;
using SpeedEnforcement.Processor.Domain.Telemetry;
using SpeedEnforcement.Processor.Domain.Tracking;

namespace SpeedEnforcement.Processor.Domain.Analytics;

/// <summary>
/// Orchestrates one reading: track -> camera board -> gate -> global board -> purge at the last camera.
/// Owns the only mutable registry (<see cref="_tracks"/>); leaderboards guard themselves.
/// </summary>
public sealed class AnalyticsEngine : IAnalyticsEngine
{
    private readonly ConcurrentDictionary<string, VehicleTrack> _tracks = new(StringComparer.Ordinal);
    private readonly CameraLeaderboardRegistry _cameraBoards;
    private readonly GlobalLeaderboard _globalBoard;
    private readonly int _lastCameraId;

    public AnalyticsEngine(AnalyticsOptions options)
    {
        _cameraBoards = new CameraLeaderboardRegistry(options);
        _globalBoard = new GlobalLeaderboard(options);
        _lastCameraId = options.CameraCount;
    }

    public int ActiveVehicles => _tracks.Count;

    public void Apply(in TelemetryReading reading)
    {
        // TODO:
        //   var track = _tracks.GetOrAdd(reading.NumberPlate, static plate => new VehicleTrack(plate));
        //   track.Record(reading.Speed);
        //   _cameraBoards.For(reading.CameraId).Upsert(new CameraLeaderboardEntry(reading.NumberPlate, reading.Speed));
        //   _globalBoard.Consider(track);
        //   if (reading.CameraId == _lastCameraId) _tracks.TryRemove(reading.NumberPlate, out _);
        _ = _lastCameraId;
        throw new NotImplementedException();
    }

    public AnalyticsSnapshot Snapshot() => new(_cameraBoards.Snapshot(), _globalBoard.Snapshot());
}
