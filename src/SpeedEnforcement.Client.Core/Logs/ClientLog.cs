using Microsoft.Extensions.Logging;

namespace SpeedEnforcement.Client.Core.Logs;

internal static partial class ClientLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information,
        Message = "Polling {ProcessorBaseUrl} every {PollInterval}")]
    public static partial void PollingStarted(this ILogger logger, TimeSpan pollInterval, string processorBaseUrl);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning,
        Message = "Snapshot poll failed; will retry on next tick")]
    public static partial void PollFailed(this ILogger logger, Exception exception);
}
