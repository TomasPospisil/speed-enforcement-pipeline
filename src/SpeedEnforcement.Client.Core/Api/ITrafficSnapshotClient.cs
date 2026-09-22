using System.Net.Http.Json;
using SpeedEnforcement.Contracts.Api;

namespace SpeedEnforcement.Client.Core.Api;

public interface ITrafficSnapshotClient
{
    Task<TrafficSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken);
}

/// <summary>Typed HttpClient; base address, timeout and resilience come from <see cref="ClientCoreBuilder"/>.</summary>
public sealed class TrafficSnapshotClient : ITrafficSnapshotClient
{
    private readonly HttpClient _http;

    public TrafficSnapshotClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<TrafficSnapshotDto> GetSnapshotAsync(CancellationToken cancellationToken)
        => await _http.GetFromJsonAsync<TrafficSnapshotDto>(TrafficApiRoutes.TrafficSnapshot, cancellationToken)
           ?? throw new InvalidOperationException("Processor returned an empty snapshot payload.");
}
