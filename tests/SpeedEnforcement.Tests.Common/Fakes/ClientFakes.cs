using SpeedEnforcement.Client.Core.Api;
using SpeedEnforcement.Client.Core.Polling;
using SpeedEnforcement.Client.Core.Threading;
using SpeedEnforcement.Contracts.Api;
using SpeedEnforcement.Tests.Common.Builders;

namespace SpeedEnforcement.Tests.Common.Fakes;

/// <summary>Runs the action inline; there is no UI thread in tests.</summary>
public sealed class ImmediateUiDispatcher : IUiDispatcher
{
    public int Invocations { get; private set; }

    public Task InvokeAsync(Action action)
    {
        Invocations++;
        action();
        return Task.CompletedTask;
    }
}

public sealed class FakeTrafficSnapshotClient : ITrafficSnapshotClient
{
    private int _calls;

    public int Calls => Volatile.Read(ref _calls);

    /// <summary>Producer of the next response; throw from it to simulate a failing processor.</summary>
    public Func<TrafficSnapshotDto> Next { get; set; } = () => TrafficSnapshotDtoBuilder.CreateDefault().Build();

    public Task<TrafficSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _calls);
        return Task.FromResult(Next());
    }
}

public sealed class RecordingSnapshotConsumer : ISnapshotConsumer
{
    public List<TrafficSnapshotDto> Snapshots { get; } = [];

    public List<string> Errors { get; } = [];

    public void Apply(TrafficSnapshotDto snapshot) => Snapshots.Add(snapshot);

    public void ReportError(string message) => Errors.Add(message);
}
