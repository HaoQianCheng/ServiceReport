using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Core
{
    public class ServiceReportUiOptions
    {
        public MailOptions MailOptions { get; set; }

        public int ExpireDay { get; set; }

        public Storage Storage { get; set; }
    }
    public class Storage
    {
        public string ConnectionString { get; set; }

        public int DeferSecond { get; set; }

        public int DeferThreshold { get; set;}
    }
    public class MailOptions
    {
        /// <summary>
        /// 邮件服务地址
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 邮件服务端口
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// 账户
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 启用加密连接
        /// </summary>
        public bool EnableSsl { get; set; } = false;
    }
}
