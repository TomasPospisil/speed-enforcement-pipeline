using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Client.Core.Options;
using SpeedEnforcement.Client.Core.Polling;
using SpeedEnforcement.Contracts.Api;

namespace SpeedEnforcement.Client.Core.ViewModels;

/// <summary>
/// Dashboard state. All members are touched on the UI thread only (the poller marshals via IUiDispatcher),
/// so plain ObservableCollections are safe. Keeps the last snapshot so switching cameras needs no refetch.
/// </summary>
public sealed class MainViewModel : ObservableObject, ISnapshotConsumer
{
    private TrafficSnapshotDto? _lastSnapshot;
    private int _selectedCameraId;
    private DateTimeOffset? _lastUpdatedUtc;
    private string _status = "Waiting for first snapshot…";
    private bool _hasError;

    public MainViewModel(IOptions<ClientOptions> options)
    {
        CameraIds = Enumerable.Range(1, options.Value.CameraCount).ToArray();
        _selectedCameraId = CameraIds[0];
    }

    public ObservableCollection<GlobalLeaderboardRow> GlobalLeaderboard { get; } = [];

    public ObservableCollection<CameraLeaderboardRow> SelectedCameraLeaderboard { get; } = [];

    public IReadOnlyList<int> CameraIds { get; }

    public int SelectedCameraId
    {
        get => _selectedCameraId;
        set
        {
            if (SetProperty(ref _selectedCameraId, value))
            {
                RefreshSelectedCamera();
            }
        }
    }

    public DateTimeOffset? LastUpdatedUtc
    {
        get => _lastUpdatedUtc;
        private set => SetProperty(ref _lastUpdatedUtc, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    public void Apply(TrafficSnapshotDto snapshot)
    {
        _lastSnapshot = snapshot;

        Replace(GlobalLeaderboard, snapshot.GlobalLeaderboard
            .Select((e, i) => new GlobalLeaderboardRow(i + 1, e.NumberPlate, e.AverageSpeed, e.CamerasPassed)));
        RefreshSelectedCamera();

        LastUpdatedUtc = snapshot.GeneratedAtUtc;
        HasError = false;
        Status = "Connected";
    }

    public void ReportError(string message)
    {
        HasError = true;
        Status = $"Processor unreachable: {message}";
    }

    private void RefreshSelectedCamera()
    {
        var board = _lastSnapshot?.CameraLeaderboards.FirstOrDefault(b => b.CameraId == SelectedCameraId);
        Replace(SelectedCameraLeaderboard, (board?.Entries ?? [])
            .Select((e, i) => new CameraLeaderboardRow(i + 1, e.NumberPlate, e.Speed)));
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}
