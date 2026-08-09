namespace RScore.Consumer.Match.Application.Core.Options;

public sealed record KafkaOptions
{
    public required string Host { get; init; }
    public required string MatchEventsTopic { get; init; }
    public required string MatchEventsConsumerGroup { get; init; }
}