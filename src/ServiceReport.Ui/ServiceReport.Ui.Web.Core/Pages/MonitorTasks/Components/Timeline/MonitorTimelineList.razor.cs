using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Storage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.MonitorTasks.Components.Timeline
{
    public partial class MonitorTimelineList
    {
        [Inject] private IDbContextFactory<ApplicationContext> DbFactory { get; set; }

        private bool loading = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await GetTimeLine();
            }
            catch (Exception ex)
            {
                throw;
            }
            await base.OnInitializedAsync();
        }

        private List<MonitorWarning> monitorWarnings = new List<MonitorWarning>();

        public async Task GetTimeLine()
        {
            loading = true;

            var applicationContext = await DbFactory.CreateDbContextAsync();

            monitorWarnings = await applicationContext.MonitorWarning
                .OrderByDescending(s => s.CreateTime)
                .ToListAsync();

            loading = false;
        }
    }
}