using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using ServiceReport.Ui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Core
{
    public class MonitorBackgroundJob : IJob
    {
        private IDbContextFactory<ApplicationContext> _contextFactory;

        private ILogger<MonitorBackgroundJob> _logger;

        public Task Execute(IJobExecutionContext context)
        {
            _contextFactory = _contextFactory ?? ServiceContainer.Provider.GetService<IDbContextFactory<ApplicationContext>>() ?? throw new ArgumentNullException(nameof(IDbContextFactory<ApplicationContext>));

            _logger = _logger ?? ServiceContainer.Provider.GetService<ILogger<MonitorBackgroundJob>>() ?? throw new ArgumentNullException(nameof(ILogger<MonitorBackgroundJob>));

            return Task.CompletedTask;
        }
    }
}
