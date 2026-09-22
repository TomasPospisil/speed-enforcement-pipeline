using SpeedEnforcement.Generator.Simulation;
using Xunit;

namespace SpeedEnforcement.Generator.UnitTests.Simulation;

/// <summary>Invariants of the assignment's state machine. Bodies follow the implementation.</summary>
public class VehicleRegistryTests
{
    private const string Todo = "Skeleton: implement VehicleRegistry first.";

    [Fact(Skip = Todo)]
    public void Spawn_AddsExactlyCountVehicles_WithCameraCounterZero() { }

    [Fact(Skip = Todo)]
    public void Spawn_TwiceInARow_ProducesUniquePlates() { }

    [Fact(Skip = Todo)]
    public void RemoveExited_DropsOnlyVehiclesPastLastCamera() { }

    [Fact(Skip = Todo)]
    public void Vehicle_LivesExactlyCameraCountTicks_AndEmitsCameraCountMessages() { }

    [Fact]
    public void Vehicle_Advance_IncrementsCounterAndStoresSpeed()
    {
        var vehicle = new Vehicle("1AB2345");

        vehicle.Advance(128.5f);
        vehicle.Advance(131.0f);

        Assert.Equal(2, vehicle.CameraCounter);
        Assert.Equal(131.0f, vehicle.LastSpeed);
    }
}
