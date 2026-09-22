using MsOptions = Microsoft.Extensions.Options.Options;
using SpeedEnforcement.Processor.Host.Ingestion;
using SpeedEnforcement.Processor.Host.Options;
using Xunit;

namespace SpeedEnforcement.Processor.UnitTests.Ingestion;

public class PartitionedIngestPipelineTests
{
    private const string Todo = "Skeleton: implement PartitionedIngestPipeline.SelectPartition first.";

    [Fact]
    public void Constructor_CreatesOneReaderPerPartition()
    {
        var pipeline = new PartitionedIngestPipeline(MsOptions.Create(new IngestionOptions { PartitionCount = 3, ChannelCapacity = 100 }));

        Assert.Equal(3, pipeline.PartitionCount);
        Assert.Equal(0, pipeline.PendingCount);
        Assert.All(Enumerable.Range(0, 3), i => Assert.NotNull(pipeline.GetReader(i)));
    }

    [Fact]
    public async Task Complete_CompletesEveryPartitionReader()
    {
        var pipeline = new PartitionedIngestPipeline(MsOptions.Create(new IngestionOptions { PartitionCount = 2, ChannelCapacity = 100 }));

        pipeline.Complete();

        await pipeline.GetReader(0).Completion.WaitAsync(TestContext.Current.CancellationToken);
        await pipeline.GetReader(1).Completion.WaitAsync(TestContext.Current.CancellationToken);
    }

    [Fact(Skip = Todo)]
    public void SelectPartition_SamePlate_AlwaysSamePartition() { }

    [Fact(Skip = Todo)]
    public void SelectPartition_IsWithinRange_ForArbitraryPlates() { }

    [Fact(Skip = Todo)]
    public async Task EnqueueAsync_FullPartition_AwaitsUntilConsumed() { await Task.CompletedTask; }
}
