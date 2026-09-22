using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using SpeedEnforcement.Contracts.Api;
using SpeedEnforcement.Processor.Domain.Analytics;
using SpeedEnforcement.Processor.Domain.Leaderboards;
using Xunit;

namespace SpeedEnforcement.Processor.IntegrationTests;

public sealed class TrafficSnapshotEndpointTests : IClassFixture<ProcessorWebApplicationFactory>
{
    private readonly ProcessorWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TrafficSnapshotEndpointTests(ProcessorWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthLive_Returns200()
    {
        var response = await _client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_WithEmptyBacklog_Returns200()
    {
        var response = await _client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TrafficSnapshot_ReturnsDtoShape_WithTenCameraBoardsAndUtcTimestamp()
    {
        _factory.Engine.NextSnapshot = new AnalyticsSnapshot(
            [.. Enumerable.Range(1, 10).Select(id => id == 5
                ? ImmutableArray.Create(new CameraLeaderboardEntry("5AB0001", 171.2f))
                : ImmutableArray<CameraLeaderboardEntry>.Empty)],
            [new GlobalLeaderboardEntry("5AB0001", 160.5f, 4)]);

        var dto = await _client.GetFromJsonAsync<TrafficSnapshotDto>(TrafficApiRoutes.TrafficSnapshot, TestContext.Current.CancellationToken);

        Assert.NotNull(dto);
        Assert.Equal(ProcessorWebApplicationFactory.FrozenNow, dto.GeneratedAtUtc);
        Assert.Equal(Enumerable.Range(1, 10), dto.CameraLeaderboards.Select(b => b.CameraId));
        var camera5 = Assert.Single(dto.CameraLeaderboards, b => b.Entries.Count > 0);
        Assert.Equal(5, camera5.CameraId);
        Assert.Equal("5AB0001", camera5.Entries[0].NumberPlate);
        var global = Assert.Single(dto.GlobalLeaderboard);
        Assert.Equal(4, global.CamerasPassed);
    }

    [Fact]
    public async Task TrafficSnapshot_UsesCamelCaseJson()
    {
        var json = await _client.GetStringAsync(TrafficApiRoutes.TrafficSnapshot, TestContext.Current.CancellationToken);

        Assert.Contains("\"generatedAtUtc\"", json, StringComparison.Ordinal);
        Assert.Contains("\"cameraLeaderboards\"", json, StringComparison.Ordinal);
        Assert.Contains("\"globalLeaderboard\"", json, StringComparison.Ordinal);
    }
}
