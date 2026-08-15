using System.Diagnostics;

namespace RScore.Consumer.Match.Application.Core;

public static class Telemetry
{
    public const string SourceName = "RScoreService";
    public const string SourceVersion = "1.0.0";
    public static readonly ActivitySource Source = new(SourceName, SourceVersion);

    public static class Tags
    {
        public const string MessagingSystem = "messaging.system";
        public const string MessagingDestination = "messaging.destination";
        public const string ExternalMatchEventId = "event.external_id";
        public const string ExternalMatchId = "event.match.external_id";
        public const string MatchNewEventsCount = "match.new_events_count";
        public const string EventType = "event.type";
        public const string EventSource = "event.source";
    }

    public static class Activities
    {
        public const string ConsumeCycle = "consume.cycle";
        public const string ConsumeProcessResult = "consume.process.result";
    }
}