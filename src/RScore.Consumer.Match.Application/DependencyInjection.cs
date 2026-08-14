using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RScore.Consumer.Match.Application.Core.Options;

namespace RScore.Consumer.Match.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOptions(configuration);

        return services;
    }

    private static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(nameof(KafkaOptions)))
            .ValidateOnStart();

        services.AddOptions<OpenTelemetryOptions>()
            .Bind(configuration.GetSection(nameof(OpenTelemetryOptions)))
            .ValidateOnStart();

        return services;
    }
}