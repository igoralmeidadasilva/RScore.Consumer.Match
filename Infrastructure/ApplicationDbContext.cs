using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RScore.Consumer.Match.Core.Entities;

namespace RScore.Consumer.Match.Infrastructure;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<MatchEvent> MatchEvents { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}