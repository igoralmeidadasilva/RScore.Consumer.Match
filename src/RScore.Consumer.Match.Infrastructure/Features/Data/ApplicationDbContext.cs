using Microsoft.EntityFrameworkCore;
using RScore.Consumer.Match.Domain.Features.Entities;

namespace RScore.Consumer.Match.Infrastructure.Features.Data;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<MatchEvent>? MatchEvents { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}