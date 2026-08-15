using System.Reflection;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RScore.Consumer.Match.Application.Core;
using RScore.Consumer.Match.Application.Core.Options;
using RScore.Consumer.Match.Domain.Core.Interfaces;
using RScore.Consumer.Match.Domain.Features.Entities.MatchEvents;
using RScore.Consumer.Match.Infrastructure.Features.Data;
using RScore.Consumer.Match.Infrastructure.Features.Repositories;

namespace RScore.Consumer.Match.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureDbContext(configuration)
                .ConfigureRepositories()
                .ConfigureTopic()
                .ConfigureTelemetry();

        return services;
    }

    private static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Postgres"));
            options.EnableSensitiveDataLogging();
        });
        
        return services;
    }

    private static IServiceCollection ConfigureTopic(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = options.Host,
                GroupId = options.MatchEventsConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false, 
                EnableAutoOffsetStore = false
            };
            var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            return consumer;
        });

        return services;
    }

    private static IServiceCollection ConfigureTelemetry(this IServiceCollection services)
    {
        var openTelemetryOptions = services.BuildServiceProvider().GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;

        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(
        [
            new TraceContextPropagator(),
            new BaggagePropagator()
        ]));

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: openTelemetryOptions.ServiceName,
                    serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                    serviceInstanceId: Environment.MachineName);

                resource.AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development",
                    ["host.name"] = Environment.MachineName 
                });
            })
            .WithTracing(tracing => tracing
                .AddSource(Telemetry.SourceName)
                .AddOtlpExporter(otlp =>
                {
                    otlp.Endpoint = new Uri(openTelemetryOptions.Endpoint);
                    otlp.Protocol = OtlpExportProtocol.Grpc;
                }));
            // .WithMetrics(metrics => metrics
            //     .AddRuntimeInstrumentation()
            //     .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(openTelemetryOptions.Endpoint)));
        
        return services;
    }

    private static IServiceCollection ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMatchEventsRepository, MatchEventsRepository>();

        return services;
    }
}