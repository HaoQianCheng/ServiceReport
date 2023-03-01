using AntDesign.Charts;
using Masuit.Tools;
using Masuit.Tools.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceReport.Core.Model;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Web.Core.Parameter;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.Request.Components.Charts
{
    public partial class RequestCharts
    {
        [Inject] private ILogger<RequestCharts> _logger { get; set; }
        [Inject] private IDbContextFactory<ApplicationContext> DbFactory { get; set; }
        [Parameter] public GlobalSearch GlobalSearch { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        public async Task LoadAsync(string activeKey = "")
        {
            if (activeKey == "1")
            {
                await GetPathDataAsync();
            }
            else if (activeKey == "2")
            {
                await GetRequestCountData();
            }
            else
            {
                await GetPathDataAsync();
                await GetRequestCountData();
            }
        }

        #region 地址调用

        private Bar PathChart;
        private object[] PathData;

        private async Task GetPathDataAsync()
        {
            var queryable = await BuildAsync();

            PathData = await queryable
                .GroupBy(x => x.Url)
                .Select(s => new { year = s.Key, value = s.Count() })
                .ToArrayAsync();

            if (PathChart != null)
            {
                if (PathData.Count() == 0)
                {
                    PathData = new object[] { new { year = "无", value = 0 } };
                }
                await PathChart.UpdateChart(all: true, csData: PathData);
            }
        }

        private readonly BarConfig PathConfig = new BarConfig
        {
            XField = "value",
            YField = "year",
            SeriesField = "year",
            Legend = new Legend
            {
                Position = "top-left"
            },
            Name = "name",
            Height = 200
        };

        #endregion 地址调用

        #region 调用总数

        private Area RequestCountChart;
        private object[] RequestCountData;

        public async Task GetRequestCountData()
        {
            var queryable = await BuildAsync();

            RequestCountData = queryable
                 .Select(s => new { date = s.CreateTimeStr })
                 .GroupBy(s => s.date)
                 .Select(s => new { date = s.Key, scales = s.Count() })
                 .ToArray();

            if (RequestCountChart != null)
            {
                if (RequestCountData.Count() == 0)
                {
                    RequestCountData = new object[] { new { date = "无", scales = 0 } };
                }
                await RequestCountChart.UpdateChart(all: true, csData: RequestCountData);
            }
        }

        private readonly AreaConfig RequestCountConfig = new AreaConfig
        {
            XField = "date",
            YField = "scales",
            XAxis = new ValueCatTimeAxis
            {
                Visible = true,
                Label = new BaseAxisLabel
                {
                    Visible = true,
                    AutoHide = true,
                }
            },
            Slider = new Slider()
            {
                Start = 0.1,
                End = 0.9,
                TrendCfg = new TrendCfg()
                {
                    IsArea = true
                }
            },
            Height = 200
        };

        #endregion 调用总数

        protected async Task<IQueryable<RequestInfo>> BuildAsync()
        {
            var context = await DbFactory.CreateDbContextAsync();

            var queryable = context.RequestInfo.AsQueryable();

            try
            {
                Expression<Func<RequestInfo, bool>> query = s => s.Id != null;

                if (!string.IsNullOrEmpty(GlobalSearch.Instance))
                {
                    query = query.And(s => s.InstanceName.Equals(GlobalSearch.Instance));
                }

                if (GlobalSearch.SearchChartBCreateTime != null)
                {
                    query = query.And(s => s.CreateTime >= GlobalSearch.SearchChartBCreateTime);
                }
                if (GlobalSearch.SearchChartECreateTime != null)
                {
                    query = query.And(s => s.CreateTime <= GlobalSearch.SearchChartECreateTime);
                }

                if (!string.IsNullOrEmpty(GlobalSearch.Instance))
                {
                    query = query.And(s => s.InstanceName.Equals(GlobalSearch.Instance));
                }

                queryable = queryable.Where(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return queryable;
        }
    }
}