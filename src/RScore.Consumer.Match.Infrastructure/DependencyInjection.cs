using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RScore.Consumer.Match.Application.Core.Options;
using RScore.Consumer.Match.Infrastructure.Features.Data;

namespace RScore.Consumer.Match.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureDbContext(configuration)
                .ConfigureTopic();

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
}