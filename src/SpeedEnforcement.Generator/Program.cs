using Microsoft.Extensions.Hosting;
using SpeedEnforcement.Generator;

var host = Host.CreateApplicationBuilder(args)
    .ConfigureGenerator()
    .Build();

await host.RunAsync();
