using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using RScore.Consumer.Match.Core.Entities;
using RScore.Consumer.Match.Infrastructure;
using RScore.Consumer.Match.Options;

namespace RScore.Consumer.Match.Workers;

public sealed class MatchEventsWorker : BackgroundService
{
    private readonly ILogger<MatchEventsWorker> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MatchEventsWorker(
        ILogger<MatchEventsWorker> logger,
        IConsumer<string, string> consumer,
        IOptions<KafkaOptions> options,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _consumer = consumer;
        _options = options.Value;
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
        _consumer.Subscribe(_options.MatchEventsTopic);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult != null)
                {
                    _logger.LogDebug("Received message: {Message}", consumeResult.Message.Value);
                    await ProcessEventAsync(
                        dbContext,
                        consumeResult.Message.Key,
                        consumeResult.Message.Value,
                        stoppingToken);
                    _consumer.Commit(consumeResult);
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
        string key,
        string payload,
        CancellationToken cancellationToken)
    {
        var matchEvent = JsonSerializer.Deserialize<MatchEvent>(payload);
        _logger.LogInformation("Processing match event with key: {Key}, payload: {Payload}", key, payload);

        if (matchEvent is null)
        {
            _logger.LogError("Unable to deserialize the object");
            return;
        }

        dbContext.MatchEvents.Add(matchEvent);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}