namespace SpeedEnforcement.Client.Core.Options;

public sealed class ClientOptions
{
    public const string SectionName = "Client";

    public string ProcessorBaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>Fixed by the assignment: exactly 3 seconds.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>Cameras offered in the selector. Must match the processor's Analytics:CameraCount.</summary>
    public int CameraCount { get; set; } = 10;
}
