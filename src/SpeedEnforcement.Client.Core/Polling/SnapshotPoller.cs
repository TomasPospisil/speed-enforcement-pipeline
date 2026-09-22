using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Client.Core.Api;
using SpeedEnforcement.Client.Core.Logs;
using SpeedEnforcement.Client.Core.Options;
using SpeedEnforcement.Client.Core.Threading;

namespace SpeedEnforcement.Client.Core.Polling;

/// <summary>
/// Background polling loop at a fixed interval (3 s per the assignment). Fetches off the UI thread and
/// marshals only the final state update through <see cref="IUiDispatcher"/>.
/// <see cref="PeriodicTimer"/> ticks from the start of the previous tick, so a slow fetch does not shift
/// the schedule, and it never runs two fetches concurrently (a missed tick is coalesced).
/// </summary>
public sealed class SnapshotPoller : BackgroundService
{
    private readonly ITrafficSnapshotClient _client;
    private readonly ISnapshotConsumer _consumer;
    private readonly IUiDispatcher _dispatcher;
    private readonly TimeProvider _timeProvider;
    private readonly ClientOptions _options;
    private readonly ILogger<SnapshotPoller> _logger;

    public SnapshotPoller(
        ITrafficSnapshotClient client,
        ISnapshotConsumer consumer,
        IUiDispatcher dispatcher,
        TimeProvider timeProvider,
        IOptions<ClientOptions> options,
        ILogger<SnapshotPoller> logger)
    {
        _client = client;
        _consumer = consumer;
        _dispatcher = dispatcher;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Timer first, log second: the log record is the observable signal that the schedule exists.
        // (.NET 10 BackgroundService starts ExecuteAsync on a separate thread, so callers cannot assume
        // the timer is registered when StartAsync returns; tests wait for this log entry.)
        using var timer = new PeriodicTimer(_options.PollInterval, _timeProvider);
        _logger.PollingStarted(_options.PollInterval, _options.ProcessorBaseUrl);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PollOnceAsync(stoppingToken);
        }
    }

    internal async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await _client.GetSnapshotAsync(cancellationToken);
            await _dispatcher.InvokeAsync(() => _consumer.Apply(snapshot));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // The dashboard stays up and shows the error; the next tick retries.
            _logger.PollFailed(ex);
            await _dispatcher.InvokeAsync(() => _consumer.ReportError(ex.Message));
        }
    }
}
