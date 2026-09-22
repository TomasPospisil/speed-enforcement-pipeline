using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SpeedEnforcement.Client.Core.Options;

public static class ClientOptionsExtensions
{
    public static IServiceCollection ConfigureClientOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ClientOptions>()
            .Bind(configuration.GetSection(ClientOptions.SectionName))
            .Validate(o => Uri.TryCreate(o.ProcessorBaseUrl, UriKind.Absolute, out _),
                "Client:ProcessorBaseUrl must be an absolute URI (e.g. http://localhost:5000).")
            .Validate(o => o.PollInterval >= TimeSpan.FromMilliseconds(500),
                "Client:PollInterval must be at least 500 ms.")
            .Validate(o => o.CameraCount is > 0 and <= 100,
                "Client:CameraCount must be in (0, 100].")
            .ValidateOnStart();

        return services;
    }
}
