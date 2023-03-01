using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceReport.Ui.Storage;

public static class ServiceReportUiStorageModuleExtension
{
    /// <summary>
    /// 为API注入配置文件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        //使用数据库
        services.AddDbContextFactory<ApplicationContext>(options =>
        {
            options.UseLazyLoadingProxies().UseMySql(configuration.GetSection("MySqlOption")["ConnectionString"], ServerVersion.Parse("5.5"));
        });
    }
}