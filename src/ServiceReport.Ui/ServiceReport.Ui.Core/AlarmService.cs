using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Quartz.Logging;
using ServiceReport.Ui.Storage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceReport.Ui.Core
{
    public class AlarmService
    {
        private readonly ILogger<AlarmService> _logger;

        private readonly ServiceReportUiOptions _options;

        public AlarmService(ILogger<AlarmService> logger,
            IOptions<ServiceReportUiOptions> options)
        {
            _logger = logger;
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task NotifyEmailAsync(IEnumerable<MonitorTask> monitorTasks)
        {
            if (monitorTasks == null || monitorTasks.Count() == 0 || string.IsNullOrEmpty(_options.MailOptions.Account) || string.IsNullOrEmpty(_options.MailOptions.Password))
            {
                return;
            }

            var emails = monitorTasks.Select(s => s.Email);

            if (emails.Count() == 0)
            {
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ServiceReport", _options.MailOptions.Account));

            foreach (var to in emails)
            {
                message.To.Add(new MailboxAddress("HttpReport", to));
            }

            message.Subject = $"HttpReport - Warning";

            message.Body = new TextPart(TextFormat.Html)
            {
                //Todo
                Text = @"报警"
            };

            try
            {

                using (var client = new SmtpClient())
                {
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Connect(_options.MailOptions.Server, _options.MailOptions.Port, _options.MailOptions.EnableSsl);
                    client.Authenticate(_options.MailOptions.Account, _options.MailOptions.Password);

                    await client.SendAsync(message);

                    await client.DisconnectAsync(true);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Failed to send alert mail：" + ex.Message, ex);
            }
        }
    }
}
