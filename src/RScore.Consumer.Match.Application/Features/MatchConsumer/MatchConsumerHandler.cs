using Microsoft.Extensions.Logging;
using RScore.Consumer.Match.Domain.Core.Interfaces;
using RScore.Consumer.Match.Domain.Features.Entities;
using RScore.Consumer.Match.Domain.Features.Entities.MatchEvents;

namespace RScore.Consumer.Match.Application.Features.MatchConsumer;

internal sealed class MatchConsumerHandler : IMatchConsumerHandler
{
    private readonly ILogger<MatchConsumerHandler> _logger;
    private readonly IMatchEventsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MatchConsumerHandler(
        ILogger<MatchConsumerHandler> logger,
        IMatchEventsRepository repository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(MatchConsumerRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting match event processing. ExternalEventId: {ExternalEventId}, ExternalMatchId: {ExternalMatchId}, EventType: {EventType}", 
            request.ExternalEventId,
            request.ExternalMatchId,
            request.EventType);

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
    }
}