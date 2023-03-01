using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ServiceReport.Ui.Web.Core.Pages.Dashboard.Components.Head;
using ServiceReport.Ui.Web.Core.Parameter;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Web.Core.Pages.Dashboard
{
    public partial class Index
    {
        [Inject] private ILogger<Index> _logger { get; set; }
        [CascadingParameter] protected GlobalSearch GlobalSearch { get; set; }

        private Head head;
        private ServiceReport.Ui.Web.Core.Pages.Dashboard.Components.Body.Body body;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            if (head != null)
            {
                await head.Reload();
            }

            if (body != null)
            {
                await body.Reload();
            }
        }
    }
}