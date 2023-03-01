using AngleSharp.Io;
using AntDesign.Charts;
using Masuit.Tools.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServiceReport.Core.Model;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Web.Core.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.Dashboard.Components.Body
{
    public partial class Body
    {
        [Inject] private IDbContextFactory<ApplicationContext> DbFactory { get; set; }

        [Parameter]
        public GlobalSearch GlobalSearch { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        public Task Reload()
        {

            Task.Run(async () =>
            {
                await Line();
            });
            Task.Run(async () =>
            {
                await Heatmap();
            });
            Task.Run(async () =>
            {
                await Bar1();
            });
            Task.Run(async () =>
            {
                await Bar2();
            });
            Task.Run(async () =>
            {
                await Bar3();
            });

            return Task.CompletedTask;
        }

        #region 线图

        private Line chart2;

        public async Task Line()
        {
            var context = DbFactory.CreateDbContext();

            object[] data1 = BuildReuqest(context.RequestInfo.AsQueryable())
                 .GroupBy(s => s.CreateTimeStr)
                 .OrderByDescending(s => s.Count())
                 .Select(s => new { date = s.Key, type = s.FirstOrDefault().InstanceName, value = s.Count() })
                 .ToArray();

            if (chart2 != null)
            {
                if (data1.Count() == 0)
                {
                    data1 = new object[] { new { date = "无", type = "", value = 0 } };
                }
                await chart2.UpdateChart(csData: data1);
            }
        }

        private LineConfig config2 = new LineConfig()
        {
            Padding = "auto",
            XField = "date",
            YField = "value",
            YAxis = new ValueAxis()
            {
                Label = new BaseAxisLabel()
                {
                    // 数值格式化为千分位
                },
            },
            Legend = new Legend()
            {
                Position = "right-top",
            },
            SeriesField = "type",
            Height = 200
        };

        #endregion 线图

        #region 热力图

        private Heatmap chart1;

        public async Task Heatmap()
        {
            var context = DbFactory.CreateDbContext();

            object[] data1 = BuildReuqest(context.RequestInfo.AsQueryable())
                .GroupBy(s => s.CreateTimeStr)
                .Select(s => new { month_of_Year = s.Key, District = s.FirstOrDefault().InstanceName, AQHI = s.Count() })
                .ToArray();
            if (chart1 != null)
            {
                if (data1.Count() == 0)
                {
                    data1 = new object[] { new { month_of_Year = "无", District = "", AQHI = 0 } };
                }
                await chart1.UpdateChart(csData: data1);
            }
        }

        private HeatmapConfig config1 = new HeatmapConfig()
        {
            Height = 200,
            XField = "month_of_Year",
            YField = "district",
            ColorField = "aqhi",
            ShapeType = "rect",
            Color = new string[] { "#F7F7F7", "#D6E2F5", "#85A9E0", "#3069BE", "#255296" },
            Meta = new
            {
                Month_of_Year = new
                {
                    Type = "cat",
                },
            },
        };

        #endregion 热力图

        #region Bar1

        private Bar chart3;

        public async Task Bar1()
        {
            var context = DbFactory.CreateDbContext();

            object[] data1 = BuildReuqest(context.RequestInfo.AsQueryable())
                .GroupBy(s => s.InstanceName)
                .OrderByDescending(s => s.Count())
                .Take(4)
                .Select(s => new { value = s.Key, type = s.Count() })
                .ToArray();

            if (chart3 != null)
            {
                if (data1.Count() == 0)
                {
                    data1 = new object[] { new { value = "无", type = 0 } };
                }
                await chart3.UpdateChart(csData: data1);
            }
        }

        private readonly BarConfig config3 = new BarConfig
        {
            XField = "type",
            YField = "value",
            Label = new BarViewConfigLabel
            {
                Visible = true
            },
            Height = 100,
        };

        #endregion Bar1

        #region Bar2

        private Bar chart4;

        public async Task Bar2()
        {
            var context = DbFactory.CreateDbContext();

            object[] data1 = BuildReuqest(context.RequestInfo.AsQueryable())
                .AsEnumerable()
                .GroupBy(s => s.InstanceName)
                .OrderByDescending(s => s.Where(s => s.TraceMilliseconds > 300).Count())
                .Take(4)
                .Select(s => new { value = s.Key, type = s.Where(s => s.TraceMilliseconds > 300).Count() })
                .ToArray();

            if (chart4 != null)
            {
                if (data1.Count() == 0)
                {
                    data1 = new object[] { new { value = "无", type = 0 } };
                }
                await chart4.UpdateChart(csData: data1);
            }
        }

        private readonly BarConfig config4 = new BarConfig
        {
            XField = "type",
            YField = "value",
            Label = new BarViewConfigLabel
            {
                Visible = true,
                Position = "middle", // options= left / middle / right
                AdjustColor = true
            },
            Height = 100,
            Color = "#EBEB7A"
        };

        #endregion Bar2

        #region Bar3

        private Bar chart5;

        public async Task Bar3()
        {
            var context = DbFactory.CreateDbContext();

            var linq = from request in context.RequestInfo
                       join requestDetail in context.RequestDetail
                       on request.Id equals requestDetail.RequestId
                       where requestDetail.ErrorStack != ""
                       select request;

            object[] data1 = BuildReuqest(linq)
                .GroupBy(s => s.InstanceName)
                .OrderByDescending(s => s.Count())
                .Take(4)
                .Select(s => new { value = s.Key, type = s.Count() })
                .ToArray();

            if (chart5 != null)
            {
                if (data1.Count() == 0)
                {
                    data1 = new object[] { new { value = "无", type = 0 } };
                }
                await chart5.UpdateChart(csData: data1);
            }
        }

        private readonly BarConfig config5 = new BarConfig
        {
            XField = "type",
            YField = "value",
            Height = 100,
            Color = "#F5708B",
            Label = new BarViewConfigLabel
            {
                Visible = true,
                Position = "middle", // options= left / middle / right
                AdjustColor = true
            }
        };

        #endregion Bar3

        private IQueryable<RequestInfo> BuildReuqest(IQueryable<RequestInfo> requests)
        {
            var context = DbFactory.CreateDbContext();

            //请求总数
            Expression<Func<RequestInfo, bool>> query = s => s.Id != null;

            if (GlobalSearch.SearchChartBCreateTime != null)
            {
                query = query.And(s => s.CreateTime >= GlobalSearch.SearchChartBCreateTime);
            }

            if (GlobalSearch.SearchChartECreateTime != null)
            {
                query = query.And(s => s.CreateTime <= GlobalSearch.SearchChartECreateTime);
            }

            return requests.Where(query);
        }
    }
}