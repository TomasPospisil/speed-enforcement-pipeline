using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpeedEnforcement.Client.Core;
using SpeedEnforcement.Client.Core.Threading;
using SpeedEnforcement.Client.Wpf.Threading;
using SpeedEnforcement.Client.Wpf.Views;

namespace SpeedEnforcement.Client.Wpf;

/// <summary>
/// Hosts the desktop app inside the Generic Host: same DI, options, logging and hosted-service model as the
/// backend. The poller is a BackgroundService started by the host, not a timer hidden in a window.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = Host.CreateApplicationBuilder(e.Args);
        builder.Services
            .ConfigureClientCore(builder.Configuration)
            .AddSingleton<IUiDispatcher, WpfDispatcher>()
            .AddSingleton<MainWindow>();

        _host = builder.Build();
        await _host.StartAsync();

        _host.Services.GetRequiredService<MainWindow>().Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
