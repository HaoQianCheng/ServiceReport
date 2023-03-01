using AntDesign;
using AntDesign.TableModels;
using DnsClient.Internal;
using Masuit.Tools;
using Masuit.Tools.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceReport.Core.Model;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Web.Core.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.Request.Components.TableList
{
    public partial class RequestTableList
    {
        [Inject] private ILogger<RequestTableList> _logger { get; set; }
        [Inject] private IDbContextFactory<ApplicationContext> DbFactory { get; set; }
        [Parameter] public GlobalSearch GlobalSearch { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        #region Table

        public QueryModel<RequestInfo> QueryModel;

        public ITable Table;
        private bool loading = false;

        private int PageIndex = 1;
        private int PageSize = 15;
        private int TotalCount = 0;

        private IEnumerable<RequestInfo> requests;

        public async Task LoadAsync(IQueryable<RequestInfo> queryable)
        {
            loading = true;

            requests = await queryable
                .Skip(PageSize * (PageIndex - 1))
                .Take(PageSize)
                .ToListAsync();

            TotalCount = await queryable.CountAsync();

            loading = false;
        }

        public async Task<IQueryable<RequestInfo>> BuildAsync()
        {
            var context = await DbFactory.CreateDbContextAsync();

            var queryable = context.RequestInfo.AsQueryable();

            try
            {
                Expression<Func<RequestInfo, bool>> query = s => s.Id != null;

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

        private async Task OnChange(QueryModel<RequestInfo> queryModel)
        {
            QueryModel = queryModel;

            PageIndex = queryModel.PageIndex;
            PageSize = queryModel.PageSize;

            IQueryable<RequestInfo> queryable = await BuildAsync();

            queryable = queryModel.ExecuteQuery(queryable);

            await LoadAsync(queryable);
        }

        #endregion Table

        #region ³éÌë

        private RequestInfo DrawerInfolData = new RequestInfo();
        private RequestDetail DrawerDetailData = new RequestDetail();

        private bool visible = false;

        private async Task DrawerOpenAsync(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return;
            }

            this.visible = true;

            var context = await DbFactory.CreateDbContextAsync();

            DrawerInfolData = requests
                .Where(s => s.Id.Equals(Id))
                .FirstOrDefault();

            if (DrawerInfolData == null)
            {
                this.visible = false;
                return;
            }

            DrawerDetailData = await context.RequestDetail
                .Where(s => s.RequestId.Equals(Id))
                .FirstOrDefaultAsync();
        }

        private void DrawerClose()
        {
            this.visible = false;
        }

        #endregion ³éÌë
    }
}