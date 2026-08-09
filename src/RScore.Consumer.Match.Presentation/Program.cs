using RScore.Consumer.Match.Application;
using RScore.Consumer.Match.Infrastructure;
using RScore.Consumer.Match.Presentation;
using RScore.Consumer.Match.Presentation.Workers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddApplication(builder.Configuration)
                    .AddInfrastructure(builder.Configuration)
                    .AddPresentation(builder.Configuration);

    builder.Services.AddHostedService<MatchEventsWorker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex.ToString());
    throw;
}
finally
{
    Log.CloseAndFlush();
}