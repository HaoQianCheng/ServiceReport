using AntDesign;
using AntDesign.TableModels;
using Masuit.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceReport.Ui.Storage;
using ServiceReport.Ui.Storage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.MonitorTasks.Components.TableList
{
    public partial class MonitorTableList
    {
        [Inject] private ILogger<MonitorTableList> _logger { get; set; }
        [Inject] private IDbContextFactory<ApplicationContext> DbFactory { get; set; }
        [Inject] private IMessageService _message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        #region Table

        private ITable Table;
        private bool Loading = false;

        private int PageIndex = 1;
        private int PageSize = 15;
        private int TotalCount = 0;

        private IEnumerable<MonitorTask> monitorTasks;

        private async Task LoadAsync(IQueryable<MonitorTask> queryable)
        {
            monitorTasks = await queryable
             .Skip(PageSize * (PageIndex - 1))
             .Take(PageSize)
             .ToListAsync();

            TotalCount = await queryable.CountAsync();
        }

        public async Task OnChangeAsync(QueryModel<MonitorTask> queryModel)
        {
            Loading = true;

            IQueryable<MonitorTask> queryable = await BuildAsync();

            queryable = queryModel.ExecuteQuery(queryable);

            PageIndex = queryModel.PageIndex;
            PageSize = queryModel.PageSize;

            await LoadAsync(queryable);

            Loading = false;
        }

        private async Task<IQueryable<MonitorTask>> BuildAsync()
        {
            var context = await DbFactory.CreateDbContextAsync();

            return context.MonitorTask.AsQueryable();
        }

        #endregion Table

        #region Edit

        private Form<MonitorTask> FormEdit;
        private bool VisibleEdit = false;

        private MonitorTask ModelEdit;

        private async Task OpenEditAsync(int Id)
        {
            if (Id != 0)
            {
                return;
            }

            var context = await DbFactory.CreateDbContextAsync();

            ModelEdit = context.MonitorTask
                .Where(s => s.Id.Equals(Id))
                .FirstOrDefault();

            if (ModelEdit != null)
            {
                VisibleEdit = true;
            }
        }

        private async Task OnFinishEditAsync(EditContext editContext)
        {
            if (ModelEdit != null)
            {
                return;
            }

            var context = await DbFactory.CreateDbContextAsync();

            context.Update(ModelEdit);

            if (await context.SaveChangesAsync() == 0)
            {
                //Output
            }

            VisibleEdit = false;

            await LoadAsync(await BuildAsync());
        }

        #endregion Edit

        #region Add

        private Form<MonitorTask> FormAdd;
        private bool VisibleAdd = false;

        public MonitorTask ModelAdd = new MonitorTask();

        private async Task OpenAddAsync()
        {
            await InitInstanceSelect();
            VisibleAdd = true;
        }

        private async Task OnFinishAsync(EditContext editContext)
        {
            ModelAdd.CreateTime = DateTime.Now;

            var context = await DbFactory.CreateDbContextAsync();

            await context.AddAsync(ModelAdd);

            if (await context.SaveChangesAsync() == 0)
            {
                //Output
            }

            VisibleAdd = false;

            await LoadAsync(await BuildAsync());
        }

        #endregion Add

        #region Delete

        public async Task DeleteAsync(int Id)
        {
            if (Id != 0)
            {
                return;
            }

            var context = await DbFactory.CreateDbContextAsync();

            var model = context.MonitorTask
                .Where(s => s.Id == Id)
                .FirstOrDefault();

            context.Remove(model);

            if (await context.SaveChangesAsync() == 0)
            {
            }

            await LoadAsync(await BuildAsync());

            await _message.Success("删除成功");
        }

        #endregion Delete

        #region Select

        public List<InstanceOption> InstanceSelects { get; set; } = new List<InstanceOption>();

        private async Task InitInstanceSelect()
        {
            var context = DbFactory.CreateDbContext();

            InstanceSelects = await context.RequestInfo
                .Where(s => s.InstanceName != "")
                .GroupBy(s => s.InstanceName)
                .Select(s => new InstanceOption() { Label = s.Key, Value = s.Key })
                .ToListAsync();
        }

        /// <summary>
        /// 执行频率
        /// </summary>
        public List<ExecutionFrequencyOption> ExecutionFrequencySelects { get; set; } = new List<ExecutionFrequencyOption>()
    {
        new ExecutionFrequencyOption{Label="5分钟",Value=5},
        new ExecutionFrequencyOption{Label="10分钟",Value=10 },
        new ExecutionFrequencyOption{Label="15分钟",Value=15},
        new ExecutionFrequencyOption{Label="20分钟",Value=20},
        new ExecutionFrequencyOption{Label="30分钟",Value=30},
        new ExecutionFrequencyOption{Label="60分钟",Value=60}
    };

        private Dictionary<int, Select> Priority = new Dictionary<int, Select>()
        {
            { 1,new Select(){Value="第一级",Color= PresetColor.Red.ToString()}},
            { 2,new Select(){Value="第二级",Color= PresetColor.Pink.ToString()} },
            { 3,new Select(){Value="第三级",Color= PresetColor.Orange.ToString()}},
            { 4,new Select(){Value="第四级",Color= PresetColor.Gold.ToString()}},
            { 5,new Select(){Value="第五级",Color= "default"}},
        };

        public List<PriorityOption> PriorityOptionSelects { get; set; } = new List<PriorityOption>()
        {
            new PriorityOption(){Label="第一级",Value=1},
            new PriorityOption(){Label="第二级",Value=2},
            new PriorityOption(){Label="第三级",Value=3},
            new PriorityOption(){Label="第四级",Value=4},
            new PriorityOption(){Label="第五级",Value=5}
        };

        #endregion Select
    }

    public class Select
    {
        public int Key { get; set; }

        public string Value { get; set; }

        public string Color { get; set; }
    }

    public class ExecutionFrequencyOption
    {
        public string Label { get; set; }

        public int Value { get; set; }
    }

    public class PriorityOption
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    public class InstanceOption
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
}