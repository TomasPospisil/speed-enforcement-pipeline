using SpeedEnforcement.Processor.Host;

var app = WebApplication.CreateBuilder(args)
    .ConfigureProcessor()
    .Build()
    .MapProcessorEndpoints();

await app.RunAsync();

/// <summary>Exposed so integration tests can bootstrap the real composition via WebApplicationFactory.</summary>
public partial class Program
{
    protected Program() { }
}
