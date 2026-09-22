using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using SpeedEnforcement.Client.Core.Options;
using SpeedEnforcement.Client.Core.Polling;
using SpeedEnforcement.Tests.Common.Builders;
using SpeedEnforcement.Tests.Common.Fakes;
using Xunit;

namespace SpeedEnforcement.Client.UnitTests;

/// <summary>The 3-second contract, verified with a fake clock: no real waiting, no flakiness.</summary>
public class SnapshotPollerTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(3);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 9, 22, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeTrafficSnapshotClient _client = new();
    private readonly RecordingSnapshotConsumer _consumer = new();
    private readonly ImmediateUiDispatcher _dispatcher = new();
    private readonly FakeLogger<SnapshotPoller> _logger = new();
    private readonly SnapshotPoller _poller;

    public SnapshotPollerTests()
    {
        _poller = new SnapshotPoller(
            _client,
            _consumer,
            _dispatcher,
            _time,
            Options.Create(new ClientOptions { PollInterval = Interval }),
            _logger);
    }

    [Fact]
    public async Task ExecuteAsync_FetchesExactlyOncePerInterval()
    {
        var ct = TestContext.Current.CancellationToken;
        await _poller.StartAsync(ct);
        // ExecuteAsync runs on another thread (.NET 10); the "polling started" log entry is written right
        // after the timer is registered with the fake clock, so only then is advancing the clock meaningful.
        await WaitUntilAsync(() => _logger.Collector.Count > 0, ct);

        _time.Advance(TimeSpan.FromSeconds(2.9));
        Assert.Equal(0, _client.Calls);

        _time.Advance(TimeSpan.FromSeconds(0.1));
        await WaitUntilAsync(() => _client.Calls == 1, ct);

        _time.Advance(Interval);
        await WaitUntilAsync(() => _client.Calls == 2, ct);

        await _poller.StopAsync(ct);
        Assert.Equal(2, _consumer.Snapshots.Count);
    }

    [Fact]
    public async Task PollOnce_Success_MarshalsSnapshotThroughUiDispatcher()
    {
        var expected = TrafficSnapshotDtoBuilder.CreateDefault().WithGlobalEntry("1AB2345", 150f, 4).Build();
        _client.Next = () => expected;

        await _poller.PollOnceAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, _dispatcher.Invocations);
        Assert.Same(expected, Assert.Single(_consumer.Snapshots));
        Assert.Empty(_consumer.Errors);
    }

    [Fact]
    public async Task PollOnce_ClientThrows_ReportsErrorOnUiThreadAndLogsWarning()
    {
        _client.Next = () => throw new HttpRequestException("connection refused");

        await _poller.PollOnceAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, _dispatcher.Invocations);
        Assert.Equal("connection refused", Assert.Single(_consumer.Errors));
        Assert.Single(_logger.Collector.GetSnapshot(), r => r.Level == LogLevel.Warning && r.Id.Id == 1001);
    }

    private async Task WaitUntilAsync(Func<bool> condition, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline,
                $"Condition not met within 5 s. Poller task: {_poller.ExecuteTask?.Status}, fake clock {_time.GetUtcNow():O}.");
            await Task.Delay(10, cancellationToken);
        }
    }
}
