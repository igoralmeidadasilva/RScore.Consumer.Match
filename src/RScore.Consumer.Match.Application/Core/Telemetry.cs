using System.Diagnostics;

namespace RScore.Consumer.Match.Application.Core;

public static class Telemetry
{
    public const string SourceName = "RScoreService";
    public const string SourceVersion = "1.0.0";
    public static readonly ActivitySource Source = new(SourceName, SourceVersion);

    public static class Tags
    {

    }

    public static class Activities
    {

    }
}