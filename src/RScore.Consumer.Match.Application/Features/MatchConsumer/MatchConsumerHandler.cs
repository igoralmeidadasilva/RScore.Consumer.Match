using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RScore.Consumer.Match.Application.Core;
using RScore.Consumer.Match.Application.Core.Options;
using RScore.Consumer.Match.Domain.Core.Interfaces;
using RScore.Consumer.Match.Domain.Features.Entities;
using RScore.Consumer.Match.Domain.Features.Entities.MatchEvents;

namespace RScore.Consumer.Match.Application.Features.MatchConsumer;

internal sealed class MatchConsumerHandler : IMatchConsumerHandler
{
    private readonly ILogger<MatchConsumerHandler> _logger;
    private readonly IMatchEventsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly KafkaOptions _kafkaOptions;

    public MatchConsumerHandler(
        ILogger<MatchConsumerHandler> logger,
        IMatchEventsRepository repository,
        IUnitOfWork unitOfWork,
        IOptions<KafkaOptions> kafkaOptions)
    {
        _logger = logger;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _kafkaOptions = kafkaOptions.Value;
    }

    public async Task ExecuteAsync(MatchConsumerRequest request, CancellationToken cancellationToken = default)
    {
        using Activity? span = Telemetry.Source.StartActivity(
            $"{_kafkaOptions.MatchEventsTopic} process",
            ActivityKind.Internal);

        span?.SetTag(Telemetry.Tags.MessagingSystem, "kafka");
        span?.SetTag(Telemetry.Tags.MessagingDestination, _kafkaOptions.MatchEventsTopic);
        span?.SetTag(Telemetry.Tags.ExternalMatchEventId, request.ExternalEventId);
        span?.SetTag(Telemetry.Tags.ExternalMatchId, request.ExternalMatchId);
        span?.SetTag(Telemetry.Tags.EventType, request.EventType.ToString());
        span?.SetTag(Telemetry.Tags.EventSource, request.Source);

        _logger.LogInformation("Starting match event processing. ExternalEventId: {ExternalEventId}, ExternalMatchId: {ExternalMatchId}, EventType: {EventType}", 
            request.ExternalEventId,
            request.ExternalMatchId,
            request.EventType);

        try
        {
            MatchEvent matchEvent = new(
                request.ExternalEventId,
                request.ExternalMatchId,
                request.EventType,
                request.Minute,
                request.Payload,
                request.ReceivedAt,
                request.Source
            );

            _repository.Create(matchEvent);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Match event successfully persisted. Id: {Id}, ExternalEventId: {ExternalEventId}, ExternalMatchId: {ExternalMatchId}", 
                matchEvent.Id,
                matchEvent.ExternalEventId,
                matchEvent.ExternalMatchId);

            span?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            span?.AddException(ex);
 
            _logger.LogError(
                ex,
                "Failed to persist match event. ExternalEventId: {ExternalEventId}, ExternalMatchId: {ExternalMatchId}",
                request.ExternalEventId,
                request.ExternalMatchId);
 
            throw;
        }       
    }
}