using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RScore.Consumer.Match.Infrastructure;
using RScore.Consumer.Match.Options;
using RScore.Consumer.Match.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<KafkaOptions>()
    .Bind(builder.Configuration.GetSection(nameof(KafkaOptions)))
    .ValidateOnStart();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton(sp =>
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

builder.Services.AddHostedService<MatchEventsWorker>();

var host = builder.Build();
host.Run();