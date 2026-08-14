namespace RScore.Consumer.Match.Application.Core.Options;

public sealed record OpenTelemetryOptions
{
    public required string Endpoint { get; init; }
}