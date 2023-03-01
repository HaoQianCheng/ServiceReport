using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Web.Core.Pages.Request.Components.Charts;
using ServiceReport.Ui.Web.Core.Pages.Request.Components.TableList;
using ServiceReport.Ui.Web.Core.Parameter;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.Request
{
    public partial class Index
    {
        [Inject] private ILogger<Index> _logger { get; set; }
        [Inject] private IDbContextFactory<ApplicationContext> DbFactory { get; set; }
        [CascadingParameter] protected GlobalSearch GlobalSearch { get; set; }

        private RequestCharts charts;
        private RequestTableList tableList;

        protected override async Task OnInitializedAsync()
        {
            await InitInstanceAsync();

            await base.OnInitializedAsync();
        }

        #region 实例

        private List<Select> Instance = new List<Select>();

        private async Task InitInstanceAsync()
        {
            var context = await DbFactory.CreateDbContextAsync();

            Instance = await context.RequestInfo
                .Where(s => s.Instance != "")
                .GroupBy(s => s.InstanceName)
                .Select(s => new Select() { Key = s.Key, Value = s.Key })
                .ToListAsync();

            Instance.Insert(0, new Select() { Key = "全部", Value = "" });
        }

        private async Task SelectedAsync(string value)
        {
            GlobalSearch.Instance = value;

            if (tableList != null)
            {
                tableList.Table.ReloadData(tableList.Table.GetQueryModel());
            }


            if (charts != null)
            {
                await charts.LoadAsync();
            }
        }

        #endregion 实例

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (tableList != null)
            {
                tableList.Table.ReloadData(tableList.Table.GetQueryModel());
            }

            await base.SetParametersAsync(parameters);

            if (charts != null)
            {
                await charts.LoadAsync();
            }
        }

        public class Select
        {
            public string Key { get; set; }

            public string Value { get; set; }
        }
    }
}