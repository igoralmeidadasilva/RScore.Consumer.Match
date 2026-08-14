using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RScore.Consumer.Match.Application.Core.Options;
using RScore.Consumer.Match.Infrastructure.Features.Data;

namespace RScore.Consumer.Match.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureDbContext(configuration)
                .ConfigureTopic()
                .ConfigureTelemetry();

        return services;
    }

    private static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Postgres"));
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

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(Constants.Telemetry.SERVICE_NAME))
            .WithTracing(tracing => tracing
                .AddSource(Constants.Telemetry.SERVICE_NAME)
                .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(openTelemetryOptions.Endpoint)))
            .WithMetrics(metrics => metrics
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(openTelemetryOptions.Endpoint)));
        
        return services;
    }
}