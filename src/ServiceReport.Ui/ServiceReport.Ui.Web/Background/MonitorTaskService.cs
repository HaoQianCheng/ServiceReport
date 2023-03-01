using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceReport.Ui.Core;
using ServiceReport.Ui.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Background
{
    public interface IMonitorTaskService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    public class MonitorTaskService : IMonitorTaskService
    {
        private readonly ILogger _logger;

        private readonly ApplicationContext _applicationContext;

        public MonitorTaskService(ILogger<MonitorTaskService> logger,
            ApplicationContext applicationContext)
        {
            _logger = logger;
            _applicationContext = applicationContext;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            var tasks = await _applicationContext.MonitorTask
                .Where(s => s.IsOpen)
                .ToListAsync();

            MonitorTaskData.Tasks = tasks;
        }
    }
}