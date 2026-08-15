using RScore.Consumer.Match.Domain.Features.Entities;
using RScore.Consumer.Match.Domain.Features.Entities.MatchEvents;
using RScore.Consumer.Match.Infrastructure.Features.Data;

namespace RScore.Consumer.Match.Infrastructure.Features.Repositories;

internal sealed class MatchEventsRepository : IMatchEventsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MatchEventsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Create(MatchEvent entity)
    {
        _dbContext.Add(entity);
    }
}