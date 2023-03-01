using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceReport.Core.Options;
using ServiceReport.Saver.Abstract;
using ServiceReport.Saver.Http.Options;

namespace ServiceReport.Saver.Http
{
    public static class ServiceReportSaverModule
    {
        /// <summary>
        /// 添加Http发送者服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServiceReportSendHttpModule(this IServiceCollection services, IConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = services
                .BuildServiceProvider()
                .GetService<IConfiguration>() ?? throw new ArgumentNullException();
            }

            //Config
            services.AddOptions()
            .Configure<HttpServiceReportSaverOptions>(configuration.GetSection(ServiceReportOptions.Position + ":" + HttpServiceReportSaverOptions.Position));

            //Http Client
            services.AddHttpClient();

            //Http 发送器
            services.AddSingleton<IServiceReportHttpClient, ServiceReportHttpClient>();
            services.AddSingleton<IServiceReportSaver, HttpServiceReportSaver>();

            return services;
        }
    }
}