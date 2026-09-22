namespace SpeedEnforcement.Client.Core.Threading;

/// <summary>Abstraction over the UI thread. WPF implements it with <c>Application.Current.Dispatcher</c>; tests run inline.</summary>
public interface IUiDispatcher
{
    Task InvokeAsync(Action action);
}
