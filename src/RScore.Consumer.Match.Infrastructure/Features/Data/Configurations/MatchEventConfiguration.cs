using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RScore.Consumer.Match.Domain.Features.Entities;

namespace RScore.Consumer.Match.Infrastructure.Features.Data.Configurations;

internal sealed class MatchEventConfiguration : IEntityTypeConfiguration<MatchEvent>
{
    public void Configure(EntityTypeBuilder<MatchEvent> builder)
    {
        builder.ToTable("match_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();
        
        builder.Property(x => x.ExternalEventId)
            .HasColumnName("external_event_id")
            .IsRequired();
        
        builder.Property(x => x.ExternalMatchId)
            .HasColumnName("external_match_id")
            .IsRequired();

        builder.Property(x => x.ExternalHomeTeamId)
            .HasColumnName("external_home_team_id")
            .IsRequired();

        builder.Property(x => x.ExternalVisitorTeamId)
            .HasColumnName("external_visitor_team_id")
            .IsRequired();
        
        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Minute)
            .HasColumnName("minute")
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.ReceivedAt)
            .HasColumnName("received_at")
            .IsRequired();

        builder.Property(x => x.Source)
            .HasColumnName("source");
    }
}