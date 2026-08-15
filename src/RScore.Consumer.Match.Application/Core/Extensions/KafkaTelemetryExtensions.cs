using System.Text;
using Confluent.Kafka;
using OpenTelemetry.Context.Propagation;

namespace RScore.Consumer.Match.Application.Core.Extensions;

public static class KafkaTelemetryExtensions
{
    public static PropagationContext ExtractTraceContext(this Headers headers)
    {
        if (headers is null)
            return default;

        return Propagators.DefaultTextMapPropagator.Extract(
            default,
            headers,
            (h, key) =>
            {
                var header = h.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (header is null) return [];

                return [Encoding.UTF8.GetString(header.GetValueBytes())];
            });
    }
}