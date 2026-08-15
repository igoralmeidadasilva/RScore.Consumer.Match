using RScore.Consumer.Match.Domain.Core.Enums;

namespace RScore.Consumer.Match.Application.Features.MatchConsumer;

public sealed record MatchConsumerRequest
{
    public Guid EventId { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public string ExternalMatchId { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public int Minute { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public string Source { get; set; } = string.Empty;
}