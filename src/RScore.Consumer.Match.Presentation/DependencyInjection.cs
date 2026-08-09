using Serilog;

namespace RScore.Consumer.Match.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureLogger(configuration);

        return services;
    }

    private static IServiceCollection ConfigureLogger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services));

        return services;
    }
}