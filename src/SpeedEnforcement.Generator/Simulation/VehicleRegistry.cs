namespace SpeedEnforcement.Generator.Simulation;

/// <summary>
/// Central registry of active vehicles. Not thread-safe by design: it is owned by the single simulation
/// thread (<see cref="SimulationLoop"/>); the network side only ever sees messages, never vehicles.
/// </summary>
public sealed class VehicleRegistry
{
    private readonly Dictionary<string, Vehicle> _active = new(StringComparer.Ordinal);

    public int Count => _active.Count;

    public IEnumerable<Vehicle> Active => _active.Values;

    /// <summary>Step 1 of a tick: adds <paramref name="count"/> new vehicles with unique plates and counter 0.</summary>
    public void Spawn(int count, IPlateGenerator plates)
    {
        // TODO: loop count times: var plate = plates.Next(); _active.Add(plate, new Vehicle(plate));
        throw new NotImplementedException();
    }

    /// <summary>Step 3 of a tick: drops vehicles whose counter exceeded the last camera.</summary>
    /// <returns>Number of vehicles removed.</returns>
    public int RemoveExited(int lastCameraId)
    {
        // TODO: collect keys where CameraCounter > lastCameraId, remove, return count.
        throw new NotImplementedException();
    }
}
