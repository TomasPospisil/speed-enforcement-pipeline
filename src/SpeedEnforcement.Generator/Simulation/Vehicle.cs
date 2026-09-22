namespace SpeedEnforcement.Generator.Simulation;

/// <summary>Mutable simulation entity. Lives exactly <c>CameraCount</c> ticks: counter 0 (spawned) .. CameraCount (last emission), then exits.</summary>
public sealed class Vehicle
{
    public Vehicle(string numberPlate)
    {
        NumberPlate = numberPlate;
    }

    public string NumberPlate { get; }

    /// <summary>0 = spawned, not yet captured. Incremented once per tick.</summary>
    public int CameraCounter { get; private set; }

    public float LastSpeed { get; private set; }

    public void Advance(float speed)
    {
        CameraCounter++;
        LastSpeed = speed;
    }
}
