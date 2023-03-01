using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceReport.Saver.Http.Options;

namespace ServiceReport.Background;

public class GcServiceReportBackgroundScopedService : BackgroundService
{
    private readonly ILogger<GcServiceReportBackgroundScopedService> _logger;

    public GcServiceReportBackgroundScopedService(IServiceProvider services,
        ILogger<GcServiceReportBackgroundScopedService> logger)
    {
        Services = services;
        _logger = logger;
    }

    public IServiceProvider Services { get; }

    private int milliseconds = 10000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service running.");

        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is working.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = Services.CreateScope())
            {
                //Option
                var sendOptions = scope.ServiceProvider
        .GetRequiredService<IOptionsSnapshot<HttpServiceReportSaverOptions>>()?.Value
        ?? throw new ArgumentNullException(nameof(HttpServiceReportSaverOptions));

                milliseconds = sendOptions.PushSecond * 1000;

                var monitorProcessingService =
                scope.ServiceProvider.GetRequiredService<IGcProcessingService>();

                //await monitorProcessingService.DoWork(stoppingToken);
            }
            await Task.Delay(milliseconds, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}