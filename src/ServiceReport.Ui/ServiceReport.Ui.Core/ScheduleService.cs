using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using ServiceReport.Ui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Core
{
    public class ScheduleService
    {
        private const string SchedulerGroup = "ServiceReport_Scheduler";

        private const string SchedulerTag = "Monitor_";

        public readonly IScheduler _scheduler;

        private readonly ILogger<ScheduleService> _logge;

        private readonly IDbContextFactory<ApplicationContext> DbFactory;

        private readonly ServiceReportUiOptions _options;

        public ScheduleService(ILogger<ScheduleService> logger,
            IDbContextFactory<ApplicationContext> dbContextFactory,
            IOptions<ServiceReportUiOptions> options)
        {
            _logge = logger;
            _scheduler = _scheduler ?? new StdSchedulerFactory().GetScheduler().Result;
            DbFactory = dbContextFactory;
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task InitAsync()
        {
            await InitMonitorJobAsync();

            await _scheduler.Start();
        }

        public async Task InitMonitorJobAsync()
        {
            var job = JobBuilder.Create<MonitorBackgroundJob>().
                WithIdentity(SchedulerTag , SchedulerGroup)
                .SetJobData(new JobDataMap { { "job", new object() } }).Build();

            var trigger = TriggerBuilder.Create().WithCronSchedule("* * * * * ? *").Build();

            await _scheduler.ScheduleJob(job, trigger);
        }
    }
}
