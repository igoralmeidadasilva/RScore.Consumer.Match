namespace RScore.Consumer.Match.Domain.Features.Entities.MatchEvents;

public interface IMatchEventsRepository
{
    void Create(MatchEvent entity);
}