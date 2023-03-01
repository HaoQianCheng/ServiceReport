using System;

namespace ServiceReport.Ui.Web.Core.Parameter
{
    public record GlobalSearch
    {
        public string Instance { get; set; }

        public DateTime? SearchChartBCreateTime { get; set; } = Convert.ToDateTime(DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd 00:00:00"));

        public DateTime? SearchChartECreateTime { get; set; } = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 23:59:59"));
    }
}