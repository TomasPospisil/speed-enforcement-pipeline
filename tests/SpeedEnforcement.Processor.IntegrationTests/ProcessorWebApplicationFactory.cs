using System.Collections.Immutable;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using SpeedEnforcement.Processor.Domain.Analytics;
using SpeedEnforcement.Processor.Domain.Leaderboards;
using SpeedEnforcement.Processor.Domain.Telemetry;

namespace SpeedEnforcement.Processor.IntegrationTests;

/// <summary>
/// Boots the real composition (Program + ProcessorBuilder) on an in-memory TestServer. Only the analytics
/// engine and the clock are swapped, so routing, DI, options validation, health checks and JSON shape are
/// exercised for real.
/// </summary>
public sealed class ProcessorWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly DateTimeOffset FrozenNow = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);

    public FakeAnalyticsEngine Engine { get; } = new();

    public FakeTimeProvider TimeProvider { get; } = new(FrozenNow);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAnalyticsEngine>();
            services.AddSingleton<IAnalyticsEngine>(Engine);
            services.RemoveAll<System.TimeProvider>();
            services.AddSingleton<System.TimeProvider>(TimeProvider);
        });
    }
}

/// <summary>Scriptable engine: tests set the snapshot they want the endpoint to publish.</summary>
public sealed class FakeAnalyticsEngine : IAnalyticsEngine
{
    public List<TelemetryReading> Applied { get; } = [];

    public AnalyticsSnapshot NextSnapshot { get; set; } = new(
        [.. Enumerable.Range(0, 10).Select(_ => ImmutableArray<CameraLeaderboardEntry>.Empty)],
        ImmutableArray<GlobalLeaderboardEntry>.Empty);

    public int ActiveVehicles => Applied.Select(r => r.NumberPlate).Distinct(StringComparer.Ordinal).Count();

    public void Apply(in TelemetryReading reading) => Applied.Add(reading);

    public AnalyticsSnapshot Snapshot() => NextSnapshot;
}
