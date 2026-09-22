using SpeedEnforcement.Contracts.Api;

namespace SpeedEnforcement.Client.Core.Polling;

/// <summary>Receives poll results on the UI thread. Implemented by the main view model; faked in tests.</summary>
public interface ISnapshotConsumer
{
    void Apply(TrafficSnapshotDto snapshot);

    void ReportError(string message);
}
