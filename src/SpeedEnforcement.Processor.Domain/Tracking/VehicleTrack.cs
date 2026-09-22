namespace SpeedEnforcement.Processor.Domain.Tracking;

/// <summary>
/// Cumulative state of one vehicle while it is inside the monitored sector.
/// Single-writer by design: the ingest pipeline routes all readings of one plate to the same partition
/// (see PartitionedIngestPipeline), so this type needs no locking.
/// </summary>
public sealed class VehicleTrack
{
    public VehicleTrack(string numberPlate)
    {
        NumberPlate = numberPlate;
    }

    public string NumberPlate { get; }
    public int CamerasPassed { get; private set; }
    public float AverageSpeed { get; private set; }

    /// <summary>Folds one more camera reading into the running average.</summary>
    public void Record(float speed)
    {
        // TODO: incremental mean, CamerasPassed++.
        throw new NotImplementedException();
    }
}
