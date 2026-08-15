using Microsoft.EntityFrameworkCore;
using RScore.Consumer.Match.Domain.Core.Interfaces;
using RScore.Consumer.Match.Infrastructure.Features.Data;

namespace RScore.Consumer.Match.Infrastructure.Features.Repositories;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}