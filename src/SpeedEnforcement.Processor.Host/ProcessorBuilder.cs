using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using SpeedEnforcement.Processor.Domain.Analytics;
using SpeedEnforcement.Processor.Host.Api;
using SpeedEnforcement.Processor.Host.Ingestion;
using SpeedEnforcement.Processor.Host.Monitoring;
using SpeedEnforcement.Processor.Host.Options;
using SpeedEnforcement.Processor.Host.Probes;

namespace SpeedEnforcement.Processor.Host;

/// <summary>
/// Single place where the processor is composed. Mirrors the "ConfigureXxx" extension style:
/// every subsystem registers itself through one extension method, so Program.cs stays a one-liner.
/// </summary>
public static class ProcessorBuilder
{
    public const string ReadinessTag = "ready";

    public static WebApplicationBuilder ConfigureProcessor(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services
            .AddSingleton(TimeProvider.System)
            .ConfigureAnalyticsOptions(configuration)
            .ConfigureIngestionOptions(configuration)
            .AddSingleton<IAnalyticsEngine, AnalyticsEngine>()
            .AddSingleton<IIngestPipeline, PartitionedIngestPipeline>()
            // Transient on purpose: gRPC service instances are created per call, so each client stream
            // gets its own rejection window (ADR-009).
            .AddTransient<IIngestRejectionPolicy, ThresholdRejectionPolicy>()
            .AddSingleton<IProcessorMetrics, ProcessorMetrics>()
            .AddSingleton<ISnapshotDtoMapper, SnapshotDtoMapper>()
            .AddHostedService<IngestWorker>();

        services.AddGrpc();
        services.AddOpenApi();

        services
            .AddHealthChecks()
            .AddCheck<PipelineHealthCheck>("ingest-pipeline", tags: [ReadinessTag]);

        services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(ProcessorMetrics.MeterName);
                if (configuration.GetValue<bool>("Telemetry:ConsoleMetricsExporter"))
                {
                    metrics.AddConsoleExporter();
                }
            });

        return builder;
    }

    public static WebApplication MapProcessorEndpoints(this WebApplication app)
    {
        // gRPC ingest listens on the dedicated HTTP/2 endpoint (see Kestrel section in appsettings.json);
        // REST, health and OpenAPI share the HTTP/1.1 + HTTP/2 endpoint.
        app.MapGrpcService<TelemetryIngestService>();
        app.MapTrafficSnapshot();

        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains(ReadinessTag) });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        return app;
    }
}
