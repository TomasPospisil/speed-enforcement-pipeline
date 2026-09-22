# ADR-006: WPF on .NET 10 with view models outside the WPF project

**Status:** accepted

## Decision
`Client.Core` (no WPF reference): typed `HttpClient`, `SnapshotPoller` (BackgroundService, `PeriodicTimer(3 s, TimeProvider)`), `MainViewModel`, `IUiDispatcher`. `Client.Wpf`: Generic Host in `App.xaml.cs`, `WpfDispatcher`, XAML views. MVVM base types from CommunityToolkit.Mvvm.

## Why
- Polling and view-model logic run in unit tests with a fake clock and an inline dispatcher; the 3-second contract is asserted without waiting.
- Same DI/options/logging/hosted-service model as the backend.
- The split is the prerequisite for sustainable UI automation (FlaUI / WinAppDriver) later.
- Target `net10.0-windows`; contracts are plain DTOs, so a .NET Framework 4.8 client could share them via a netstandard2.0 build if ever needed.
