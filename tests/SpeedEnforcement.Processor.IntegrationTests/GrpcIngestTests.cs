using Xunit;

namespace SpeedEnforcement.Processor.IntegrationTests;

/// <summary>
/// End-to-end through the wire: a Grpc.Net.Client channel over <c>factory.Server.CreateHandler()</c> streams
/// messages into <c>TelemetryIngestService</c>; the REST snapshot is then asserted against the fake engine's
/// recorded readings. Needs TelemetryMapper and the rejection policy implemented.
/// </summary>
public sealed class GrpcIngestTests : IClassFixture<ProcessorWebApplicationFactory>
{
    private const string Todo = "Skeleton: implement TelemetryMapper.TryMap and ThresholdRejectionPolicy first.";

    public GrpcIngestTests(ProcessorWebApplicationFactory factory)
    {
        _ = factory;
    }

    [Fact(Skip = Todo)]
    public void Ingest_StreamOfValidMessages_ReachesEngineInPerPlateOrder() { }

    [Fact(Skip = Todo)]
    public void Ingest_IsolatedInvalidMessage_IsCountedAndStreamContinues() { }

    [Fact(Skip = Todo)]
    public void Ingest_SystematicInvalidMessages_AbortsStreamWithInvalidArgument() { }

    [Fact(Skip = Todo)]
    public void Ingest_CompletedStream_ReturnsAcceptedAndRejectedCounts() { }
}
