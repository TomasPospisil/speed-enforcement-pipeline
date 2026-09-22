using Xunit;

namespace SpeedEnforcement.Generator.UnitTests.Simulation;

/// <summary>
/// Intended tests for the tick orchestration. They will drive <c>SimulationLoop.TickAsync</c> directly with a
/// FakeTimeProvider, a fake outbox and a stub speed model, no timer involved.
/// </summary>
public class SimulationLoopTests
{
    private const string Todo = "Skeleton: needs VehicleRegistry + plate generator implementation.";

    [Fact(Skip = Todo)]
    public void Tick_RunsInfluxThenProgressionThenExit_InThatOrder() { }

    [Fact(Skip = Todo)]
    public void Tick_EmitsOneMessagePerActiveVehicle_WithCurrentCameraId() { }

    [Fact(Skip = Todo)]
    public void Tick_VehiclePastLastCamera_IsNotEmittedAndIsRemoved() { }

    [Fact(Skip = Todo)]
    public void Tick_TimestampTicks_ComeFromTimeProvider() { }

    [Fact(Skip = Todo)]
    public void ExecuteAsync_AdvancingFakeClockByOneSecond_RunsExactlyOneTick() { }
}
