using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using RScore.Consumer.Match.Application.Core.Options;
using RScore.Consumer.Match.Application.Features.MatchConsumer;

namespace RScore.Consumer.Match.Presentation.Workers;

internal sealed class MatchEventsWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<MatchEventsWorker> _logger;

    public MatchEventsWorker(ILogger<MatchEventsWorker> logger, IConsumer<string, string> consumer, IOptions<KafkaOptions> kafkaOptions, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _consumer = consumer;
        _kafkaOptions = kafkaOptions.Value;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(
            () => StartConsumerAsync(stoppingToken),
            stoppingToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
    }

    private async Task StartConsumerAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_kafkaOptions.MatchEventsTopic);
        _logger.LogInformation("Subscribed to topic {Topic}. Starting consumption loop...", _kafkaOptions.MatchEventsTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? consumeResult = null;

                try
                {
                    consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message is not null)
                    {
                        await ProcessConsumeResult(consumeResult, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Cancellation requested for topic {Topic}. Exiting consumption loop...", _kafkaOptions.MatchEventsTopic);
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consumption error on topic {Topic}. ErrorCode: {ErrorCode}, Reason: {Reason}", 
                        _kafkaOptions.MatchEventsTopic, ex.Error.Code, ex.Error.Reason);

                    if (ex.Error.IsFatal)
                    {
                        _logger.LogCritical("Fatal error encountered in Kafka consumer for topic {Topic}. Stopping worker.", _kafkaOptions.MatchEventsTopic);
                        break;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize message payload from topic {Topic}, Partition: {Partition}, Offset: {Offset}. Raw Value: {Payload}", 
                        consumeResult?.Topic ?? _kafkaOptions.MatchEventsTopic, 
                        consumeResult?.Partition.Value, 
                        consumeResult?.Offset.Value, 
                        consumeResult?.Message?.Value);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception processing message from topic {Topic}, Partition: {Partition}, Offset: {Offset}", 
                        consumeResult?.Topic ?? _kafkaOptions.MatchEventsTopic, 
                        consumeResult?.Partition.Value, 
                        consumeResult?.Offset.Value);
                }
            }
        }
        finally
        {
            _logger.LogInformation("Closing Kafka consumer for topic {Topic} to leave consumer group gracefully...", _kafkaOptions.MatchEventsTopic);
            
            _consumer.Close();
        }
    }

    private async Task ProcessConsumeResult(ConsumeResult<string, string> consumeResult, CancellationToken stoppingToken)
    {
        _logger.LogDebug("Processing message from topic {Topic}, Partition: {Partition}, Offset: {Offset}",
            consumeResult.Topic,
            consumeResult.Partition.Value,
            consumeResult.Offset.Value);

        using (var scope = _scopeFactory.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<IMatchConsumerHandler>();

            var matchConsumerRequest = JsonSerializer.Deserialize<MatchConsumerRequest>(consumeResult.Message.Value);

            await handler.ExecuteAsync(matchConsumerRequest!, stoppingToken);
        }

        _consumer.Commit(consumeResult);
    }
}