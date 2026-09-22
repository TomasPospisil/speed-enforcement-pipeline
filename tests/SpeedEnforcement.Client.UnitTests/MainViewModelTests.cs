using Microsoft.Extensions.Options;
using SpeedEnforcement.Client.Core.Options;
using SpeedEnforcement.Client.Core.ViewModels;
using SpeedEnforcement.Tests.Common.Builders;
using Xunit;

namespace SpeedEnforcement.Client.UnitTests;

public class MainViewModelTests
{
    private readonly MainViewModel _vm = new(Options.Create(new ClientOptions { CameraCount = 10 }));

    [Fact]
    public void Apply_FillsGlobalBoardWithRanks_AndSelectedCameraBoard()
    {
        var snapshot = TrafficSnapshotDtoBuilder.CreateDefault()
            .WithGlobalEntry("AAA0001", 161.0f, 5)
            .WithGlobalEntry("AAA0002", 158.5f, 3)
            .WithCameraEntry(1, "BBB0001", 172.0f)
            .Build();

        _vm.Apply(snapshot);

        Assert.Equal([1, 2], _vm.GlobalLeaderboard.Select(r => r.Rank));
        Assert.Equal("AAA0001", _vm.GlobalLeaderboard[0].NumberPlate);
        Assert.Equal("BBB0001", Assert.Single(_vm.SelectedCameraLeaderboard).NumberPlate);
        Assert.Equal(snapshot.GeneratedAtUtc, _vm.LastUpdatedUtc);
        Assert.False(_vm.HasError);
    }

    [Fact]
    public void SelectedCameraId_Change_RefreshesFromLastSnapshotWithoutRefetch()
    {
        _vm.Apply(TrafficSnapshotDtoBuilder.CreateDefault().WithCameraEntry(7, "CCC0007", 149.9f).Build());
        Assert.Empty(_vm.SelectedCameraLeaderboard);

        _vm.SelectedCameraId = 7;

        Assert.Equal("CCC0007", Assert.Single(_vm.SelectedCameraLeaderboard).NumberPlate);
    }

    [Fact]
    public void ReportError_SetsErrorState_ButKeepsLastData()
    {
        _vm.Apply(TrafficSnapshotDtoBuilder.CreateDefault().WithGlobalEntry("AAA0001", 161.0f, 5).Build());

        _vm.ReportError("timeout");

        Assert.True(_vm.HasError);
        Assert.Contains("timeout", _vm.Status, StringComparison.Ordinal);
        Assert.Single(_vm.GlobalLeaderboard);
    }
}
