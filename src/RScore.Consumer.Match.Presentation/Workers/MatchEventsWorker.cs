using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using RScore.Consumer.Match.Application.Core.Options;
using RScore.Consumer.Match.Domain.Features.Entities;
using RScore.Consumer.Match.Infrastructure;
using RScore.Consumer.Match.Infrastructure.Features.Data;
using RScore.Consumer.Match.Infrastructure.Features.Data.Extensions;

namespace RScore.Consumer.Match.Presentation.Workers;

public sealed class MatchEventsWorker : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new(Constants.Telemetry.SERVICE_NAME);
    private readonly ILogger<MatchEventsWorker> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MatchEventsWorker(
        ILogger<MatchEventsWorker> logger,
        IConsumer<string, string> consumer,
        IOptions<KafkaOptions> kafkaOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _consumer = consumer;
        _kafkaOptions = kafkaOptions.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(
            () => StartConsumer(stoppingToken),
            stoppingToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
    }

    private async Task StartConsumer(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _consumer.Subscribe(_kafkaOptions.MatchEventsTopic);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult != null)
                {
                    _logger.LogDebug("Received message: {Message}", consumeResult.Message.Value);
                    await ProcessEventAsync(
                        dbContext,
                        consumeResult,
                        stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Match events worker is stopping due to cancellation.");
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Error consuming message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the message. The offset will NOT be committed.");
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessEventAsync(
        ApplicationDbContext dbContext,
        ConsumeResult<string, string> consumeResult,
        CancellationToken cancellationToken)
    {
        var propagationContext = consumeResult.Message.Headers.ExtractTraceContext();

        using var activity = ActivitySource.StartActivity(
            "ConsumeEvent",
            ActivityKind.Consumer,
            parentContext: propagationContext.ActivityContext);

        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination", consumeResult.Topic);
        activity?.SetTag("messaging.kafka.consumer_group", _kafkaOptions.MatchEventsTopic);
        activity?.SetTag("messaging.kafka.partition", consumeResult.Partition.Value);
        activity?.SetTag("messaging.kafka.offset", consumeResult.Offset.Value);

        var matchEvent = JsonSerializer.Deserialize<MatchEvent>(consumeResult.Message.Value);

        if (matchEvent is null)
        {
            _logger.LogWarning("Null payload received for key {Key}", consumeResult.Message.Key);

            return;
        }

        activity?.SetTag("event.type", matchEvent.EventType);
        activity?.SetTag("match.external_id", matchEvent.ExternalMatchId);

        _logger.LogInformation("Processing match event with key: {Key}, payload: {Payload}", consumeResult.Message.Key, consumeResult.Message.Value);
        
        dbContext.MatchEvents!.Add(matchEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        _consumer.Commit(consumeResult);
    }
}