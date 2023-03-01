using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ServiceReport.Ui.Storage.Model
{
    public class MonitorTask
    {
        [DisplayName("唯一标识")]
        public int Id { get; set; }

        [Required]
        [DisplayName("优先级")]
        public int Priority { get; set; }

        [Required]
        [DisplayName("任务标题")]
        public string TaskName { get; set; } = string.Empty;

        [DisplayName("说明")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DisplayName("实例")]
        public string InstanceName { get; set; } = string.Empty;

        [Required]
        [DisplayName("报警邮件")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DisplayName("执行频率")]
        public int ExecutionFrequency { get; set; }

        [DisplayName("是否开启")]
        public bool IsOpen { get; set; }

        [DisplayName("超时监控 是否打开")]
        public bool TimeOutIsOpen { get; set; }

        [DisplayName("超时时间设置")]
        public int TimeOutTime { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }
    }
}