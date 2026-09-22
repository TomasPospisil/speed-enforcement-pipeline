using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SpeedEnforcement.Client.Core.Api;
using SpeedEnforcement.Client.Core.Options;
using SpeedEnforcement.Client.Core.Polling;
using SpeedEnforcement.Client.Core.ViewModels;

namespace SpeedEnforcement.Client.Core;

/// <summary>
/// Composition of everything UI-framework-agnostic. The WPF host adds <c>IUiDispatcher</c> and the views;
/// tests add a fake dispatcher and a fake clock.
/// </summary>
public static class ClientCoreBuilder
{
    public static IServiceCollection ConfigureClientCore(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton(TimeProvider.System)
            .ConfigureClientOptions(configuration)
            .AddSingleton<MainViewModel>()
            .AddSingleton<ISnapshotConsumer>(sp => sp.GetRequiredService<MainViewModel>())
            .AddHostedService<SnapshotPoller>();

        services
            .AddHttpClient<ITrafficSnapshotClient, TrafficSnapshotClient>((sp, http) =>
            {
                var options = sp.GetRequiredService<IOptions<ClientOptions>>().Value;
                http.BaseAddress = new Uri(options.ProcessorBaseUrl);
                // A poll that outlives its interval is worthless; fail fast and let the next tick retry.
                http.Timeout = options.PollInterval;
            })
            .AddStandardResilienceHandler();

        return services;
    }
}
