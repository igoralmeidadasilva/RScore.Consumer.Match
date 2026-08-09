namespace RScore.Consumer.Match.Domain.Core.Enums;

public enum EventType
{
    Unknown,
    MatchStart,
    MatchEnd,
    Goal,
    Substitution,
    YellowCard,
    RedCard,
    PenaltyMissed,
    ExtraTimeFirstHalfStart,
    ExtraTimeFirstHalfEnd,
    ExtraTimeSecondHalfStart,
    ExtraTimeSecondHalfEnd
}