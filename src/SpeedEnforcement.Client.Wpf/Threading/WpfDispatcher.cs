using System.Windows;
using SpeedEnforcement.Client.Core.Threading;

namespace SpeedEnforcement.Client.Wpf.Threading;

/// <summary>The only place in the client that knows about the WPF Dispatcher.</summary>
public sealed class WpfDispatcher : IUiDispatcher
{
    public Task InvokeAsync(Action action)
        => Application.Current.Dispatcher.InvokeAsync(action).Task;
}
