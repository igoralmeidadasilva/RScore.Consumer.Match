using RScore.Consumer.Match.Core.Enums;

namespace RScore.Consumer.Match.Core.Entities;

public sealed record MatchEvent
{
    public Guid EventId { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public string ExternalMatchId { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public int Minute { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MatchEvent"/> class.
    /// This constructor is required for ORM purposes.
    /// Don't use this constructor directly; use the parameterized constructor instead.
    /// </summary>
    public MatchEvent() { }

    public MatchEvent(
        Guid eventId,
        string externalEventId,
        string externalMatchId,
        EventType eventType,
        int minute,
        string payload,
        DateTime receivedAt,
        string source)
    {
        EventId = eventId;
        ExternalEventId = externalEventId;
        ExternalMatchId = externalMatchId;
        EventType = eventType;
        Minute = minute;
        Payload = payload;
        ReceivedAt = receivedAt;
        Source = source;
    }
}