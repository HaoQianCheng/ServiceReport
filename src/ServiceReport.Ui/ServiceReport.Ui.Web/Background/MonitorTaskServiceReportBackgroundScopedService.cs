using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Background
{
    public class MonitorTaskServiceReportBackgroundScopedService : BackgroundService
    {
        private readonly ILogger<MonitorTaskServiceReportBackgroundScopedService> _logger;

        public MonitorTaskServiceReportBackgroundScopedService(IServiceProvider services,
            ILogger<MonitorTaskServiceReportBackgroundScopedService> logger)
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
                    var monitorTaskService =
                    scope.ServiceProvider.GetRequiredService<IMonitorTaskService>();

                    await monitorTaskService.DoWork(stoppingToken);
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
}