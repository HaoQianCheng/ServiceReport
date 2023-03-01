using Masuit.Tools.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServiceReport.Core.Model;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Web.Core.Parameter;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.Dashboard.Components.Head
{
    public partial class Head : ComponentBase
    {
        [Parameter]
        public GlobalSearch GlobalSearch { get; set; }

        [Inject] private IDbContextFactory<ApplicationContext> DbFactory { get; set; }

        public int RequestCounts { get; set; }
        public int RequestErrorCounts { get; set; }
        public int InStanceCounts { get; set; }
        public int WarningCounts { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await Init();
        }

        public async Task Reload()
        {
            await Init();
            StateHasChanged();
        }

        public async Task Init()
        {
            var context = await DbFactory.CreateDbContextAsync();

            //请求总数
            Expression<Func<RequestInfo, bool>> Requestquery = s => s.Id != null;

            if (GlobalSearch.SearchChartBCreateTime != null)
            {
                Requestquery = Requestquery.And(s => s.CreateTime >= GlobalSearch.SearchChartBCreateTime);
            }

            if (GlobalSearch.SearchChartECreateTime != null)
            {
                Requestquery = Requestquery.And(s => s.CreateTime <= GlobalSearch.SearchChartECreateTime);
            }

            if (!string.IsNullOrEmpty(GlobalSearch.Instance))
            {
                Requestquery = Requestquery.And(s => s.InstanceName.Equals(GlobalSearch.Instance));
            }

            var requesttQueryable = BuildReuqest(context.RequestInfo.AsQueryable());

            RequestCounts = await requesttQueryable
                .CountAsync();

            //错误总数
            RequestErrorCounts = await (from request in context.RequestInfo.Where(Requestquery)
                                        join requestDetail in context.RequestDetail
                                        on request.Id equals requestDetail.RequestId
                                        where requestDetail.ErrorStack != ""
                                        select request
                                 ).CountAsync();

            //服务数量
            InStanceCounts = await context.RequestInfo
                .Where(Requestquery)
                .GroupBy(s => s.InstanceName)
                .Select(s => s.Key)
                .CountAsync();

            //报警数量
            WarningCounts = await context.MonitorWarning.CountAsync();
        }

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